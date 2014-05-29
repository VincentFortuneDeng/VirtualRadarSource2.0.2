// Copyright © 2012 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml.Serialization;
using InterfaceFactory;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Database;
using VirtualRadar.Interface.Settings;
using VirtualRadar.Interface.WebServer;
using VirtualRadar.Interface.WebSite;
using VirtualRadar.Interface.BaseStation;
using System.Diagnostics;
using System.Threading;

namespace VirtualRadar.Plugin.AircraftTrackLog
{
    /// <summary>
    /// An example plugin that adds a page to the web site output of the server and a link to the aircraft
    /// detail panel.
    /// </summary>
    /// <remarks><para>
    /// If you want to modify this to create your own plugin that adds content to the web site then you
    /// will need to change the name of the plugin returned by the "Id" property and then add your content
    /// to the Web folder.
    /// </para></remarks>
    public class Plugin : IPlugin
    {
        #region Private class - DefaultProvider
        /// <summary>
        /// The default implementation of <see cref="IPluginProvider"/>.
        /// </summary>
        class DefaultProvider : IPluginProvider
        {
            /*public DateTime UtcNow                      { get { return DateTime.UtcNow; } }*/
            public DateTime LocalNow { get { return DateTime.Now; } }
            public IOptionsView CreateOptionsView() { return new WinForms.OptionsView(); }
            /*public bool FileExists(string fileName)     { return File.Exists(fileName); }*/
            /*public long FileSize(string fileName)       { return new FileInfo(fileName).Length; }*/
        }
        #endregion



        // Constant fields
        //private const string SettingsKey = "Settings";


        // Fields
        private Options _Options;

        /// <summary>
        /// The web site that's currently in use.
        /// </summary>
        private IWebSite _WebSite;

        private PluginStartupParameters _PluginStartupParameters;

        private IWebSiteExtender _WebSiteExtender;

        /// <summary>
        /// The feed whose aircraft messages are being recorded in the database.
        /// </summary>
        private IFeed _Feed;

        /// <summary>
        /// The object that different threads synchronise on before using the contents of the fields.
        /// </summary>
        //private object _SyncLock = new object();


        /// <summary>
        /// 记录飞机轨迹日志对象
        /// </summary>
        private ITrackFlightLog _TrackFlightLog;

        /// <summary>
        /// The object that queues messages from BaseStation and relays them to us on a background thread.
        /// </summary>
        private BackgroundThreadQueue<BaseStationMessageEventArgs> _BackgroundThreadMessageQueue;

        //private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        // Properties
        /// <summary>
        /// Gets or sets the provider that abstracts away the environment for testing.
        /// </summary>
        public IPluginProvider Provider { get; set; }

        public string Id { get { return "VirtualRadar.Plugin.AircraftTrackLog"; } }

        public string PluginFolder { get; set; }

        public string Name { get { return "Aircraft Track Log"; } }

        public string Version { get { return "2.0.2"; } }

        public string Status { get; private set; }

        public string StatusDescription { get; private set; }

        public bool HasOptions { get { return true; } }


        // Events
        public event EventHandler StatusChanged;

        protected virtual void OnStatusChanged(EventArgs args)
        {
            if(StatusChanged != null)
                StatusChanged(this, args);
        }

        #region Constructor
        /// <summary>
        /// Creates a new object.
        /// </summary>
        public Plugin()
        {
            Provider = new DefaultProvider();
            
            Status = PluginStrings.Disabled;
        }
        #endregion


        // Updates the status and raises StatusChanged
        private void UpdateStatus()
        {
            //lock(_SyncLock) {
                var feedManager = Factory.Singleton.Resolve<IFeedManager>().Singleton;

                if(!_Options.Enabled) {
                    Status = PluginStrings.Disabled;
                    StatusDescription = null;
                } else if(_Options.ReceiverId == 0) {
                    Status = PluginStrings.EnabledNoReceiver;
                } else if(feedManager.GetByUniqueId(_Options.ReceiverId) == null) {
                    Status = PluginStrings.EnabledBadReceiver;
                } else {
                    Status = PluginStrings.Enabled;
                    StatusDescription = PluginStrings.EnabledDescription;
                }

                OnStatusChanged(EventArgs.Empty);
            //}
        }

