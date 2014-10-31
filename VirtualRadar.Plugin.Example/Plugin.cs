// Copyright © 2012 onwards, Vincent Deng(邓守海)
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

namespace VirtualRadar.Plugin.Example
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
        // Constant fields
        private const string SettingsKey = "Settings";


        // Fields
        private Settings _Settings;

        private IWebSiteExtender _WebSiteExtender;


        // Properties
        public string Id                    { get { return "VirtualRadar.Plugin.Example"; } }

        public string PluginFolder          { get; set; }

        public string Name                  { get { return "A Working Example"; } }

        public string Version               { get { return "2.0.1"; } }

        public string Status                { get; private set; }

        public string StatusDescription     { get; private set; }

        public bool HasOptions              { get { return true; } }


        // Events
        public event EventHandler StatusChanged;

        protected virtual void OnStatusChanged(EventArgs args)
        {
            if(StatusChanged != null) StatusChanged(this, args);
        }


        // Updates the status and raises StatusChanged
        private void UpdateStatus()
        {
            if(!_Settings.Enabled) {
                Status = "已禁用";
                StatusDescription = null;
            } else {
                Status = "已启用";
                StatusDescription = "已启用, 浏览 /VirtualRadar/Example/Index.html";
            }

            OnStatusChanged(EventArgs.Empty);
        }


        // Configuration load and save methods
        /// <summary>
        /// Loads the XML stored in the plugin settings and deserialises it into an object. If there are
        /// no settings stored for the plugin then a new <see cref="Settings"/> object is returned.
        /// </summary>
        /// <returns>The deserialised <see cref="Settings"/> object.</returns>
        private Settings LoadSettings()
        {
            Settings settings = null;

            var pluginSettingsStorage = Factory.Singleton.Resolve<IPluginSettingsStorage>().Singleton;
            var pluginSettings = pluginSettingsStorage.Load();

            var settingsXml = pluginSettings.ReadString(this, SettingsKey);
            if(String.IsNullOrEmpty(settingsXml)) {
                settings = new Settings();
            } else {
                var xmlSerialiser = new XmlSerializer(typeof(Settings));
                using(var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(settingsXml))) {
                    settings = (Settings)xmlSerialiser.Deserialize(memoryStream);
                }
            }

            return settings;
        }

        /// <summary>
        /// Serialises the <see cref="Settings"/> object passed across and stores it for the plugin.
        /// </summary>
        /// <param name="settings"></param>
        private void SaveSettings(Settings settings)
        {
            string settingsXml = null;
            using(var memoryStream = new MemoryStream()) {
                var xmlSerialiser = new XmlSerializer(typeof(Settings));
                xmlSerialiser.Serialize(memoryStream, settings);
                settingsXml = Encoding.UTF8.GetString(memoryStream.ToArray());
            }

            var pluginSettingsStorage = Factory.Singleton.Resolve<IPluginSettingsStorage>().Singleton;

            var pluginSettings = pluginSettingsStorage.Load();
            pluginSettings.Write(this, SettingsKey, settingsXml);

            pluginSettingsStorage.Save(pluginSettings);
        }


        // IPlugin methods
        /// <summary>
        /// This is called by Virtual Radar Server before Startup is called. This is where you can override any
        /// of the Virtual Radar Server implementations of interfaces. Do not do any other work here - Virtual
        /// Radar Server has not yet finished initialising the application.
        /// </summary>
        /// <param name="classFactory"></param>
        public void RegisterImplementations(IClassFactory classFactory)
        {
        }

        /// <summary>
        /// This is called by Virtual Radar Server. This is where you can be assured that Virtual Radar Server
        /// has finished initialising the application and that it is safe to use the interfaces.
        /// </summary>
        /// <param name="parameters">An object carrying details about the application and your plugin's environment.</param>
        public void Startup(PluginStartupParameters parameters)
        {
            // Load the settings
            _Settings = LoadSettings();

            // Create the web site extender and initialise it. This adds our content into the web site, see the comments
            // on IWebSiteExtender for more information.
            _WebSiteExtender = Factory.Singleton.Resolve<IWebSiteExtender>();
            _WebSiteExtender.Enabled = _Settings.Enabled;
            _WebSiteExtender.WebRootSubFolder = "Web";
            _WebSiteExtender.InjectContent = @"<script src=""Example/inject.js"" type=""text/javascript""></script>";
            _WebSiteExtender.InjectMapPages();
            _WebSiteExtender.Initialise(parameters);

            UpdateStatus();
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
            // To keep things simple the plugin only has one configuration setting - an enabled switch. Rather than using
            // a form to ask the user for the setting of this we'll just use a standard message box.
            _Settings.Enabled = MessageBox.Show(
                "启用此插件添加示例内容到网站. 要启用吗?",
                "启用插件",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button1
            ) == DialogResult.Yes;

            SaveSettings(_Settings);

            if(_WebSiteExtender != null) _WebSiteExtender.Enabled = _Settings.Enabled;

            UpdateStatus();
        }

        public void Shutdown()
        {
            // We just want to remove our site root from the web site here
            _WebSiteExtender.Dispose();

            _Settings.Enabled = false;
            UpdateStatus();
        }
    }
}