        /// <summary>
        /// Called when the background queue pops a message off the queue of messages.
        /// </summary>
        /// <param name="args"></param>
        private void MessageQueue_MessageReceived(BaseStationMessageEventArgs args)
        {
            try {
                TrackFlight(args.Message);
            } catch(ThreadAbortException) {
            } catch(Exception ex) {
                Debug.WriteLine(String.Format("BaseStationDatabaseWriter.Plugin.MessageRelay_MessageReceived caught exception {0}", ex.ToString()));
                Factory.Singleton.Resolve<VirtualRadar.Interface.ILog>().Singleton.WriteLine("Database writer plugin caught exception on message processing: {0}", ex.ToString());
                StatusDescription = String.Format(PluginStrings.ExceptionCaught, ex.Message);
                OnStatusChanged(EventArgs.Empty);
            }
        }

        /// <summary>
        /// Creates database records and updates internal objects to track an aircraft that is currently transmitting messages.
        /// </summary>
        /// <param name="message"></param>
        private void TrackFlight(BaseStationMessage message)
        {
            if(IsTransmissionMessage(message)) {
                var localNow = Provider.LocalNow;

                //记录轨迹日志
                _TrackFlightLog.FileName = localNow.ToString("yyyyMMdd") + "ICAO" + message.Icao24;

                _TrackFlightLog.WriteLine(String.Concat(message.ToBaseStationString(), "\r\n"));

                /*
                log.Info("AircraftID:" + flight.AircraftID + " Callsign:" + flight.Callsign + " FlightID:" + flight.FlightID
                    + " FirstAltitude:" + flight.FirstAltitude + " FirstGroundSpeed:" + flight.FirstGroundSpeed
                    + " FirstLat:" + flight.FirstLat + " FirstLon:" + flight.FirstLon
                    + " FirstTrack:" + flight.FirstTrack + " FirstVerticalRate:" + flight.FirstVerticalRate
                    + " LastAltitude:" + flight.LastAltitude + " LastGroundSpeed:" + flight.LastGroundSpeed
                    + " LastLat:" + flight.LastLat + " LastLon:" + flight.LastLon
                    + " LastTrack:" + flight.LastTrack + " LastVerticalRate:" + flight.LastVerticalRate
                    + " StartTime:" + flight.StartTime + " EndTime:" + flight.EndTime
                    + " NumADSBMsgRec:" + flight.NumADSBMsgRec + " NumPosMsgRec:" + flight.NumPosMsgRec
                    + " NumModeSMsgRec:" + flight.NumModeSMsgRec + " NumAirPosMsgRec:" + flight.NumAirPosMsgRec
                    + " NumAirCallRepMsgRec:" + flight.NumAirCallRepMsgRec);*/
                //log.Info(String.Concat(message.ToBaseStationString(), "\r\n"));
                //log.Logger.

                /*
                 * 7500：非法行为（比如劫机）
                 * 7600：通讯故障
                 * 7700：紧急状况 
                 */
            }
        }

        /// <summary>
        /// Returns true if the message holds values transmitted from a vehicle.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private bool IsTransmissionMessage(BaseStationMessage message)
        {
            return !String.IsNullOrEmpty(message.Icao24) &&
                   message.MessageType == BaseStationMessageType.Transmission &&
                   message.TransmissionType != BaseStationTransmissionType.None;
        }

        /// <summary>
        /// Called when an exception is allowed to bubble up from <see cref="MessageReceived"/>. This never happens,
        /// it's just here because the background thread queue object needs it.
        /// </summary>
        /// <param name="exception"></param>
        private void MessageQueue_ExceptionCaught(Exception exception)
        {

        }

        /// <summary>
        /// Called by the listener when a BaseStation message is received.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void MessageListener_MessageReceived(object sender, BaseStationMessageEventArgs args)
        {
            if(_BackgroundThreadMessageQueue != null) _BackgroundThreadMessageQueue.Enqueue(args);
        }

        #region HookFeed, UnhookFeed
        /// <summary>
        /// Hooks the feed specified in options, unhooking the previous feed if there was one.
        /// </summary>
        private void HookFeed()
        {
            //lock(_SyncLock) {
                var feedManager = Factory.Singleton.Resolve<IFeedManager>().Singleton;
                var feed = feedManager.GetByUniqueId(_Options.ReceiverId);
                if(feed != _Feed) {
                    if(feed != null) {
                        feed.Listener.Port30003MessageReceived += MessageListener_MessageReceived;
                        feed.Listener.SourceChanged += MessageListener_SourceChanged;
                    }

                    _Feed = feed;
                }
            //}
        }

        /// <summary>
        /// Unhooks the current feed if it isn't currently valid.
        /// </summary>
        private void UnhookFeed()
        {
            //lock(_SyncLock) {
                var feedManager = Factory.Singleton.Resolve<IFeedManager>().Singleton;
                var feed = feedManager.GetByUniqueId(_Options.ReceiverId);
                if(feed != _Feed) {
                    if(_Feed != null) {
                        _Feed.Listener.Port30003MessageReceived -= MessageListener_MessageReceived;
                        _Feed.Listener.SourceChanged -= MessageListener_SourceChanged;
                    }

                    _Feed = null;
                }
            //}
        }

        /// <summary>
        /// Called when the listener changes source.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void MessageListener_SourceChanged(object sender, EventArgs args)
        {
            //EndSession();
            //StartSession();
            UpdateStatus();
        }

        /// <summary>
        /// Called when the feed manager reports a change in feeds.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void FeedManager_FeedsChanged(object sender, EventArgs args)
        {
            UnhookFeed();
            //StartSession();
            UpdateStatus();
            HookFeed();
        }
        #endregion
        // Configuration load and save methods
        /// <summary>
        /// Loads the XML stored in the plugin settings and deserialises it into an object. If there are
        /// no settings stored for the plugin then a new <see cref="Options"/> object is returned.
        /// </summary>
        /// <returns>The deserialised <see cref="Options"/> object.</returns>
        /*private Options LoadSettings()
        {
            Options settings = null;

            var pluginSettingsStorage = Factory.Singleton.Resolve<IPluginSettingsStorage>().Singleton;
            var pluginSettings = pluginSettingsStorage.Load();

            var settingsXml = pluginSettings.ReadString(this, SettingsKey);
            if(String.IsNullOrEmpty(settingsXml)) {
                settings = new Options();
            } else {
                var xmlSerialiser = new XmlSerializer(typeof(Options));
                using(var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(settingsXml))) {
                    settings = (Options)xmlSerialiser.Deserialize(memoryStream);
                }
            }

            return settings;
        }*/

        /// <summary>
        /// Serialises the <see cref="Options"/> object passed across and stores it for the plugin.
        /// </summary>
        /// <param name="settings"></param>
        /*private void SaveSettings(Options settings)
        {
            string settingsXml = null;
            using(var memoryStream = new MemoryStream()) {
                var xmlSerialiser = new XmlSerializer(typeof(Options));
                xmlSerialiser.Serialize(memoryStream, settings);
                settingsXml = Encoding.UTF8.GetString(memoryStream.ToArray());
            }

            var pluginSettingsStorage = Factory.Singleton.Resolve<IPluginSettingsStorage>().Singleton;

            var pluginSettings = pluginSettingsStorage.Load();
            pluginSettings.Write(this, SettingsKey, settingsXml);

            pluginSettingsStorage.Save(pluginSettings);
        }*/


        // IPlugin methods
        /// <summary>
        /// This is called by Virtual Radar Server before Startup is called. This is where you can override any
        /// of the Virtual Radar Server implementations of interfaces. Do not do any other work here - Virtual
        /// Radar Server has not yet finished initialising the application.
        /// </summary>
        /// <param name="classFactory"></param>
        public void RegisterImplementations(IClassFactory classFactory)
        {
            classFactory.Register<ITrackFlightLog, TrackFlightLog>();
        }

        /// <summary>
        /// This is called by Virtual Radar Server. This is where you can be assured that Virtual Radar Server
        /// has finished initialising the application and that it is safe to use the interfaces.
        /// </summary>
        /// <param name="parameters">An object carrying details about the application and your plugin's environment.</param>
        public void Startup(PluginStartupParameters parameters)
        {
            //lock(_SyncLock) {
                // Load the settings
                _Options = OptionsStorage.Load(this);
                //_Options = LoadSettings();
                _WebSite = parameters.WebSite;
                _PluginStartupParameters = parameters;
                _TrackFlightLog = Factory.Singleton.Resolve<ITrackFlightLog>().Singleton;

                var feedManager = Factory.Singleton.Resolve<IFeedManager>().Singleton;
                feedManager.FeedsChanged += FeedManager_FeedsChanged;

                // Create the web site extender and initialise it. This adds our content into the web site, see the comments
                // on IWebSiteExtender for more information.
                ApplyOptions();

                _BackgroundThreadMessageQueue = new BackgroundThreadQueue<BaseStationMessageEventArgs>("BaseStationDatabaseWriterMessageQueue");
                _BackgroundThreadMessageQueue.StartBackgroundThread(MessageQueue_MessageReceived, MessageQueue_ExceptionCaught);

                HookFeed();
            //}
        }

        /// <summary>
        /// This is called by Virtual Radar Server when the GUI thread, the only thread where it is safe to make GUI
        /// calls, is started. Most plugins can ignore this, you only need to use it if you need to create objects
        /// that must be started from the GUI thread.
        /// </summary>
        public void GuiThreadStartup()
        {
        }

        /// <summary>
        /// This is called by Virtual Radar Server when the user clicks the OPTIONS button for your plugin. If you set
        /// the <see cref="HasOptions"/> property on this class to false then it will never be called, but it's usually
        /// a good idea to allow the user to enable or disable the plugin at a bare minimum.
        /// </summary>
        public void ShowWinFormsOptionsUI()
        {
            using(var view = Provider.CreateOptionsView()) {
                view.PluginEnabled = _Options.Enabled;
                view.ReceiverId = _Options.ReceiverId;

                if(view.DisplayView()) {
                    _Options.Enabled = view.PluginEnabled;
                    _Options.ReceiverId = view.ReceiverId;
                        if(view.DisplayView()) {
                            _Options.Enabled = view.PluginEnabled;
                            _Options.ReceiverId = view.ReceiverId;

                            OptionsStorage.Save(this, _Options);
                            UnhookFeed();
                            ApplyOptions();
                            HookFeed();
                    }
                }
            }
            // To keep things simple the plugin only has one configuration setting - an enabled switch. Rather than using
            // a form to ask the user for the setting of this we'll just use a standard message box.
            /*_Options.Enabled = MessageBox.Show(
                "启用此插件添加示例内容到网站. 要启用吗?",
                "启用插件",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button1
            ) == DialogResult.Yes;*/

            //OptionsStorage.Save(this, _Options);
            //SaveSettings(_Options);



            //UpdateStatus();
        }

        public void Shutdown()
        {
            if(_BackgroundThreadMessageQueue != null)
                _BackgroundThreadMessageQueue.Dispose();
            // We just want to remove our site root from the web site here
            _WebSiteExtender.Dispose();

            _Options.Enabled = false;
            UpdateStatus();
        }

        #region ApplyOptions
        /// <summary>
        /// Applies the options.
        /// </summary>
        private void ApplyOptions()
        {
            if(_Options.Enabled) {
                _WebSiteExtender = Factory.Singleton.Resolve<IWebSiteExtender>();
                _WebSiteExtender.Enabled = _Options.Enabled;
                _WebSiteExtender.WebRootSubFolder = "Web";
                _WebSiteExtender.InjectContent = @"<script src=""Example/inject.js"" type=""text/javascript""></script>";
                _WebSiteExtender.InjectMapPages();
                _WebSiteExtender.Initialise(_PluginStartupParameters);
            }

            UpdateStatus();
        }

        //OnStatusChanged(EventArgs.Empty);

        #endregion
    }
}
