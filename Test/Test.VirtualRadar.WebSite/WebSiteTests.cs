﻿// Copyright © 2014 Honglin(宏林), Vincent Deng(邓守海)
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
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using System.Web;
using HtmlAgilityPack;
using InterfaceFactory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Test.Framework;
using VirtualRadar.Interface;
using VirtualRadar.Interface.BaseStation;
using VirtualRadar.Interface.Database;
using VirtualRadar.Interface.Listener;
using VirtualRadar.Interface.Settings;
using VirtualRadar.Interface.StandingData;
using VirtualRadar.Interface.WebServer;
using VirtualRadar.Interface.WebSite;

namespace Test.VirtualRadar.WebSite
{
    [TestClass]
    public partial class WebSiteTests
    {
        #region TestContext, Fields, TestInitialise etc.
        public TestContext TestContext { get; set; }

        private IClassFactory _OriginalContainer;

        private bool _AutoAttached = false;
        private IWebSite _WebSite;
        private Mock<IWebSiteProvider> _Provider;
        private Mock<IWebServer> _WebServer;
        private Mock<IRequest> _Request;
        private Mock<IResponse> _Response;
        private MemoryStream _OutputStream;
        private Mock<IInstallerSettingsStorage> _InstallerSettingsStorage;
        private InstallerSettings _InstallerSettings;
        private Mock<IConfigurationStorage> _ConfigurationStorage;
        private Configuration _Configuration;
        private Mock<ISimpleAircraftList> _FlightSimulatorAircraftList;
        private List<IAircraft> _FlightSimulatorAircraft;
        private AircraftListAddress _AircraftListAddress;
        private AircraftListFilter _AircraftListFilter;
        private ReportRowsAddress _ReportRowsAddress;
        private Mock<IBaseStationDatabase> _BaseStationDatabase;
        private Mock<IAutoConfigBaseStationDatabase> _AutoConfigBaseStationDatabase;
        private Mock<IImageFileManager> _ImageFileManager;
        private Mock<IAudio> _Audio;
        private List<BaseStationFlight> _DatabaseFlights;
        private BaseStationAircraft _DatabaseAircraft;
        private List<BaseStationFlight> _DatabaseFlightsForAircraft;
        private Mock<IAircraftPictureManager> _AircraftPictureManager;
        private bool _LoadPictureTestParams;
        private string _LoadPictureExpectedIcao;
        private string _LoadPictureExpectedReg;
        private Mock<IStandingDataManager> _StandingDataManager;
        private Mock<ILog> _Log;
        private Mock<IApplicationInformation> _ApplicationInformation;
        private Mock<IAutoConfigPictureFolderCache> _AutoConfigPictureFolderCache;
        private Mock<IDirectoryCache> _DirectoryCache;
        private Mock<IRuntimeEnvironment> _RuntimeEnvironment;
        private Mock<IBundler> _Bundler;
        private Mock<IMinifier> _Minifier;
        private string _ChecksumsFileName;
        private string _WebRoot;
        private List<Mock<IFeed>> _ReceiverPathways;
        private List<Mock<IBaseStationAircraftList>> _BaseStationAircraftLists;
        private List<List<IAircraft>> _AircraftLists;
        private Mock<IFeedManager> _ReceiverManager;
        private ClockMock _Clock;
        private Image _Image;
        private Mock<IAirportDataDotCom> _AirportDataDotCom;
        private WebRequestResult<AirportDataThumbnailsJson> _AirportDataThumbnails;
        private string _AirportDataThumbnailsIcao;
        private string _AirportDataThumbnailsRegistration;
        private int _AirportDataThumbnailsCountThumbnails;
        private Mock<IPolarPlotter> _PolarPlotter;
        private List<PolarPlotSlice> _PolarPlotSlices;
        private Mock<ICallsignParser> _CallsignParser;
        private Dictionary<string, List<string>> _RouteCallsigns;

        // The named colours (Black, Green etc.) don't compare well to the colors returned by Bitmap.GetPixel - e.g.
        // Color.Black == new Color(0, 0, 0) is false even though the ARGB values are equal. Further Color.Green isn't
        // new Color(0, 255, 0). So we declare our own versions of the colours here to make life easier.
        private Color _Black = Color.FromArgb(0, 0, 0);
        private Color _White = Color.FromArgb(255, 255, 255);
        private Color _Red = Color.FromArgb(255, 0, 0);
        private Color _Green = Color.FromArgb(0, 255, 0);
        private Color _Blue = Color.FromArgb(0, 0, 255);
        private Color _Transparent = Color.FromArgb(0, 0, 0, 0);

        [TestInitialize]
        public void TestInitialise()
        {
            _OriginalContainer = Factory.TakeSnapshot();

            _Clock = new ClockMock();

            _Bundler = TestUtilities.CreateMockImplementation<IBundler>();
            _Bundler.Setup(r => r.BundleHtml(It.IsAny<string>(), It.IsAny<string>())).Returns((string requestPath, string html) => html);
            _WebServer = new Mock<IWebServer>() { DefaultValue = DefaultValue.Mock }.SetupAllProperties();

            _Minifier = TestUtilities.CreateMockImplementation<IMinifier>();
            _Minifier.Setup(r => r.MinifyJavaScript(It.IsAny<string>())).Returns((string js) => js);
            _Minifier.Setup(r => r.MinifyCss(It.IsAny<string>())).Returns((string css) => css);

            _Request = new Mock<IRequest>() { DefaultValue = DefaultValue.Mock }.SetupAllProperties();
            _Response = new Mock<IResponse>() { DefaultValue = DefaultValue.Mock }.SetupAllProperties();

            _Request.Setup(m => m.ContentLength64).Returns(() => { return _Request.Object.InputStream == null ? 0L : _Request.Object.InputStream.Length; });

            _OutputStream = new MemoryStream();
            _Response.Setup(m => m.OutputStream).Returns(_OutputStream);

            _InstallerSettingsStorage = TestUtilities.CreateMockImplementation<IInstallerSettingsStorage>();
            _InstallerSettings = new InstallerSettings();
            _InstallerSettingsStorage.Setup(m => m.Load()).Returns(_InstallerSettings);

            _ConfigurationStorage = TestUtilities.CreateMockSingleton<IConfigurationStorage>();
            _Configuration = new Configuration();
            _Configuration.GoogleMapSettings.WebSiteReceiverId = 1;
            _Configuration.GoogleMapSettings.ClosestAircraftReceiverId = 1;
            _ConfigurationStorage.Setup(m => m.Load()).Returns(_Configuration);

            _RuntimeEnvironment = TestUtilities.CreateMockSingleton<IRuntimeEnvironment>();
            _RuntimeEnvironment.Setup(r => r.IsMono).Returns(false);
            _RuntimeEnvironment.Setup(r => r.ExecutablePath).Returns(Path.Combine(TestContext.TestDeploymentDir));

            _ReceiverPathways = new List<Mock<IFeed>>();
            _BaseStationAircraftLists = new List<Mock<IBaseStationAircraftList>>();
            _AircraftLists = new List<List<IAircraft>>();
            _ReceiverManager = FeedHelper.CreateMockFeedManager(_ReceiverPathways, _BaseStationAircraftLists, _AircraftLists, 1, 2);

            _PolarPlotter = TestUtilities.CreateMockInstance<IPolarPlotter>();
            _PolarPlotSlices = new List<PolarPlotSlice>();
            _PolarPlotter.Setup(r => r.TakeSnapshot()).Returns(() => {
                return _PolarPlotSlices;
            });

            _FlightSimulatorAircraftList = new Mock<ISimpleAircraftList>() { DefaultValue = DefaultValue.Mock }.SetupAllProperties();
            _FlightSimulatorAircraft = new List<IAircraft>();
            long of1, of2;
            _FlightSimulatorAircraftList.Setup(m => m.TakeSnapshot(out of1, out of2)).Returns(_FlightSimulatorAircraft);

            _AircraftListAddress = new AircraftListAddress(_Request);
            _AircraftListFilter = new AircraftListFilter();

            _ReportRowsAddress = new ReportRowsAddress();
            _ApplicationInformation = TestUtilities.CreateMockImplementation<IApplicationInformation>();

            _BaseStationDatabase = new Mock<IBaseStationDatabase>() { DefaultValue = DefaultValue.Mock }.SetupAllProperties();
            _AutoConfigBaseStationDatabase = TestUtilities.CreateMockSingleton<IAutoConfigBaseStationDatabase>();
            _AutoConfigBaseStationDatabase.Setup(a => a.Database).Returns(_BaseStationDatabase.Object);

            _DatabaseFlights = new List<BaseStationFlight>();
            _BaseStationDatabase.Setup(db => db.GetCountOfFlights(It.IsAny<SearchBaseStationCriteria>())).Returns(_DatabaseFlights.Count);
            _BaseStationDatabase.Setup(db => db.GetFlights(It.IsAny<SearchBaseStationCriteria>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<bool>())).Returns(_DatabaseFlights);
            _DatabaseFlightsForAircraft = new List<BaseStationFlight>();
            _BaseStationDatabase.Setup(db => db.GetCountOfFlightsForAircraft(It.IsAny<BaseStationAircraft>(), It.IsAny<SearchBaseStationCriteria>())).Returns(_DatabaseFlightsForAircraft.Count);
            _BaseStationDatabase.Setup(db => db.GetFlightsForAircraft(It.IsAny<BaseStationAircraft>(), It.IsAny<SearchBaseStationCriteria>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<bool>())).Returns(_DatabaseFlightsForAircraft);
            _DatabaseAircraft = new BaseStationAircraft();

            _ImageFileManager = TestUtilities.CreateMockImplementation<IImageFileManager>();
            _ImageFileManager.Setup(i => i.LoadFromFile(It.IsAny<string>())).Returns((string fileName) => {
                return Bitmap.FromFile(fileName);
            });

            _Audio = TestUtilities.CreateMockImplementation<IAudio>();
            _AircraftPictureManager = TestUtilities.CreateMockSingleton<IAircraftPictureManager>();
            _AutoConfigPictureFolderCache = TestUtilities.CreateMockSingleton<IAutoConfigPictureFolderCache>();
            _DirectoryCache = new Mock<IDirectoryCache>() { DefaultValue = DefaultValue.Mock }.SetupAllProperties();
            _AutoConfigPictureFolderCache.Setup(a => a.DirectoryCache).Returns(_DirectoryCache.Object);
            _StandingDataManager = TestUtilities.CreateMockSingleton<IStandingDataManager>();
            _Log = TestUtilities.CreateMockSingleton<ILog>();

            _AirportDataThumbnails = new WebRequestResult<AirportDataThumbnailsJson>();
            _AirportDataDotCom = TestUtilities.CreateMockImplementation<IAirportDataDotCom>();
            _AirportDataThumbnailsIcao = null;
            _AirportDataThumbnailsRegistration = null;
            _AirportDataThumbnailsCountThumbnails = 0;
            _AirportDataDotCom.Setup(r => r.GetThumbnails(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>())).Returns((string icao, string reg, int countThumbnails) => {
                _AirportDataThumbnailsIcao = icao;
                _AirportDataThumbnailsRegistration = reg;
                _AirportDataThumbnailsCountThumbnails = countThumbnails;
                return _AirportDataThumbnails;
            });

            _ChecksumsFileName = Path.Combine(TestContext.TestDeploymentDir, "Checksums.txt");
            _WebRoot = Path.Combine(TestContext.TestDeploymentDir, "Web");

            _Image = null;
            _LoadPictureTestParams = false;
            _LoadPictureExpectedIcao = _LoadPictureExpectedReg = null;
            _AircraftPictureManager.Setup(r => r.LoadPicture(_DirectoryCache.Object, It.IsAny<string>(), It.IsAny<string>())).Returns((IDirectoryCache cache, string lpIcao, string lpReg) => {
                if(_LoadPictureTestParams) {
                    Assert.AreEqual(_LoadPictureExpectedIcao, lpIcao);
                    Assert.AreEqual(_LoadPictureExpectedReg, lpReg);
                }
                return _Image;
            });

            _CallsignParser = TestUtilities.CreateMockImplementation<ICallsignParser>();
            _CallsignParser.Setup(r => r.GetAllRouteCallsigns(It.IsAny<string>(), It.IsAny<string>())).Returns((string callsign, string operatorCode) => {
                List<string> result;
                if(callsign == null || !_RouteCallsigns.TryGetValue(callsign, out result)) {
                    result = new List<string>();
                    if(!String.IsNullOrEmpty(callsign)) result.Add(callsign);
                }
                return result;
            });
            _RouteCallsigns = new Dictionary<string,List<string>>();

            // Initialise this last, just in case the constructor uses any of the mocks
            _WebSite = Factory.Singleton.Resolve<IWebSite>();
            _WebSite.FlightSimulatorAircraftList = _FlightSimulatorAircraftList.Object;
            _WebSite.BaseStationDatabase = _BaseStationDatabase.Object;
            _WebSite.StandingDataManager = _StandingDataManager.Object;
            _AutoAttached = false;

            _Provider = new Mock<IWebSiteProvider>() { DefaultValue = DefaultValue.Mock }.SetupAllProperties();
            _Provider.Setup(m => m.UtcNow).Returns(DateTime.UtcNow);
            _Provider.Setup(m => m.DirectoryExists(It.IsAny<string>())).Returns((string folder) => {
                switch(folder.ToUpper()) {
                    case null:          throw new ArgumentNullException();
                    case "NOTEXISTS":   return false;
                    default:            return true;
                }
            });
            _WebSite.Provider = _Provider.Object;
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Factory.RestoreSnapshot(_OriginalContainer);
            if(_OutputStream != null) _OutputStream.Dispose();
            _OutputStream = null;
            if(_Image != null) _Image.Dispose();
        }
        #endregion

        #region Helper Methods - Attach, SendRequest, SendJsonRequest
        /// <summary>
        /// Calls AttachToServer on _WebSite, but only if it has not already been called for this test.
        /// </summary>
        private void Attach()
        {
            if(!_AutoAttached) {
                _WebSite.AttachSiteToServer(_WebServer.Object);
                _AutoAttached = true;
            }
        }

        /// <summary>
        /// Attaches to the webServer with <see cref="Attach"/> and then raises an event on the webServer to simulate a request coming
        /// in for the path and file specified. The request is not flagged as coming from the Internet.
        /// </summary>
        /// <param name="pathAndFile"></param>
        private void SendRequest(string pathAndFile)
        {
            SendRequest(pathAndFile, false);
        }

        /// <summary>
        /// Attaches to the webServer with <see cref="Attach"/> and then raises an event on the webServer to simulate a request coming
        /// in for the path and file specified and with the origin optionally coming from the Internet.
        /// </summary>
        /// <param name="pathAndFile"></param>
        /// <param name="isInternetClient"></param>
        private void SendRequest(string pathAndFile, bool isInternetClient)
        {
            Attach();
            _WebServer.Raise(m => m.RequestReceived += null, RequestReceivedEventArgsHelper.Create(_Request, _Response, pathAndFile, isInternetClient));
        }

        /// <summary>
        /// Attaches to the webServer with <see cref="Attach"/> and then raises an event on the webServer to simulate a request for a
        /// JSON file. The output is parsed into a JSON file of the type specified.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pathAndFile"></param>
        /// <param name="isInternetClient"></param>
        /// <param name="jsonPCallback"></param>
        /// <returns></returns>
        private T SendJsonRequest<T>(string pathAndFile, bool isInternetClient = false, string jsonPCallback = null)
            where T: class
        {
            return (T)SendJsonRequest(typeof(T), pathAndFile, isInternetClient, jsonPCallback);
        }

        /// <summary>
        /// Attaches to the webServer with <see cref="Attach"/> and then raises an event on the webServer to simulate a request for a
        /// JSON file. The output is parsed into a JSON file of the type specified.
        /// </summary>
        /// <param name="jsonType"></param>
        /// <param name="pathAndFile"></param>
        /// <param name="isInternetClient"></param>
        /// <param name="jsonPCallback"></param>
        /// <returns></returns>
        private object SendJsonRequest(Type jsonType, string pathAndFile, bool isInternetClient = false, string jsonPCallback = null)
        {
            _OutputStream.SetLength(0);
            SendRequest(pathAndFile, isInternetClient);

            DataContractJsonSerializer serialiser = new DataContractJsonSerializer(jsonType);
            object result = null;

            var text = Encoding.UTF8.GetString(_OutputStream.ToArray());
            if(!String.IsNullOrEmpty(text)) {
                if(jsonPCallback != null) {
                    var jsonpStart = String.Format("{0}(", jsonPCallback);
                    var jsonpEnd = ")";
                    Assert.IsTrue(text.StartsWith(jsonpStart), text);
                    Assert.IsTrue(text.EndsWith(jsonpEnd), text);

                    text = text.Substring(jsonpStart.Length, text.Length - (jsonpStart.Length + jsonpEnd.Length));
                }

                try {
                    using(var stream = new MemoryStream(Encoding.UTF8.GetBytes(text))) {
                        result = serialiser.ReadObject(stream);
                    }
                } catch(Exception ex) {
                    Assert.Fail(@"Could not parse output stream into JSON file: {0}, text was ""{1}""", ex.Message, text);
                }
            }

            return result;
        }

        private void EnsureFileDoesNotExist(string fileName)
        {
            var path = Path.Combine(TestContext.TestDeploymentDir, fileName);
            if(File.Exists(path)) File.Delete(path);
        }

        private void AssertFilesAreIdentical(string fileName, byte[] expectedContent, string message = "")
        {
            var actualContent = File.ReadAllBytes(Path.Combine(TestContext.TestDeploymentDir, fileName));
            Assert.IsTrue(expectedContent.SequenceEqual(actualContent), message);
        }
        #endregion

        #region Helper Methods - AddBlankDatabaseFlights
        private void AddBlankDatabaseFlights(int count)
        {
            for(var i = 0;i < count;++i) {
                var flight = new BaseStationFlight();
                flight.FlightID = i + 1;

                var aircraft = new BaseStationAircraft();
                aircraft.AircraftID = i + 101;

                flight.Aircraft = aircraft;
                _DatabaseFlights.Add(flight);
            }
        }

        private void AddBlankDatabaseFlightsForAircraft(int count)
        {
            for(var i = 0;i < count;++i) {
                var flight = new BaseStationFlight();
                flight.FlightID = i + 1;
                flight.Aircraft = _DatabaseAircraft;

                _DatabaseFlightsForAircraft.Add(flight);
            }
        }
        #endregion

        #region Helper Methods - BuildUrl
        /// <summary>
        /// Builds a URL for a page and a bunch of query strings.
        /// </summary>
        /// <param name="pageAddress"></param>
        /// <param name="queryStrings"></param>
        /// <returns></returns>
        public static string BuildUrl(string pageAddress, Dictionary<string, string> queryStrings)
        {
            var result = new StringBuilder(pageAddress);
            bool first = true;
            foreach(var keyValue in queryStrings) {
                result.AppendFormat("{0}{1}={2}",
                    first ? '?' : '&',
                    HttpUtility.UrlEncode(keyValue.Key),
                    HttpUtility.UrlDecode(keyValue.Value));
                first = false;
            }

            return result.ToString();
        }
        #endregion

        #region Constructors and Properties
        [TestMethod]
        public void WebSite_Constructor_Initialises_To_Known_State_And_Properties_Work()
        {
            _WebSite = Factory.Singleton.Resolve<IWebSite>();
            Assert.IsNull(_WebSite.WebServer);
            Assert.IsNotNull(_WebSite.Provider);

            TestUtilities.TestProperty(_WebSite, "Provider", _WebSite.Provider, _Provider.Object);
            TestUtilities.TestProperty(_WebSite, "BaseStationDatabase", null, _BaseStationDatabase.Object);
            TestUtilities.TestProperty(_WebSite, "FlightSimulatorAircraftList", null, _FlightSimulatorAircraftList.Object);
            TestUtilities.TestProperty(_WebSite, "StandingDataManager", null, _StandingDataManager.Object);
        }
        #endregion

        #region AttachSiteToServer
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WebSite_AttachSiteToServer_Throws_If_Passed_Null()
        {
            _WebSite.AttachSiteToServer(null);
        }

        [TestMethod]
        public void WebSite_AttachSiteToServer_Sets_Server_Property()
        {
            _WebSite.AttachSiteToServer(_WebServer.Object);
            Assert.AreSame(_WebServer.Object, _WebSite.WebServer);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void WebSite_AttachSiteToServer_Can_Only_Be_Called_Once()
        {
            _WebSite.AttachSiteToServer(_WebServer.Object);
            _WebSite.AttachSiteToServer(new Mock<IWebServer>().Object);
        }

        [TestMethod]
        public void WebSite_AttachSiteToServer_Copes_If_Attached_To_The_Same_Server_Twice()
        {
            _WebSite.AttachSiteToServer(_WebServer.Object);
            _WebSite.AttachSiteToServer(_WebServer.Object);
        }

        [TestMethod]
        public void WebSite_AttachSiteToServer_Sets_Server_Root()
        {
            _WebSite.AttachSiteToServer(_WebServer.Object);
            Assert.AreEqual("/VirtualRadar", _WebServer.Object.Root);
        }

        [TestMethod]
        public void WebSite_AttachSiteToServer_Sets_Server_Port()
        {
            _InstallerSettings.WebServerPort = 9876;
            _WebSite.AttachSiteToServer(_WebServer.Object);
            Assert.AreEqual(9876, _WebServer.Object.Port);
        }

        [TestMethod]
        public void WebSite_AttachSiteToServer_Initialises_Minifier()
        {
            _WebSite.AttachSiteToServer(_WebServer.Object);
            _Minifier.Verify(r => r.Initialise(), Times.Once());
        }

        [TestMethod]
        public void WebSite_AttachSiteToServer_Initialises_Bundler()
        {
            _WebSite.AttachSiteToServer(_WebServer.Object);
            _Bundler.Verify(r => r.AttachToWebSite(_WebSite), Times.Once());
        }

        [TestMethod]
        public void WebSite_AttachSiteToServer_Sets_Correct_Authentication_On_Server()
        {
            _Configuration.WebServerSettings.AuthenticationScheme = AuthenticationSchemes.Anonymous;
            _WebSite.AttachSiteToServer(_WebServer.Object);
            Assert.AreEqual(AuthenticationSchemes.Anonymous, _WebServer.Object.AuthenticationScheme);
        }

        [TestMethod]
        public void WebSite_AttachSiteToServer_Sets_Correct_Map_Mode_For_GoogleMap_html()
        {
            SendRequest("/GoogleMap.html");

            string content = Encoding.UTF8.GetString(_OutputStream.ToArray());
            Assert.IsFalse(content.Contains("__MAP_MODE__"));
            Assert.IsTrue(content.Contains("var _MapMode = MapMode.normal;"));
        }

        [TestMethod]
        public void WebSite_AttachSiteToServer_Sets_Correct_Map_Mode_For_FlightSim_html()
        {
            SendRequest("/FlightSim.html");

            string content = Encoding.UTF8.GetString(_OutputStream.ToArray());
            Assert.IsFalse(content.Contains("__MAP_MODE__"));
            Assert.IsTrue(content.Contains("var _MapMode = MapMode.flightSim;"));
        }

        [TestMethod]
        public void WebSite_AttachSiteToServer_Sets_Correct_Name_For_RegReport_html()
        {
            SendRequest("/RegReport.html");

            string content = Encoding.UTF8.GetString(_OutputStream.ToArray());
            Assert.IsFalse(content.Contains("%%NAME%%"));
            Assert.IsTrue(content.Contains("<script type=\"text/javascript\" src=\"RegReport.js\">"));
        }

        [TestMethod]
        public void WebSite_AttachSiteToServer_Sets_Correct_Name_For_IcaoReport_html()
        {
            SendRequest("/IcaoReport.html");

            string content = Encoding.UTF8.GetString(_OutputStream.ToArray());
            Assert.IsFalse(content.Contains("%%NAME%%"));
            Assert.IsTrue(content.Contains("<script type=\"text/javascript\" src=\"IcaoReport.js\">"));
        }

        [TestMethod]
        public void WebSite_AttachSiteToServer_Sets_Correct_Name_For_DateReport_html()
        {
            SendRequest("/DateReport.html");

            string content = Encoding.UTF8.GetString(_OutputStream.ToArray());
            Assert.IsFalse(content.Contains("%%NAME%%"));
            Assert.IsTrue(content.Contains("<script type=\"text/javascript\" src=\"DateReport.js\">"));
        }

        [TestMethod]
        [DataSource("Data Source='WebSiteTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "AttachSiteToServer$")]
        public void WebSite_AttachSiteToServer_Causes_Pages_To_Be_Served_For_Correct_Addresses()
        {
            ExcelWorksheetData worksheet = new ExcelWorksheetData(TestContext);
            string pathAndFile = worksheet.String("PathAndFile");

            _WebSite.AttachSiteToServer(_WebServer.Object);

            var args = RequestReceivedEventArgsHelper.Create(_Request, _Response, pathAndFile, false);
            _WebServer.Raise(m => m.RequestReceived += null, args);
            Assert.AreEqual(true, args.Handled, pathAndFile);
            Assert.AreEqual(worksheet.String("MimeType"), _Response.Object.MimeType, pathAndFile);
            Assert.AreEqual(worksheet.ParseEnum<ContentClassification>("Classification"), args.Classification);

            args = RequestReceivedEventArgsHelper.Create(_Request, _Response, pathAndFile.ToLower(), false);
            _WebServer.Raise(m => m.RequestReceived += null, args);
            Assert.AreEqual(true, args.Handled, "Lowercase version", pathAndFile);

            args = RequestReceivedEventArgsHelper.Create(_Request, _Response, pathAndFile.ToUpper(), false);
            _WebServer.Raise(m => m.RequestReceived += null, args);
            Assert.AreEqual(true, args.Handled, "Uppercase version", pathAndFile);
        }

        [TestMethod]
        public void WebSite_AttachSiteToServer_Allows_Default_File_System_Site_Pages_To_Be_Served()
        {
            _WebSite.AttachSiteToServer(_WebServer.Object);

            var checksums = ChecksumFile.Load(File.ReadAllText(_ChecksumsFileName));
            if(checksums.Count == 0) throw new InvalidOperationException("The checksum file is either missing or couldn't be parsed");
            foreach(var checksum in checksums) {
                var args = RequestReceivedEventArgsHelper.Create(_Request, _Response, checksum.FileName.Replace("\\", "/"), false);
                var mimeType = MimeType.GetForExtension(Path.GetExtension(checksum.FileName));
                var classification = MimeType.GetContentClassification(mimeType);

                _WebServer.Raise(r => r.RequestReceived += null, args);

                Assert.AreEqual(true, args.Handled, checksum.FileName);
                Assert.AreEqual(checksum.FileSize, _Response.Object.ContentLength, checksum.FileName);
                Assert.AreEqual(HttpStatusCode.OK, _Response.Object.StatusCode, checksum.FileName);
                Assert.AreEqual(mimeType, _Response.Object.MimeType, checksum.FileName);
                Assert.AreEqual(classification, args.Classification, checksum.FileName);
            }
        }

        [TestMethod]
        public void WebSite_AttachSiteToServer_Only_Attaches_Site_Once()
        {
            _WebSite.AttachSiteToServer(_WebServer.Object);

            var checksums = ChecksumFile.Load(File.ReadAllText(_ChecksumsFileName));
            if(checksums.Count == 0) throw new InvalidOperationException("The checksum file is either missing or couldn't be parsed");
            foreach(var checksum in checksums) {
                var args = RequestReceivedEventArgsHelper.Create(_Request, _Response, checksum.FileName.Replace("\\", "/"), false);
                var mimeType = MimeType.GetForExtension(Path.GetExtension(checksum.FileName));
                var classification = MimeType.GetContentClassification(mimeType);

                _WebServer.Raise(r => r.RequestReceived += null, args);
            }

            _Bundler.Verify(r => r.AttachToWebSite(_WebSite), Times.Once());
        }

        [TestMethod]
        public void WebSite_AttachSiteToServer_Compresses_FileContent_Html()
        {
            _WebSite.AttachSiteToServer(_WebServer.Object);

            var checksums = ChecksumFile.Load(File.ReadAllText(_ChecksumsFileName));
            if(checksums.Count == 0) throw new InvalidOperationException("The checksum file is either missing or couldn't be parsed");
            var checksum = checksums.First(r => Path.GetExtension(r.FileName) == ".html");
            var pathAndFile = checksum.FileName.Replace("\\", "/");

            var args = RequestReceivedEventArgsHelper.Create(_Request, _Response, pathAndFile, false);
            _WebServer.Raise(r => r.RequestReceived += null, args);

            _Response.Verify(r => r.EnableCompression(_Request.Object), Times.Once());
        }

        [TestMethod]
        public void WebSite_AttachSiteToServer_Passes_Html_Through_Bundler()
        {
            _WebSite.AttachSiteToServer(_WebServer.Object);

            var checksums = ChecksumFile.Load(File.ReadAllText(_ChecksumsFileName));
            if(checksums.Count == 0) throw new InvalidOperationException("The checksum file is either missing or couldn't be parsed");
            var checksum = checksums.First(r =>
                Path.GetExtension(r.FileName) == ".html" &&
                File.ReadAllText(r.GetFullPathFromRoot(_WebRoot)).Contains("<!-- [[ JS BUNDLE START ]] -->")
            );
            var pathAndFile = checksum.FileName.Replace("\\", "/");
            var htmlContent = File.ReadAllText(checksum.GetFullPathFromRoot(_WebRoot));
            var newContent = "New Content";
            _Bundler.Setup(r => r.BundleHtml(pathAndFile, htmlContent)).Returns("New Content");
            var expectedBytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(newContent)).ToArray();
            newContent = Encoding.UTF8.GetString(expectedBytes);

            var args = RequestReceivedEventArgsHelper.Create(_Request, _Response, pathAndFile, false);
            _WebServer.Raise(r => r.RequestReceived += null, args);
            var servedContent = Encoding.UTF8.GetString(_OutputStream.ToArray());

            _Bundler.Verify(r => r.BundleHtml(pathAndFile, htmlContent), Times.Once());
            Assert.AreEqual(true, args.Handled);
            Assert.AreEqual(newContent, servedContent);
            Assert.AreEqual(expectedBytes.Length, args.Response.ContentLength);
            Assert.AreEqual(ContentClassification.Html, args.Classification);
            Assert.AreEqual("text/html", args.Response.MimeType);
            Assert.AreEqual(HttpStatusCode.OK, args.Response.StatusCode);
        }

        [TestMethod]
        public void WebSite_AttachSiteToServer_Only_Passes_Html_Through_Bundler()
        {
            _WebSite.AttachSiteToServer(_WebServer.Object);

            var checksums = ChecksumFile.Load(File.ReadAllText(_ChecksumsFileName));
            if(checksums.Count == 0) throw new InvalidOperationException("The checksum file is either missing or couldn't be parsed");
            var checksum = checksums.First(r => Path.GetExtension(r.FileName) == ".js");
            var pathAndFile = checksum.FileName.Replace("\\", "/");

            var args = RequestReceivedEventArgsHelper.Create(_Request, _Response, pathAndFile, false);
            _WebServer.Raise(r => r.RequestReceived += null, args);

            _Bundler.Verify(r => r.BundleHtml(It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        }

        [TestMethod]
        public void WebSite_AttachSiteToServer_Compresses_FileContent_JavaScript()
        {
            _WebSite.AttachSiteToServer(_WebServer.Object);

            var checksums = ChecksumFile.Load(File.ReadAllText(_ChecksumsFileName));
            if(checksums.Count == 0) throw new InvalidOperationException("The checksum file is either missing or couldn't be parsed");
            var checksum = checksums.First(r => Path.GetExtension(r.FileName) == ".js");
            var pathAndFile = checksum.FileName.Replace("\\", "/");

            var args = RequestReceivedEventArgsHelper.Create(_Request, _Response, pathAndFile, false);
            _WebServer.Raise(r => r.RequestReceived += null, args);

            _Response.Verify(r => r.EnableCompression(_Request.Object), Times.Once());
        }

        [TestMethod]
        public void WebSite_AttachSiteToServer_Passes_JavaScript_Through_Minifier()
        {
            _WebSite.AttachSiteToServer(_WebServer.Object);

            var checksums = ChecksumFile.Load(File.ReadAllText(_ChecksumsFileName));
            if(checksums.Count == 0) throw new InvalidOperationException("The checksum file is either missing or couldn't be parsed");
            var checksum = checksums.First(r => Path.GetExtension(r.FileName) == ".js");
            var pathAndFile = checksum.FileName.Replace("\\", "/");
            var jsContent = File.ReadAllText(checksum.GetFullPathFromRoot(_WebRoot));
            var newContent = "New Content";
            _Minifier.Setup(r => r.MinifyJavaScript(jsContent)).Returns(newContent);
            var expectedBytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(newContent)).ToArray();
            newContent = Encoding.UTF8.GetString(expectedBytes);

            var args = RequestReceivedEventArgsHelper.Create(_Request, _Response, pathAndFile, false);
            _WebServer.Raise(r => r.RequestReceived += null, args);
            var servedContent = Encoding.UTF8.GetString(_OutputStream.ToArray());

            _Minifier.Verify(r => r.MinifyJavaScript(jsContent), Times.Once());
            Assert.AreEqual(true, args.Handled);
            Assert.AreEqual(newContent, servedContent);
            Assert.AreEqual(expectedBytes.Length, args.Response.ContentLength);
            Assert.AreEqual(ContentClassification.Other, args.Classification);
            Assert.AreEqual("application/javascript", args.Response.MimeType);
            Assert.AreEqual(HttpStatusCode.OK, args.Response.StatusCode);
        }

        /* NO LONGER MINIFYING CSS - See test to prove that we don't minify CSS :)
        [TestMethod]
        public void WebSite_AttachSiteToServer_Passes_Css_Through_Minifier()
        {
            _WebSite.AttachSiteToServer(_WebServer.Object);

            var checksums = ChecksumFile.Load(File.ReadAllText(_ChecksumsFileName));
            if(checksums.Count == 0) throw new InvalidOperationException("The checksum file is either missing or couldn't be parsed");
            var checksum = checksums.First(r => Path.GetExtension(r.FileName) == ".css");
            var pathAndFile = checksum.FileName.Replace("\\", "/");
            var cssContent = Encoding.UTF8.GetString(File.ReadAllBytes(checksum.GetFullPathFromRoot(_WebRoot)));   // <-- ensures the BOM is preserved
            var newContent = "New Content";
            _Minifier.Setup(r => r.MinifyCss(cssContent)).Returns(newContent);

            var args = RequestReceivedEventArgsHelper.Create(_Request, _Response, pathAndFile, false);
            _WebServer.Raise(r => r.RequestReceived += null, args);
            var servedContent = Encoding.UTF8.GetString(_OutputStream.ToArray());

            _Minifier.Verify(r => r.MinifyCss(cssContent), Times.Once());
            Assert.AreEqual(true, args.Handled);
            Assert.AreEqual(newContent, servedContent);
            Assert.AreEqual(newContent.Length, args.Response.ContentLength);
            Assert.AreEqual(ContentClassification.Other, args.Classification);
            Assert.AreEqual("text/css", args.Response.MimeType);
            Assert.AreEqual(HttpStatusCode.OK, args.Response.StatusCode);
        }
        */

        [TestMethod]
        public void WebSite_AttachSiteToServer_Does_Not_Pass_Css_Through_Minifier()
        {
            // The minifier library I'm using works alright but it breaks jQuery UI's CSS, it stops the icons from
            // appearing. I'm not too fussed about the size of the CSS so I'm letting it through un-mangled.

            _WebSite.AttachSiteToServer(_WebServer.Object);

            var checksums = ChecksumFile.Load(File.ReadAllText(_ChecksumsFileName));
            if(checksums.Count == 0) throw new InvalidOperationException("The checksum file is either missing or couldn't be parsed");
            var checksum = checksums.First(r => Path.GetExtension(r.FileName) == ".css");
            var pathAndFile = checksum.FileName.Replace("\\", "/");

            var args = RequestReceivedEventArgsHelper.Create(_Request, _Response, pathAndFile, false);
            _WebServer.Raise(r => r.RequestReceived += null, args);

            _Minifier.Verify(r => r.MinifyCss(It.IsAny<string>()), Times.Never());
        }

        [TestMethod]
        public void WebSite_AttachSiteToServer_Compresses_FileContent_CSS()
        {
            _WebSite.AttachSiteToServer(_WebServer.Object);

            var checksums = ChecksumFile.Load(File.ReadAllText(_ChecksumsFileName));
            if(checksums.Count == 0) throw new InvalidOperationException("The checksum file is either missing or couldn't be parsed");
            var checksum = checksums.First(r => Path.GetExtension(r.FileName) == ".css");
            var pathAndFile = checksum.FileName.Replace("\\", "/");

            var args = RequestReceivedEventArgsHelper.Create(_Request, _Response, pathAndFile, false);
            _WebServer.Raise(r => r.RequestReceived += null, args);

            _Response.Verify(r => r.EnableCompression(_Request.Object), Times.Once());
        }

        [TestMethod]
        public void WebSite_AttachSiteToServer_Does_Not_Compress_Favicons()
        {
            _WebSite.AttachSiteToServer(_WebServer.Object);

            var checksums = ChecksumFile.Load(File.ReadAllText(_ChecksumsFileName));
            if(checksums.Count == 0) throw new InvalidOperationException("The checksum file is either missing or couldn't be parsed");
            var checksum = checksums.First(r => Path.GetExtension(r.FileName) == ".ico");
            var pathAndFile = checksum.FileName.Replace("\\", "/");

            var args = RequestReceivedEventArgsHelper.Create(_Request, _Response, pathAndFile, false);
            _WebServer.Raise(r => r.RequestReceived += null, args);

            _Response.Verify(r => r.EnableCompression(_Request.Object), Times.Never());
        }

        [TestMethod]
        public void WebSite_AttachSiteToServer_Does_Not_Compress_Images()
        {
            _WebSite.AttachSiteToServer(_WebServer.Object);

            var checksums = ChecksumFile.Load(File.ReadAllText(_ChecksumsFileName));
            if(checksums.Count == 0) throw new InvalidOperationException("The checksum file is either missing or couldn't be parsed");
            var checksum = checksums.First(r => Path.GetExtension(r.FileName) == ".gif");
            var pathAndFile = checksum.FileName.Replace("\\", "/");

            var args = RequestReceivedEventArgsHelper.Create(_Request, _Response, pathAndFile, false);
            _WebServer.Raise(r => r.RequestReceived += null, args);

            _Response.Verify(r => r.EnableCompression(_Request.Object), Times.Never());
        }

        [TestMethod]
        public void WebSite_AttachSiteToServer_Compresses_Other_File_Types()
        {
            _WebSite.AttachSiteToServer(_WebServer.Object);

            var checksums = ChecksumFile.Load(File.ReadAllText(_ChecksumsFileName));
            if(checksums.Count == 0) throw new InvalidOperationException("The checksum file is either missing or couldn't be parsed");
            var checksum = checksums.First(r => !(new string[] { ".gif", ".ico", ".html", ".htm", ".js", ".css" }).Contains(Path.GetExtension(r.FileName)));
            var pathAndFile = checksum.FileName.Replace("\\", "/");

            var args = RequestReceivedEventArgsHelper.Create(_Request, _Response, pathAndFile, false);
            _WebServer.Raise(r => r.RequestReceived += null, args);

            _Response.Verify(r => r.EnableCompression(_Request.Object), Times.Once());
        }

        [TestMethod]
        public void WebSite_AttachSiteToServer_Will_Not_Serve_Files_From_Default_Site_That_Do_Not_Match_Checksum()
        {
            var checksums = ChecksumFile.Load(File.ReadAllText(_ChecksumsFileName));
            var checksum = checksums.First(r => r.FileName.EndsWith(".js"));
            var checksumFile = checksum.GetFullPathFromRoot(_WebRoot);
            var backupCopyFileName = String.Format("{0}.backup", checksumFile);
            File.Copy(checksumFile, backupCopyFileName);
            try {
                File.AppendAllText(checksumFile, "\r\n");

                _WebSite.AttachSiteToServer(_WebServer.Object);
                var args = RequestReceivedEventArgsHelper.Create(_Request, _Response, checksum.FileName.Replace("\\", "/"), false);
                _WebServer.Raise(r => r.RequestReceived += null, args);

                Assert.AreEqual(true, args.Handled);
                Assert.AreEqual(HttpStatusCode.BadRequest, args.Response.StatusCode);
                Assert.AreEqual(0, _OutputStream.Length);
            } finally {
                File.Copy(backupCopyFileName, checksumFile, overwrite: true);
                File.Delete(backupCopyFileName);
            }
        }

        [TestMethod]
        public void WebSite_AttachSiteToServer_Will_Serve_Explanatory_Page_If_HTML_Is_Altered()
        {
            var checksums = ChecksumFile.Load(File.ReadAllText(_ChecksumsFileName));
            var checksum = checksums.First(r => r.FileName.EndsWith(".html"));
            var checksumFile = checksum.GetFullPathFromRoot(_WebRoot);
            var backupCopyFileName = String.Format("{0}.backup", checksumFile);
            File.Copy(checksumFile, backupCopyFileName);
            try {
                File.AppendAllText(checksumFile, "\r\n");

                _WebSite.AttachSiteToServer(_WebServer.Object);
                var args = RequestReceivedEventArgsHelper.Create(_Request, _Response, checksum.FileName.Replace("\\", "/"), false);
                _WebServer.Raise(r => r.RequestReceived += null, args);

                Assert.AreEqual(true, args.Handled);
                Assert.AreEqual(HttpStatusCode.OK, args.Response.StatusCode);
                Assert.AreEqual("text/html", args.Response.MimeType);
                Assert.AreEqual(ContentClassification.Html, args.Classification);
                Assert.AreNotEqual(checksum.FileSize, _OutputStream.Length);
            } finally {
                File.Copy(backupCopyFileName, checksumFile, overwrite: true);
                File.Delete(backupCopyFileName);
            }
        }

        [TestMethod]
        public void WebSite_AttachSiteToServer_Will_Not_Serve_Files_Above_The_Site_Root()
        {
            _WebSite.AttachSiteToServer(_WebServer.Object);
            var args = RequestReceivedEventArgsHelper.Create(_Request, _Response, "/../DLH.BMP", false);
            _WebServer.Raise(m => m.RequestReceived += null, args);
            Assert.AreEqual(false, args.Handled);
        }

        [TestMethod]
        [DataSource("Data Source='WebSiteTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "AttachSiteToServer$")]
        public void WebSite_AttachSiteToServer_Only_Hooks_The_RequestReceived_Event()
        {
            ExcelWorksheetData worksheet = new ExcelWorksheetData(TestContext);
            string pathAndFile = worksheet.String("PathAndFile");

            _WebSite.AttachSiteToServer(_WebServer.Object);

            var args = RequestReceivedEventArgsHelper.Create(_Request, _Response, pathAndFile, false);

            _WebServer.Raise(m => m.BeforeRequestReceived += null, args);
            Assert.AreEqual(false, args.Handled, pathAndFile);

            _WebServer.Raise(m => m.AfterRequestReceived += null, args);
            Assert.AreEqual(false, args.Handled, pathAndFile);
        }

        [TestMethod]
        [DataSource("Data Source='WebSiteTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "AttachSiteToServer$")]
        public void WebSite_AttachSiteToServer_Does_Not_Send_More_Data_On_Requests_That_Have_Already_Been_Handled()
        {
            ExcelWorksheetData worksheet = new ExcelWorksheetData(TestContext);
            string pathAndFile = worksheet.String("PathAndFile");

            _WebSite.AttachSiteToServer(_WebServer.Object);

            var args = RequestReceivedEventArgsHelper.Create(_Request, _Response, pathAndFile, false);
            args.Handled = true;

            _WebServer.Raise(m => m.RequestReceived += null, args);
            Assert.AreEqual(0, _OutputStream.Length);
        }

        [TestMethod]
        [DataSource("Data Source='WebSiteTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "DefaultPage$")]
        public void WebSite_AttachSiteToServer_Causes_Correct_Default_Page_To_Be_Served()
        {
            ExcelWorksheetData worksheet = new ExcelWorksheetData(TestContext);

            _Configuration.GoogleMapSettings.ProxyType = worksheet.ParseEnum<ProxyType>("ProxyType");

            _WebSite.AttachSiteToServer(_WebServer.Object);
            _WebServer.Object.Root = worksheet.String("Root");

            //if(!worksheet.NBool("Isolate").GetValueOrDefault()) return;

            var localEndPointAddress = worksheet.String("LocalEndPoint");
            var localIPAddress = localEndPointAddress.Split(':')[0];
            var localIPPort = int.Parse(localEndPointAddress.Split(':')[1]);
            var localEndPoint = new IPEndPoint(IPAddress.Parse(localIPAddress), localIPPort);

            var endPoint = new IPEndPoint(IPAddress.Parse("192.168.0.200"), 1234);
            var url = new Uri(worksheet.String("Url"));
            _Request.Setup(r => r.RemoteEndPoint).Returns(endPoint);
            _Request.Setup(r => r.LocalEndPoint).Returns(localEndPoint);
            _Request.Setup(r => r.Url).Returns(url);
            _Request.Setup(r => r.RawUrl).Returns(worksheet.String("RawUrl"));
            _Request.Setup(r => r.UserHostName).Returns(worksheet.String("UserHostName"));

            if(worksheet.Bool("IsAndroid")) RequestReceivedEventArgsHelper.SetAndroidUserAgent(_Request);
            if(worksheet.Bool("IsIPad")) RequestReceivedEventArgsHelper.SetIPadUserAgent(_Request);
            if(worksheet.Bool("IsIPhone")) RequestReceivedEventArgsHelper.SetIPhoneUserAgent(_Request);
            if(worksheet.Bool("IsIPod")) RequestReceivedEventArgsHelper.SetIPodUserAgent(_Request);

            var args = new RequestReceivedEventArgs(_Request.Object, _Response.Object, _WebServer.Object.Root);
            _WebServer.Raise(m => m.RequestReceived += null, args);
            Assert.AreEqual(worksheet.Bool("Handled"), args.Handled);

            if(worksheet.String("RedirectUrl") == null) _Response.Verify(r => r.Redirect(It.IsAny<string>()), Times.Never());
            else {
                _Response.Verify(r => r.Redirect(It.IsAny<string>()), Times.Once());
                _Response.Verify(r => r.Redirect(worksheet.String("RedirectUrl")), Times.Once());
                Assert.AreEqual(0L, _OutputStream.Length);
            }
        }
        #endregion

        #region AddSiteRoot
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WebSite_AddSiteRoot_Throws_If_SiteRoot_Is_Null()
        {
            _WebSite.AddSiteRoot(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void WebSite_AddSiteRoot_Throws_If_SiteRoot_Folder_Is_Null()
        {
            _WebSite.AddSiteRoot(new SiteRoot() { Folder = null });
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void WebSite_AddSiteRoot_Throws_If_SiteRoot_Folder_Is_Empty()
        {
            _WebSite.AddSiteRoot(new SiteRoot() { Folder = "" });
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void WebSite_AddSiteRoot_Throws_If_Folder_Does_Not_Exist()
        {
            _WebSite.AddSiteRoot(new SiteRoot() { Folder = @"C:\DOES-NOT-EXIST" });
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void WebSite_AddSiteRoot_Throws_If_Folder_Is_Relative()
        {
            _WebSite.AddSiteRoot(new SiteRoot() { Folder = "." });
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void WebSite_AddSiteRoot_Throws_If_Folder_Is_Already_A_Site_Root()
        {
            _WebSite.AddSiteRoot(new SiteRoot() { Folder = TestContext.TestDeploymentDir });
            _WebSite.AddSiteRoot(new SiteRoot() { Folder = TestContext.TestDeploymentDir });
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void WebSite_AddSiteRoot_Is_Not_Case_Sensitive_When_Searching_For_Duplicate_Folders()
        {
            _WebSite.AddSiteRoot(new SiteRoot() { Folder = TestContext.TestDeploymentDir.ToUpper() });
            _WebSite.AddSiteRoot(new SiteRoot() { Folder = TestContext.TestDeploymentDir.ToLower() });
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void WebSite_AddSiteRoot_Flattens_Traversal_Folders_When_Searching_For_Duplicate_Folders()
        {
            var anotherWayToFolder = String.Format(@"{0}\..\{1}", TestContext.TestDeploymentDir, Path.GetFileName(TestContext.TestDeploymentDir));
            _WebSite.AddSiteRoot(new SiteRoot() { Folder = TestContext.TestDeploymentDir });
            _WebSite.AddSiteRoot(new SiteRoot() { Folder = anotherWayToFolder });
        }

        [TestMethod]
        public void WebSite_AddSiteRoot_Adds_Trailing_Directory_Separator_To_Folder()
        {
            var folder = TestContext.TestDeploymentDir;
            if(folder[folder.Length - 1] == '\\') folder = folder.Substring(0, folder.Length - 1);

            _WebSite.AddSiteRoot(new SiteRoot() { Folder = folder });

            var folders = _WebSite.GetSiteRootFolders();
            Assert.IsFalse(folders.Any(r => r.ToLower() == folder.ToLower()));
            Assert.IsTrue(folders.Any(r => r.ToLower() == (folder + '\\').ToLower()));
        }

        [TestMethod]
        public void WebSite_AddSiteRoot_Can_Serve_Unprotected_Content()
        {
            var checksums = ChecksumFile.Load(File.ReadAllText(_ChecksumsFileName));
            var checksum = checksums.First(r => Path.GetExtension(r.FileName).ToLower() == ".html" && r.FileName.Count(i => i == '\\') == 1);
            var originalFileName = checksum.GetFullPathFromRoot(_WebRoot);
            var newFileName = Path.Combine(TestContext.TestDeploymentDir, Path.GetFileName(originalFileName));
            File.Copy(originalFileName, newFileName);
            try {
                File.AppendAllText(newFileName, "I-ADDED-THIS-TEXT!!");

                for(var i = 0;i < 2;++i) {
                    var shouldServeCustomContent = i == 0;
                    var siteRoot = new SiteRoot() { Folder = TestContext.TestDeploymentDir, Priority = shouldServeCustomContent ? -1 : 1 };
                    _WebSite.AddSiteRoot(siteRoot);

                    SendRequest(checksum.FileName.Replace('\\', '/'));

                    var text = Encoding.UTF8.GetString(_OutputStream.ToArray());
                    var isCustomContent = text.Contains("I-ADDED-THIS-TEXT!!");
                    if(shouldServeCustomContent) Assert.IsTrue(isCustomContent);
                    else                         Assert.IsFalse(isCustomContent);

                    _OutputStream.SetLength(0);
                    _WebSite.RemoveSiteRoot(siteRoot);
                }
            } finally {
                File.Delete(newFileName);
            }
        }
        #endregion

        #region RemoveSiteRoot
        [TestMethod]
        public void WebSite_RemoveSiteRoot_Does_Nothing_If_Passed_Null()
        {
            _WebSite.RemoveSiteRoot(null);
            Assert.AreEqual(1, _WebSite.GetSiteRootFolders().Count);
        }

        [TestMethod]
        public void WebSite_RemoveSiteRoot_Removes_Added_Site()
        {
            var siteRoot = new SiteRoot() { Folder = TestContext.TestDeploymentDir };
            _WebSite.AddSiteRoot(siteRoot);
            _WebSite.RemoveSiteRoot(siteRoot);

            Assert.IsFalse(_WebSite.IsSiteRootActive(siteRoot, false));
            Assert.AreEqual(1, _WebSite.GetSiteRootFolders().Count);
        }

        [TestMethod]
        public void WebSite_RemoveSiteRoot_Ignores_Sites_Not_Added()
        {
            var siteRoot = new SiteRoot() { Folder = TestContext.TestDeploymentDir };
            _WebSite.RemoveSiteRoot(siteRoot);
            Assert.IsFalse(_WebSite.IsSiteRootActive(siteRoot, false));
        }
        #endregion

        #region IsSiteRootActive
        [TestMethod]
        public void WebSite_IsSiteRootActive_Returns_False_If_Passed_Null()
        {
            Assert.IsFalse(_WebSite.IsSiteRootActive(null, false));
        }

        [TestMethod]
        public void WebSite_IsSiteRootActive_Returns_False_If_SiteRoot_Not_Added()
        {
            var siteRoot = new SiteRoot() { Folder = TestContext.TestDeploymentDir };
            Assert.IsFalse(_WebSite.IsSiteRootActive(siteRoot, false));
        }

        [TestMethod]
        public void WebSite_IsSiteRootActive_Returns_True_If_SiteRoot_Added()
        {
            var siteRoot = new SiteRoot() { Folder = TestContext.TestDeploymentDir };
            _WebSite.AddSiteRoot(siteRoot);
            Assert.IsTrue(_WebSite.IsSiteRootActive(siteRoot, false));
        }

        [TestMethod]
        public void WebSite_IsSiteRootActive_Returns_True_If_SiteRoot_Folder_Changed_And_Folders_Are_Ignored()
        {
            var siteRoot = new SiteRoot() { Folder = TestContext.TestDeploymentDir };
            _WebSite.AddSiteRoot(siteRoot);
            siteRoot.Folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            Assert.IsTrue(_WebSite.IsSiteRootActive(siteRoot, false));
        }

        [TestMethod]
        public void WebSite_IsSiteRootActive_Returns_True_If_SiteRoot_Folder_Changed_And_Folders_Are_Significant()
        {
            var siteRoot = new SiteRoot() { Folder = TestContext.TestDeploymentDir };
            _WebSite.AddSiteRoot(siteRoot);
            siteRoot.Folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            Assert.IsFalse(_WebSite.IsSiteRootActive(siteRoot, true));
        }

        [TestMethod]
        public void WebSite_IsSiteRootActive_Returns_True_If_Changed_Folder_Points_To_Original_Folder()
        {
            var siteRoot = new SiteRoot() { Folder = TestContext.TestDeploymentDir };
            _WebSite.AddSiteRoot(siteRoot);
            siteRoot.Folder = Path.Combine(siteRoot.Folder, String.Format(@"..\{0}", Path.GetFileName(siteRoot.Folder)));
            Assert.IsTrue(_WebSite.IsSiteRootActive(siteRoot, true));
        }
        #endregion

        #region GetSiteRootFolders
        [TestMethod]
        public void WebSite_GetSiteRootFolders_Includes_Default_Folder()
        {
            var folders = _WebSite.GetSiteRootFolders();
            Assert.AreEqual(1, folders.Count);
            Assert.AreEqual(1, folders.Count(r => r.ToLower() == (_WebRoot + '\\').ToLower()));
        }

        [TestMethod]
        public void WebSite_GetSiteRootFolders_Includes_Sites_Added_In_Order_Of_Priority()
        {
            for(var i = 0;i < 4;++i) {
                var expectCustomFirst = i % 2 == 0;
                var customFolder = TestContext.TestDeploymentDir + '\\';
                var siteRoot = new SiteRoot() { Folder = customFolder, Priority = expectCustomFirst ? -1 : 1 };
                _WebSite.AddSiteRoot(siteRoot);

                var folders = _WebSite.GetSiteRootFolders();
                Assert.AreEqual(2, folders.Count);
                var customIsFirst = folders[0].ToLower() == customFolder.ToLower();

                Assert.AreEqual(expectCustomFirst, customIsFirst);
                _WebSite.RemoveSiteRoot(siteRoot);
            }
        }
        #endregion

        #region AddHtmlContentInjector
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WebSite_AddHtmlContentInjector_Throws_If_Passed_Null()
        {
            _WebSite.AddHtmlContentInjector(null);
        }

        [TestMethod]
        public void WebSite_AddHtmlContentInjector_Injects_Content_Into_HTML_At_Start_Of_Element()
        {
            var checksums = ChecksumFile.Load(File.ReadAllText(_ChecksumsFileName));
            var checksum = checksums.First(r => "\\index.html".Equals(r.FileName, StringComparison.OrdinalIgnoreCase));
            var pathAndFile = checksum.FileName.Replace("\\", "/");

            _WebSite.AddHtmlContentInjector(new HtmlContentInjector() {
                AtStart = true,
                Element = "head",
                PathAndFile = pathAndFile,
                Content = () => "<script src=\"Hello\"></script><script src=\"There\"></script>"
            });

            SendRequest(pathAndFile);
            var output = Encoding.UTF8.GetString(_OutputStream.ToArray());

            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(output);

            var head = htmlDocument.DocumentNode.Descendants("head").Single();
            var firstChild = head.FirstChild;
            Assert.AreEqual("script", firstChild.Name);
            Assert.AreEqual("Hello", firstChild.GetAttributeValue("src", null));

            var nextChild = firstChild.NextSibling;
            Assert.AreEqual("script", nextChild.Name);
            Assert.AreEqual("There", nextChild.GetAttributeValue("src", null));
        }

        [TestMethod]
        public void WebSite_AddHtmlContentInjector_Does_Not_Inject_Content_Into_Other_Html_Files()
        {
            var checksums = ChecksumFile.Load(File.ReadAllText(_ChecksumsFileName));
            var checksum = checksums.First(r => "\\index.html".Equals(r.FileName, StringComparison.OrdinalIgnoreCase));
            var pathAndFile = checksum.FileName.Replace("\\", "/");

            var other = checksums.First(r => "\\settings.html".Equals(r.FileName, StringComparison.OrdinalIgnoreCase));
            var otherPathAndFile = other.FileName.Replace("\\", "/");

            _WebSite.AddHtmlContentInjector(new HtmlContentInjector() {
                AtStart = true,
                Element = "head",
                PathAndFile = pathAndFile,
                Content = () => "<script src=\"Hello\"></script><script src=\"There\"></script>"
            });

            SendRequest(otherPathAndFile);
            var output = Encoding.UTF8.GetString(_OutputStream.ToArray());

            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(output);

            var head = htmlDocument.DocumentNode.Descendants("head").Single();
            var firstChild = head.FirstChild;
            Assert.AreNotEqual("Hello", firstChild.GetAttributeValue("src", null));
        }

        [TestMethod]
        public void WebSite_AddHtmlContentInjector_Injects_Content_Regardless_Of_PathAndFile_Case()
        {
            var checksums = ChecksumFile.Load(File.ReadAllText(_ChecksumsFileName));
            var checksum = checksums.First(r => "\\index.html".Equals(r.FileName, StringComparison.OrdinalIgnoreCase));
            var pathAndFile = checksum.FileName.Replace("\\", "/");
            var otherCasePathAndFile = Char.IsLower(pathAndFile[1]) ? pathAndFile.ToUpper() : pathAndFile.ToLower();

            _WebSite.AddHtmlContentInjector(new HtmlContentInjector() {
                AtStart = true,
                Element = "head",
                PathAndFile = otherCasePathAndFile,
                Content = () => "<script src=\"Hello\"></script>"
            });

            SendRequest(pathAndFile);
            var output = Encoding.UTF8.GetString(_OutputStream.ToArray());

            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(output);

            var node = htmlDocument.DocumentNode.Descendants("head").Single().FirstChild;
            Assert.AreEqual("Hello", node.GetAttributeValue("src", null));
        }

        [TestMethod]
        public void WebSite_AddHtmlContentInjector_Injects_Content_Regardless_Of_Element_Case()
        {
            var checksums = ChecksumFile.Load(File.ReadAllText(_ChecksumsFileName));
            var checksum = checksums.First(r => "\\index.html".Equals(r.FileName, StringComparison.OrdinalIgnoreCase));
            var pathAndFile = checksum.FileName.Replace("\\", "/");
            var headIsLowerCase = File.ReadAllText(checksum.GetFullPathFromRoot(_WebRoot)).Contains("<head>");

            _WebSite.AddHtmlContentInjector(new HtmlContentInjector() {
                AtStart = true,
                Element = headIsLowerCase ? "HEAD" : "head",
                PathAndFile = pathAndFile,
                Content = () => "<script src=\"Hello\"></script>"
            });

            SendRequest(pathAndFile);
            var output = Encoding.UTF8.GetString(_OutputStream.ToArray());

            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(output);

            var node = htmlDocument.DocumentNode.Descendants("head").Single().FirstChild;
            Assert.AreEqual("Hello", node.GetAttributeValue("src", null));
        }

        [TestMethod]
        public void WebSite_AddHtmlContentInjector_Null_PathAndFile_Injects_Content_Into_Every_Html_File()
        {
            var checksums = ChecksumFile.Load(File.ReadAllText(_ChecksumsFileName));
            var checksum = checksums.First(r => "\\index.html".Equals(r.FileName, StringComparison.OrdinalIgnoreCase));
            var pathAndFile = checksum.FileName.Replace("\\", "/");

            _WebSite.AddHtmlContentInjector(new HtmlContentInjector() {
                AtStart = true,
                Element = "head",
                PathAndFile = null,
                Content = () => "<script src=\"Hello\"></script><script src=\"There\"></script>"
            });

            SendRequest(pathAndFile);
            var output = Encoding.UTF8.GetString(_OutputStream.ToArray());

            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(output);

            var head = htmlDocument.DocumentNode.Descendants("head").Single();
            var firstChild = head.FirstChild;
            Assert.AreEqual("Hello", firstChild.GetAttributeValue("src", null));
        }

        [TestMethod]
        public void WebSite_AddHtmlContentInjector_Injects_Content_Into_HTML_At_End_Of_Element()
        {
            var checksums = ChecksumFile.Load(File.ReadAllText(_ChecksumsFileName));
            var checksum = checksums.First(r => "\\index.html".Equals(r.FileName, StringComparison.OrdinalIgnoreCase));
            var pathAndFile = checksum.FileName.Replace("\\", "/");

            _WebSite.AddHtmlContentInjector(new HtmlContentInjector() {
                AtStart = false,
                Element = "body",
                PathAndFile = pathAndFile,
                Content = () => "<div>Hello, this was injected</div><div>And so was this</div>"
            });

            SendRequest(pathAndFile);
            var output = Encoding.UTF8.GetString(_OutputStream.ToArray());

            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(output);

            var head = htmlDocument.DocumentNode.Descendants("body").Single();
            var lastChild = head.ChildNodes.Where(r => r.Name == "div").Last();
            Assert.AreEqual("And so was this", lastChild.InnerText);

            var previous = lastChild.PreviousSibling;
            Assert.AreEqual("div", previous.Name);
            Assert.AreEqual("Hello, this was injected", previous.InnerText);
        }

        [TestMethod]
        public void WebSite_AddHtmlContentInjector_Injects_Into_First_Element_When_Multiple_Are_Present_And_AtStart_Is_True()
        {
            var checksums = ChecksumFile.Load(File.ReadAllText(_ChecksumsFileName));
            var checksum = checksums.First(r => "\\desktop.html".Equals(r.FileName, StringComparison.OrdinalIgnoreCase));
            var pathAndFile = checksum.FileName.Replace("\\", "/");

            _WebSite.AddHtmlContentInjector(new HtmlContentInjector() {
                AtStart = true,
                Element = "div",
                PathAndFile = pathAndFile,
                Content = () => "Hello"
            });

            SendRequest(pathAndFile);
            var output = Encoding.UTF8.GetString(_OutputStream.ToArray());

            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(output);

            var node = htmlDocument.DocumentNode.Descendants("div").First();
            var injected = node.FirstChild;
            Assert.AreEqual("#text", injected.Name);
            Assert.AreEqual("Hello", injected.InnerText);
        }

        [TestMethod]
        public void WebSite_AddHtmlContentInjector_Injects_Into_Last_Element_When_Multiple_Are_Present_And_AtStart_Is_False()
        {
            var checksums = ChecksumFile.Load(File.ReadAllText(_ChecksumsFileName));
            var checksum = checksums.First(r => "\\desktop.html".Equals(r.FileName, StringComparison.OrdinalIgnoreCase));
            var pathAndFile = checksum.FileName.Replace("\\", "/");

            _WebSite.AddHtmlContentInjector(new HtmlContentInjector() {
                AtStart = false,
                Element = "div",
                PathAndFile = pathAndFile,
                Content = () => "Hello"
            });

            SendRequest(pathAndFile);
            var output = Encoding.UTF8.GetString(_OutputStream.ToArray());

            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(output);

            var node = htmlDocument.DocumentNode.Descendants("div").Last();
            var injected = node.LastChild;
            Assert.AreEqual("#text", injected.Name);
            Assert.AreEqual("Hello", injected.InnerText);
        }

        [TestMethod]
        public void WebSite_AddHtmlContentInjector_Does_Nothing_When_Element_Does_Not_Exist()
        {
            var checksums = ChecksumFile.Load(File.ReadAllText(_ChecksumsFileName));
            var checksum = checksums.First(r => "\\index.html".Equals(r.FileName, StringComparison.OrdinalIgnoreCase));
            var pathAndFile = checksum.FileName.Replace("\\", "/");
            var content = Encoding.UTF8.GetString(File.ReadAllBytes(checksum.GetFullPathFromRoot(_WebRoot)));

            _WebSite.AddHtmlContentInjector(new HtmlContentInjector() {
                AtStart = false,
                Element = "IMadeThisUp",
                PathAndFile = pathAndFile,
                Content = () => "Hello"
            });

            SendRequest(pathAndFile);
            var output = Encoding.UTF8.GetString(_OutputStream.ToArray());
            Assert.AreEqual(content, output);
        }

        [TestMethod]
        public void WebSite_AddHtmlContentInjector_Does_Nothing_When_Element_Is_Null()
        {
            var checksums = ChecksumFile.Load(File.ReadAllText(_ChecksumsFileName));
            var checksum = checksums.First(r => "\\index.html".Equals(r.FileName, StringComparison.OrdinalIgnoreCase));
            var pathAndFile = checksum.FileName.Replace("\\", "/");
            var content = Encoding.UTF8.GetString(File.ReadAllBytes(checksum.GetFullPathFromRoot(_WebRoot)));

            _WebSite.AddHtmlContentInjector(new HtmlContentInjector() {
                AtStart = false,
                Element = null,
                PathAndFile = pathAndFile,
                Content = () => "Hello"
            });

            SendRequest(pathAndFile);
            var output = Encoding.UTF8.GetString(_OutputStream.ToArray());
            Assert.AreEqual(content, output);
        }

        [TestMethod]
        public void WebSite_AddHtmlContentInjector_Does_Nothing_When_Element_Is_Empty_String()
        {
            var checksums = ChecksumFile.Load(File.ReadAllText(_ChecksumsFileName));
            var checksum = checksums.First(r => "\\index.html".Equals(r.FileName, StringComparison.OrdinalIgnoreCase));
            var pathAndFile = checksum.FileName.Replace("\\", "/");
            var content = Encoding.UTF8.GetString(File.ReadAllBytes(checksum.GetFullPathFromRoot(_WebRoot)));

            _WebSite.AddHtmlContentInjector(new HtmlContentInjector() {
                AtStart = false,
                Element = "",
                PathAndFile = pathAndFile,
                Content = () => "Hello"
            });

            SendRequest(pathAndFile);
            var output = Encoding.UTF8.GetString(_OutputStream.ToArray());
            Assert.AreEqual(content, output);
        }

        [TestMethod]
        public void WebSite_AddHtmlContentInjector_Does_Nothing_When_Content_Is_Null()
        {
            var checksums = ChecksumFile.Load(File.ReadAllText(_ChecksumsFileName));
            var checksum = checksums.First(r => "\\index.html".Equals(r.FileName, StringComparison.OrdinalIgnoreCase));
            var pathAndFile = checksum.FileName.Replace("\\", "/");
            var content = Encoding.UTF8.GetString(File.ReadAllBytes(checksum.GetFullPathFromRoot(_WebRoot)));

            _WebSite.AddHtmlContentInjector(new HtmlContentInjector() {
                AtStart = false,
                Element = "div",
                PathAndFile = pathAndFile,
                Content = null
            });

            SendRequest(pathAndFile);
            var output = Encoding.UTF8.GetString(_OutputStream.ToArray());
            Assert.AreEqual(content, output);
        }

        [TestMethod]
        public void WebSite_AddHtmlContentInjector_Does_Nothing_When_Content_Returns_Null()
        {
            var checksums = ChecksumFile.Load(File.ReadAllText(_ChecksumsFileName));
            var checksum = checksums.First(r => "\\index.html".Equals(r.FileName, StringComparison.OrdinalIgnoreCase));
            var pathAndFile = checksum.FileName.Replace("\\", "/");
            var content = Encoding.UTF8.GetString(File.ReadAllBytes(checksum.GetFullPathFromRoot(_WebRoot)));

            _WebSite.AddHtmlContentInjector(new HtmlContentInjector() {
                AtStart = false,
                Element = "div",
                PathAndFile = pathAndFile,
                Content = () => null
            });

            SendRequest(pathAndFile);
            var output = Encoding.UTF8.GetString(_OutputStream.ToArray());
            Assert.AreEqual(content, output);
        }

        [TestMethod]
        public void WebSite_AddHtmlContentInjector_Does_Nothing_When_Content_Returns_Empty_String()
        {
            var checksums = ChecksumFile.Load(File.ReadAllText(_ChecksumsFileName));
            var checksum = checksums.First(r => "\\index.html".Equals(r.FileName, StringComparison.OrdinalIgnoreCase));
            var pathAndFile = checksum.FileName.Replace("\\", "/");
            var content = Encoding.UTF8.GetString(File.ReadAllBytes(checksum.GetFullPathFromRoot(_WebRoot)));

            _WebSite.AddHtmlContentInjector(new HtmlContentInjector() {
                AtStart = false,
                Element = "div",
                PathAndFile = pathAndFile,
                Content = () => ""
            });

            SendRequest(pathAndFile);
            var output = Encoding.UTF8.GetString(_OutputStream.ToArray());
            Assert.AreEqual(content, output);
        }

        [TestMethod]
        public void WebSite_AddHtmlContentInjector_Calls_Multiple_AtStart_Injectors_In_Correct_Order()
        {
            var checksums = ChecksumFile.Load(File.ReadAllText(_ChecksumsFileName));
            var checksum = checksums.First(r => "\\index.html".Equals(r.FileName, StringComparison.OrdinalIgnoreCase));
            var pathAndFile = checksum.FileName.Replace("\\", "/");

            _WebSite.AddHtmlContentInjector(new HtmlContentInjector() {
                AtStart = true,
                Element = "body",
                Priority = 1,
                PathAndFile = pathAndFile,
                Content = () => "<p>First</p>"
            });

            _WebSite.AddHtmlContentInjector(new HtmlContentInjector() {
                AtStart = true,
                Element = "body",
                Priority = 2,
                PathAndFile = pathAndFile,
                Content = () => "<p>Second</p>"
            });

            SendRequest(pathAndFile);
            var output = Encoding.UTF8.GetString(_OutputStream.ToArray());

            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(output);

            var body = htmlDocument.DocumentNode.Descendants("body").Single();
            var first = body.FirstChild;
            var second = first.NextSibling;

            Assert.AreEqual("First", first.InnerText);
            Assert.AreEqual("Second", second.InnerText);
        }

        [TestMethod]
        public void WebSite_AddHtmlContentInjector_Calls_Multiple_AtEnd_Injectors_In_Correct_Order()
        {
            var checksums = ChecksumFile.Load(File.ReadAllText(_ChecksumsFileName));
            var checksum = checksums.First(r => "\\index.html".Equals(r.FileName, StringComparison.OrdinalIgnoreCase));
            var pathAndFile = checksum.FileName.Replace("\\", "/");

            _WebSite.AddHtmlContentInjector(new HtmlContentInjector() {
                AtStart = false,
                Element = "body",
                Priority = 1,
                PathAndFile = pathAndFile,
                Content = () => "<p>First</p>"
            });

            _WebSite.AddHtmlContentInjector(new HtmlContentInjector() {
                AtStart = false,
                Element = "body",
                Priority = 2,
                PathAndFile = pathAndFile,
                Content = () => "<p>Second</p>"
            });

            SendRequest(pathAndFile);
            var output = Encoding.UTF8.GetString(_OutputStream.ToArray());

            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(output);

            var body = htmlDocument.DocumentNode.Descendants("body").Single();
            var last = body.LastChild;
            var secondToLast = last.PreviousSibling;

            Assert.AreEqual("First", last.InnerText);
            Assert.AreEqual("Second", secondToLast.InnerText);
        }
        #endregion

        #region RemoveHtmlContentInjector
        [TestMethod]
        public void WebSite_RemoveHtmlContentInjector_Prevents_Injector_From_Injecting_Content()
        {
            var checksums = ChecksumFile.Load(File.ReadAllText(_ChecksumsFileName));
            var checksum = checksums.First(r => "\\index.html".Equals(r.FileName, StringComparison.OrdinalIgnoreCase));
            var pathAndFile = checksum.FileName.Replace("\\", "/");
            var content = Encoding.UTF8.GetString(File.ReadAllBytes(checksum.GetFullPathFromRoot(_WebRoot)));

            var injector = new HtmlContentInjector() {
                AtStart = true,
                Element = "head",
                PathAndFile = pathAndFile,
                Content = () => "<script src=\"Hello\"></script>"
            };

            _WebSite.AddHtmlContentInjector(injector);
            _WebSite.RemoveHtmlContentInjector(injector);

            SendRequest(pathAndFile);
            var output = Encoding.UTF8.GetString(_OutputStream.ToArray());

            Assert.AreEqual(content, output);
        }

        [TestMethod]
        public void WebSite_RemoveHtmlContentInjector_Does_Not_Care_If_Injector_Was_Never_Injected()
        {
            _WebSite.RemoveHtmlContentInjector(new HtmlContentInjector() {
                AtStart = true,
                Element = "head",
                PathAndFile = "/index.html",
                Content = () => "<script src=\"Hello\"></script>"
            });
        }

        [TestMethod]
        public void WebSite_RemoveHtmlContentInjector_Does_Not_Care_If_Injector_Is_Null()
        {
            _WebSite.RemoveHtmlContentInjector(null);
        }
        #endregion

        #region RequestContent
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WebSite_RequestContent_Throws_If_Passed_Null()
        {
            _WebSite.AttachSiteToServer(_WebServer.Object);
            _WebSite.RequestContent(null);
        }

        [TestMethod]
        public void WebSite_RequestContent_Fetches_The_Same_Content_As_A_WebServer_Request()
        {
            _WebSite.AttachSiteToServer(_WebServer.Object);

            var checksums = ChecksumFile.Load(File.ReadAllText(_ChecksumsFileName));
            if(checksums.Count == 0) throw new InvalidOperationException("The checksum file is either missing or couldn't be parsed");
            foreach(var checksum in checksums) {
                var args = RequestReceivedEventArgsHelper.Create(_Request, _Response, checksum.FileName.Replace("\\", "/"), false);
                var mimeType = MimeType.GetForExtension(Path.GetExtension(checksum.FileName));
                var classification = MimeType.GetContentClassification(mimeType);

                _WebSite.RequestContent(args);

                Assert.AreEqual(true, args.Handled, checksum.FileName);
                Assert.AreEqual(checksum.FileSize, _Response.Object.ContentLength, checksum.FileName);
                Assert.AreEqual(HttpStatusCode.OK, _Response.Object.StatusCode, checksum.FileName);
                Assert.AreEqual(mimeType, _Response.Object.MimeType, checksum.FileName);
                Assert.AreEqual(classification, args.Classification, checksum.FileName);
            }
        }
        #endregion

        #region RequestSimpleContent
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WebSite_RequestSimpleContent_Throws_If_Passed_Null()
        {
            _WebSite.AttachSiteToServer(_WebServer.Object);
            _WebSite.RequestSimpleContent(null);
        }

        [TestMethod]
        public void WebSite_RequestSimpleContent_Fetches_The_Same_Content_As_A_WebServer_Request()
        {
            _WebSite.AttachSiteToServer(_WebServer.Object);

            var checksums = ChecksumFile.Load(File.ReadAllText(_ChecksumsFileName));
            if(checksums.Count == 0) throw new InvalidOperationException("The checksum file is either missing or couldn't be parsed");
            foreach(var checksum in checksums) {
                var pathAndFile = checksum.FileName.Replace("\\", "/");
                var filePath = checksum.GetFullPathFromRoot(_WebRoot);

                var simpleContent = _WebSite.RequestSimpleContent(pathAndFile);

                Assert.AreEqual(HttpStatusCode.OK, simpleContent.HttpStatusCode);
                Assert.AreEqual(checksum.FileSize, simpleContent.Content.LongLength);
                Assert.IsTrue(File.ReadAllBytes(filePath).SequenceEqual(simpleContent.Content));
            }
        }

        [TestMethod]
        public void WebSite_RequestSimpleContent_Returns_Correct_Response_For_Missing_Content()
        {
            _WebSite.AttachSiteToServer(_WebServer.Object);
            var simpleContent = _WebSite.RequestSimpleContent("/FILE-THAT-DOES-NOT-EXIST.HTML");

            Assert.AreEqual(HttpStatusCode.NotFound, simpleContent.HttpStatusCode);
            Assert.AreEqual(0, simpleContent.Content.Length);
        }

        [TestMethod]
        public void WebSite_RequestSimpleContent_Returns_Correct_Response_For_Bad_Checksum_Files()
        {
            var checksums = ChecksumFile.Load(File.ReadAllText(_ChecksumsFileName));
            var checksum = checksums.First(r => r.FileName.EndsWith(".js"));
            var checksumFile = checksum.GetFullPathFromRoot(_WebRoot);
            var backupCopyFileName = String.Format("{0}.backup", checksumFile);
            File.Copy(checksumFile, backupCopyFileName);
            try {
                File.AppendAllText(checksumFile, "\r\n");

                _WebSite.AttachSiteToServer(_WebServer.Object);
                var simpleContent = _WebSite.RequestSimpleContent(checksum.FileName.Replace("\\", "/"));

                Assert.AreEqual(HttpStatusCode.BadRequest, simpleContent.HttpStatusCode);
                Assert.AreEqual(0, simpleContent.Content.Length);
            } finally {
                File.Copy(backupCopyFileName, checksumFile, overwrite: true);
                File.Delete(backupCopyFileName);
            }
        }
        #endregion

        #region Authentication
        [TestMethod]
        public void WebSite_Authentication_Accepts_Basic_Authentication_Credentials_From_Configuration()
        {
            _Configuration.WebServerSettings.AuthenticationScheme = AuthenticationSchemes.Basic;
            _Configuration.WebServerSettings.BasicAuthenticationUser = "Deckard";
            _Configuration.WebServerSettings.BasicAuthenticationPasswordHash = new Hash("B26354");
            _WebSite.AttachSiteToServer(_WebServer.Object);
            var args = new AuthenticationRequiredEventArgs("Deckard", "B26354");

            _WebServer.Raise(m => m.AuthenticationRequired += null, args);

            Assert.IsTrue(args.IsAuthenticated);
            Assert.IsTrue(args.IsHandled);
        }

        [TestMethod]
        public void WebSite_Authentication_Rejects_Wrong_User_For_Basic_Authentication()
        {
            _Configuration.WebServerSettings.AuthenticationScheme = AuthenticationSchemes.Basic;
            _Configuration.WebServerSettings.BasicAuthenticationUser = "Deckard";
            _Configuration.WebServerSettings.BasicAuthenticationPasswordHash = new Hash("B26354");
            _WebSite.AttachSiteToServer(_WebServer.Object);
            var args = new AuthenticationRequiredEventArgs("Zhora", "B26354");

            _WebServer.Raise(m => m.AuthenticationRequired += null, args);

            Assert.IsFalse(args.IsAuthenticated);
            Assert.IsTrue(args.IsHandled);
        }

        [TestMethod]
        public void WebSite_Authentication_Rejects_Null_User_For_Basic_Authentication()
        {
            _Configuration.WebServerSettings.AuthenticationScheme = AuthenticationSchemes.Basic;
            _Configuration.WebServerSettings.BasicAuthenticationUser = "Deckard";
            _Configuration.WebServerSettings.BasicAuthenticationPasswordHash = new Hash("B26354");
            _WebSite.AttachSiteToServer(_WebServer.Object);
            var args = new AuthenticationRequiredEventArgs(null, "B26354");

            _WebServer.Raise(m => m.AuthenticationRequired += null, args);

            Assert.IsFalse(args.IsAuthenticated);
            Assert.IsTrue(args.IsHandled);
        }

        [TestMethod]
        public void WebSite_Authentication_Basic_Authentication_User_Is_Not_Case_Sensitive()
        {
            _Configuration.WebServerSettings.AuthenticationScheme = AuthenticationSchemes.Basic;
            _Configuration.WebServerSettings.BasicAuthenticationUser = "Deckard";
            _Configuration.WebServerSettings.BasicAuthenticationPasswordHash = new Hash("B26354");
            _WebSite.AttachSiteToServer(_WebServer.Object);
            var args = new AuthenticationRequiredEventArgs("DECKARD", "B26354");

            _WebServer.Raise(m => m.AuthenticationRequired += null, args);

            Assert.IsTrue(args.IsAuthenticated);
            Assert.IsTrue(args.IsHandled);
        }

        [TestMethod]
        public void WebSite_Authentication_Rejects_Wrong_Password_For_Basic_Authentication()
        {
            _Configuration.WebServerSettings.AuthenticationScheme = AuthenticationSchemes.Basic;
            _Configuration.WebServerSettings.BasicAuthenticationUser = "Deckard";
            _Configuration.WebServerSettings.BasicAuthenticationPasswordHash = new Hash("B26354");
            _WebSite.AttachSiteToServer(_WebServer.Object);
            var args = new AuthenticationRequiredEventArgs("Deckard", "b26354");

            _WebServer.Raise(m => m.AuthenticationRequired += null, args);

            Assert.IsFalse(args.IsAuthenticated);
            Assert.IsTrue(args.IsHandled);
        }

        [TestMethod]
        public void WebSite_Authentication_Ignores_Unknown_Authentication_Schemes()
        {
            foreach(AuthenticationSchemes scheme in Enum.GetValues(typeof(AuthenticationSchemes))) {
                if(scheme == AuthenticationSchemes.Basic) continue;

                _Configuration.WebServerSettings.AuthenticationScheme = scheme;
                _Configuration.WebServerSettings.BasicAuthenticationUser = "Deckard";
                _Configuration.WebServerSettings.BasicAuthenticationPasswordHash = new Hash("B26354");
                _WebSite.AttachSiteToServer(_WebServer.Object);
                var args = new AuthenticationRequiredEventArgs("Deckard", "B26354");

                _WebServer.Raise(m => m.AuthenticationRequired += null, args);

                Assert.IsFalse(args.IsAuthenticated);
                Assert.IsFalse(args.IsHandled);
            }
        }

        [TestMethod]
        public void WebSite_Authentication_Ignores_Events_That_Have_Already_Been_Handled()
        {
            _Configuration.WebServerSettings.AuthenticationScheme = AuthenticationSchemes.Basic;
            _Configuration.WebServerSettings.BasicAuthenticationUser = "Deckard";
            _Configuration.WebServerSettings.BasicAuthenticationPasswordHash = new Hash("B26354");
            _WebSite.AttachSiteToServer(_WebServer.Object);
            var args = new AuthenticationRequiredEventArgs("Deckard", "B26354") { IsHandled = true };

            _WebServer.Raise(m => m.AuthenticationRequired += null, args);

            Assert.IsFalse(args.IsAuthenticated);
        }
        #endregion

        #region Configuration changes
        [TestMethod]
        public void WebSite_Configuration_Changes_Do_Not_Crash_WebSite_If_Server_Not_Attached()
        {
            _ConfigurationStorage.Raise(m => m.ConfigurationChanged += null, EventArgs.Empty);
        }

        [TestMethod]
        public void WebSite_Configuration_Picks_Up_Change_In_Basic_Authentication_User()
        {
            _Configuration.WebServerSettings.AuthenticationScheme = AuthenticationSchemes.Basic;
            _Configuration.WebServerSettings.BasicAuthenticationUser = "JFSebastian";
            _Configuration.WebServerSettings.BasicAuthenticationPasswordHash = new Hash("B26354");
            _WebSite.AttachSiteToServer(_WebServer.Object);
            var args = new AuthenticationRequiredEventArgs("Deckard", "B26354");
            _WebServer.Raise(m => m.AuthenticationRequired += null, args);

            args.IsHandled = false;
            _Configuration.WebServerSettings.BasicAuthenticationUser = "Deckard";
            _ConfigurationStorage.Raise(m => m.ConfigurationChanged += null, EventArgs.Empty);
            _WebServer.Raise(m => m.AuthenticationRequired += null, args);

            Assert.IsTrue(args.IsAuthenticated);
        }

        [TestMethod]
        public void WebSite_Configuration_Picks_Up_Change_In_Basic_Authentication_Password()
        {
            _Configuration.WebServerSettings.AuthenticationScheme = AuthenticationSchemes.Basic;
            _Configuration.WebServerSettings.BasicAuthenticationUser = "Deckard";
            _Configuration.WebServerSettings.BasicAuthenticationPasswordHash = new Hash("16417");
            _WebSite.AttachSiteToServer(_WebServer.Object);
            var args = new AuthenticationRequiredEventArgs("Deckard", "B26354");
            _WebServer.Raise(m => m.AuthenticationRequired += null, args);

            args.IsHandled = false;
            _Configuration.WebServerSettings.BasicAuthenticationPasswordHash = new Hash("B26354");
            _ConfigurationStorage.Raise(m => m.ConfigurationChanged += null, EventArgs.Empty);
            _WebServer.Raise(m => m.AuthenticationRequired += null, args);

            Assert.IsTrue(args.IsAuthenticated);
        }

        [TestMethod]
        public void WebSite_Configuration_Picks_Up_Change_In_Authentication_Scheme()
        {
            _Configuration.WebServerSettings.AuthenticationScheme = AuthenticationSchemes.Anonymous;
            _WebSite.AttachSiteToServer(_WebServer.Object);

            _Configuration.WebServerSettings.AuthenticationScheme = AuthenticationSchemes.Basic;
            _ConfigurationStorage.Raise(m => m.ConfigurationChanged += null, EventArgs.Empty);

            Assert.AreEqual(AuthenticationSchemes.Basic, _WebServer.Object.AuthenticationScheme);
        }

        [TestMethod]
        public void WebSite_Configuration_Restarts_Server_If_Authentication_Scheme_Changes()
        {
            _Configuration.WebServerSettings.AuthenticationScheme = AuthenticationSchemes.Anonymous;
            _WebSite.AttachSiteToServer(_WebServer.Object);
            _WebServer.Object.Online = true;

            _Configuration.WebServerSettings.AuthenticationScheme = AuthenticationSchemes.Basic;
            _ConfigurationStorage.Raise(m => m.ConfigurationChanged += null, EventArgs.Empty);

            _WebServer.VerifySet(m => m.Online = false, Times.Once());
            _WebServer.VerifySet(m => m.Online = true, Times.Exactly(2));
        }

        [TestMethod]
        public void WebSite_IsMono_Correctly_Set_In_ServerConfig_Js_When_Running_Under_Mono()
        {
            _RuntimeEnvironment.Setup(r => r.IsMono).Returns(true);
            SendRequest("/ServerConfig.js");

            string output = Encoding.UTF8.GetString(_OutputStream.ToArray());
            Assert.AreNotEqual(-1, output.IndexOf("this.isMono = true;"));
        }

        [TestMethod]
        public void WebSite_IsMono_Correctly_Set_In_ServerConfig_Js_When_Running_Under_DotNet()
        {
            _RuntimeEnvironment.Setup(r => r.IsMono).Returns(false);
            SendRequest("/ServerConfig.js");

            string output = Encoding.UTF8.GetString(_OutputStream.ToArray());
            Assert.AreNotEqual(-1, output.IndexOf("this.isMono = false;"));
        }

        [TestMethod]
        public void WebSite_Configuration_VrsVersion_Set_In_ServerConfig_Js()
        {
            _ApplicationInformation.Setup(i => i.ShortVersion).Returns("1.2.3");
            SendRequest("/ServerConfig.js");

            string content = Encoding.UTF8.GetString(_OutputStream.ToArray());
            Assert.IsFalse(content.Contains("__VRS_VERSION"));
            Assert.IsTrue(content.Contains("vrsVersion = '1.2.3';"));
        }

        [TestMethod]
        public void WebSite_Configuration_Is_Reflected_In_Output_From_ReportMap_Js()
        {
            SendRequest("/ReportMap.js");

            string output = Encoding.UTF8.GetString(_OutputStream.ToArray());
            Assert.IsFalse(output.Contains("__"));
        }

        [TestMethod]
        public void WebSite_Configuration_Copies_Receiver_Details_To_ServerConfig_Js()
        {
            _Configuration.Receivers.Add(new Receiver() { UniqueId = 1, Name = "R1" });
            _Configuration.Receivers.Add(new Receiver() { UniqueId = 2, Name = "R2" });
            _Configuration.MergedFeeds.Add(new MergedFeed() { UniqueId = 3, Name = "M1" });

            SendRequest("/ServerConfig.json");
            var jsonSerialiser = new DataContractJsonSerializer(typeof(ServerConfigJson));
            _OutputStream.Position = 0;
            var result = (ServerConfigJson)jsonSerialiser.ReadObject(_OutputStream);

            Assert.AreEqual(3, result.Receivers.Count);
            Assert.IsTrue(result.Receivers.Any(r => r.UniqueId == 1 && r.Name == "R1"));
            Assert.IsTrue(result.Receivers.Any(r => r.UniqueId == 2 && r.Name == "R2"));
            Assert.IsTrue(result.Receivers.Any(r => r.UniqueId == 3 && r.Name == "M1"));
        }

        [TestMethod]
        [DataSource("Data Source='WebSiteTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "SubstituteConfiguration$")]
        public void WebSite_Configuration_Changes_Correctly_Substituted_Into_Web_Pages()
        {
            CheckServerConfig((bool isLocalAddress) => {
                _WebServer.Raise(m => m.RequestReceived += null, RequestReceivedEventArgsHelper.Create(_Request, _Response, "/ServerConfig.json", !isLocalAddress));

                var jsonSerialiser = new DataContractJsonSerializer(typeof(ServerConfigJson));
                _OutputStream.Position = 0;
                var result = (ServerConfigJson)jsonSerialiser.ReadObject(_OutputStream);

                return result;
            });
        }

        private void CheckServerConfig(Func<bool, ServerConfigJson> trigger)
        {
            _WebSite.AttachSiteToServer(_WebServer.Object);

            var worksheet = new ExcelWorksheetData(TestContext);
            if(worksheet.String("ConfigProperty") != "CanShowPolarPlots") return;

            var isLocalAddress = true;
            using(var cultureSwitcher = new CultureSwitcher(worksheet.String("Culture"))) {
                var configProperty = worksheet.String("ConfigProperty");
                switch(configProperty) {
                    case "VrsVersion":              _ApplicationInformation.Setup(r => r.ShortVersion).Returns(worksheet.String("Value")); break;
                    case "IsLocalAddress":          isLocalAddress = worksheet.Bool("Value"); break;
                    case "IsMono":                  _RuntimeEnvironment.Setup(r => r.IsMono).Returns(worksheet.Bool("Value")); break;
                    case "InitialMapLatitude":      _Configuration.GoogleMapSettings.InitialMapLatitude = worksheet.Double("Value"); break;
                    case "InitialMapLongitude":     _Configuration.GoogleMapSettings.InitialMapLongitude = worksheet.Double("Value"); break;
                    case "InitialMapType":          _Configuration.GoogleMapSettings.InitialMapType = worksheet.EString("Value"); break;
                    case "InitialMapZoom":          _Configuration.GoogleMapSettings.InitialMapZoom = worksheet.Int("Value"); break;
                    case "InitialRefreshSeconds":   _Configuration.GoogleMapSettings.InitialRefreshSeconds = worksheet.Int("Value"); break;
                    case "MinimumRefreshSeconds":   _Configuration.GoogleMapSettings.MinimumRefreshSeconds = worksheet.Int("Value"); break;
                    case "InitialDistanceUnit":     _Configuration.GoogleMapSettings.InitialDistanceUnit = worksheet.ParseEnum<DistanceUnit>("Value"); break;
                    case "InitialHeightUnit":       _Configuration.GoogleMapSettings.InitialHeightUnit = worksheet.ParseEnum<HeightUnit>("Value"); break;
                    case "InitialSpeedUnit":        _Configuration.GoogleMapSettings.InitialSpeedUnit = worksheet.ParseEnum<SpeedUnit>("Value"); break;
                    case "CanRunReports":           _Configuration.InternetClientSettings.CanRunReports = worksheet.Bool("Value"); break;
                    case "CanShowPinText":          _Configuration.InternetClientSettings.CanShowPinText = worksheet.Bool("Value"); break;
                    case "TimeoutMinutes":          _Configuration.InternetClientSettings.TimeoutMinutes = worksheet.Int("Value"); break;
                    case "CanPlayAudio":            _Configuration.InternetClientSettings.CanPlayAudio = worksheet.Bool("Value"); break;
                    case "CanSubmitRoutes":         _Configuration.InternetClientSettings.CanSubmitRoutes = worksheet.Bool("Value"); break;
                    case "CanShowPictures":         _Configuration.InternetClientSettings.CanShowPictures = worksheet.Bool("Value"); break;
                    case "AudioEnabled":            _Configuration.AudioSettings.Enabled = worksheet.Bool("Value"); break;
                    case "CanShowPolarPlots":       _Configuration.InternetClientSettings.CanShowPolarPlots = worksheet.Bool("Value"); break;
                    default:                        throw new NotImplementedException();
                }
            }

            using(var switcher = new CultureSwitcher(worksheet.String("Culture"))) {
                _ConfigurationStorage.Raise(m => m.ConfigurationChanged += null, EventArgs.Empty);
            }

            var json = trigger(isLocalAddress);

            var propertyName = worksheet.String("ConfigProperty");
            switch(propertyName) {
                case "VrsVersion":                  Assert.AreEqual(worksheet.EString("JsonValue"), json.VrsVersion); break;
                case "IsLocalAddress":              Assert.AreEqual(worksheet.Bool("JsonValue"), json.IsLocalAddress); break;
                case "IsMono":                      Assert.AreEqual(worksheet.Bool("JsonValue"), json.IsMono); break;
                case "InitialMapLatitude":          Assert.AreEqual(worksheet.Double("JsonValue"), json.InitialLatitude); break;
                case "InitialMapLongitude":         Assert.AreEqual(worksheet.Double("JsonValue"), json.InitialLongitude); break;
                case "InitialMapType":              Assert.AreEqual(worksheet.EString("JsonValue"), json.InitialMapType); break;
                case "InitialMapZoom":              Assert.AreEqual(worksheet.Int("JsonValue"), json.InitialZoom); break;
                case "InitialRefreshSeconds":       Assert.AreEqual(worksheet.Int("JsonValue"), json.RefreshSeconds); break;
                case "MinimumRefreshSeconds":       Assert.AreEqual(worksheet.Int("JsonValue"), json.MinimumRefreshSeconds); break;
                case "InitialDistanceUnit":         Assert.AreEqual(worksheet.String("JsonValue"), json.InitialDistanceUnit); break;
                case "InitialHeightUnit":           Assert.AreEqual(worksheet.String("JsonValue"), json.InitialHeightUnit); break;
                case "InitialSpeedUnit":            Assert.AreEqual(worksheet.String("JsonValue"), json.InitialSpeedUnit); break;
                case "CanRunReports":               Assert.AreEqual(worksheet.Bool("JsonValue"), json.InternetClientCanRunReports); break;
                case "CanShowPinText":              Assert.AreEqual(worksheet.Bool("JsonValue"), json.InternetClientCanShowPinText); break;
                case "TimeoutMinutes":              Assert.AreEqual(worksheet.Int("JsonValue"), json.InternetClientTimeoutMinutes); break;
                case "CanPlayAudio":                Assert.AreEqual(worksheet.Bool("JsonValue"), json.InternetClientsCanPlayAudio); break;
                case "CanSubmitRoutes":             Assert.AreEqual(worksheet.Bool("JsonValue"), json.InternetClientsCanSubmitRoutes); break;
                case "CanShowPictures":             Assert.AreEqual(worksheet.Bool("JsonValue"), json.InternetClientsCanSeeAircraftPictures); break;
                case "AudioEnabled":                Assert.AreEqual(worksheet.Bool("JsonValue"), json.IsAudioEnabled); break;
                case "CanShowPolarPlots":           Assert.AreEqual(worksheet.Bool("JsonValue"), json.InternetClientsCanSeePolarPlots); break;
                default:                            throw new NotImplementedException();
            }
        }
        #endregion

        #region IsLocal
        [TestMethod]
        public void WebSite_ServerConfig_Js_Substitutes_IsLocal_Correctly()
        {
            SendRequest("/ServerConfig.js", false);
            var output1 = Encoding.UTF8.GetString(_OutputStream.ToArray());

            SendRequest("/ServerConfig.js", true);
            var output2 = Encoding.UTF8.GetString(_OutputStream.ToArray());

            Assert.IsFalse(output1.Contains("__IS_LOCAL_ADDRESS"));
            Assert.IsTrue(output1.Contains("isLocalAddress = true;"));

            Assert.IsFalse(output2.Contains("__IS_LOCAL_ADDRESS"));
            Assert.IsTrue(output2.Contains("isLocalAddress = false;"));
        }
        #endregion

        #region Audio
        [TestMethod]
        public void WebSite_Audio_Can_Synthesise_Text_Correctly()
        {
            var wavAudio = new byte[] { 0x01, 0x02, 0x03 };
            _Audio.Setup(p => p.SpeechToWavBytes(It.IsAny<string>())).Returns(wavAudio);

            SendRequest("/Audio?cmd=say&line=Hello%20World!");

            _Audio.Verify(a => a.SpeechToWavBytes("Hello World!"), Times.Once());

            Assert.AreEqual(3, _Response.Object.ContentLength);
            Assert.AreEqual(MimeType.WaveAudio, _Response.Object.MimeType);
            Assert.AreEqual(HttpStatusCode.OK, _Response.Object.StatusCode);

            Assert.AreEqual(3, _OutputStream.Length);
            Assert.IsTrue(_OutputStream.ToArray().SequenceEqual(wavAudio));
        }

        [TestMethod]
        public void WebSite_Audio_Ignores_Unknown_Commands()
        {
            SendRequest("/Audio?line=Hello%20World!");

            Assert.AreEqual((HttpStatusCode)0, _Response.Object.StatusCode);
            Assert.AreEqual(0, _Response.Object.ContentLength);
            Assert.AreEqual(0, _OutputStream.Length);
        }

        [TestMethod]
        public void WebSite_Audio_Ignores_Speak_Command_With_No_Speech_Text()
        {
            SendRequest("/Audio?cmd=say");

            Assert.AreEqual((HttpStatusCode)0, _Response.Object.StatusCode);
            Assert.AreEqual(0, _Response.Object.ContentLength);
            Assert.AreEqual(0, _OutputStream.Length);
        }

        [TestMethod]
        public void WebSite_Audio_Honours_Configuration_Options()
        {
            _Audio.Setup(a => a.SpeechToWavBytes(It.IsAny<string>())).Returns(new byte[] { 0x01, 0xff });

            _Configuration.InternetClientSettings.CanPlayAudio = false;

            SendRequest("/Audio?cmd=say&line=whatever", true);
            Assert.AreEqual(0, _OutputStream.Length);

            SendRequest("/Audio?cmd=say&line=whatever", false);
            Assert.AreEqual(2, _OutputStream.Length);
            _OutputStream.SetLength(0);

            _Configuration.InternetClientSettings.CanPlayAudio = true;
            _ConfigurationStorage.Raise(e => e.ConfigurationChanged += null, EventArgs.Empty);

            SendRequest("/Audio?cmd=say&line=whatever", true);
            Assert.AreEqual(2, _OutputStream.Length);
            _OutputStream.SetLength(0);

            SendRequest("/Audio?cmd=say&line=whatever", false);
            Assert.AreEqual(2, _OutputStream.Length);
        }
        #endregion

        #region ClosestAircraft.json
        [TestMethod]
        public void WebSite_ClosestAircraft_Serves_Correct_Json_Object()
        {
            AddBlankAircraft(1);
            var aircraft = _AircraftLists[0][0];
            aircraft.Latitude = aircraft.Longitude = 1;

            var json = SendJsonRequest<ProximityGadgetAircraftJson>("/ClosestAircraft.json?lat=1&lng=1");

            Assert.IsNotNull(json);
            Assert.IsNotNull(json.ClosestAircraft);
        }

        [TestMethod]
        public void WebSite_ClosestAircraft_Sends_JSON_As_Text()
        {
            // The version of jQuery used in the proximity gadget leaked when it parsed JSON so traditionally it has been
            // sent as text and parsed with another library. VRS would always send JSON as text so there was no distinction
            // required but with v2 it started sending JSON with the JSON MIME type, which broke the gadget. Rather than
            // change the gadget and force everyone to install a new version - which would then be incompatible with older
            // versions of the server - the server just sends ClosestAircraft JSON as text.

            AddBlankAircraft(1);
            var aircraft = _AircraftLists[0][0];
            aircraft.Latitude = aircraft.Longitude = 1;

            var json = SendJsonRequest<ProximityGadgetAircraftJson>("/ClosestAircraft.json?lat=1&lng=1");

            Assert.AreEqual(MimeType.Text, _Response.Object.MimeType);
        }

        [TestMethod]
        public void WebSite_ClosestAircraft_Uses_Configured_Aircraft_List()
        {
            AddBlankAircraft(1, listIndex: 1);
            var aircraft = _AircraftLists[1][0];
            aircraft.Latitude = aircraft.Longitude = 1;
            _Configuration.GoogleMapSettings.ClosestAircraftReceiverId = 2;

            var json = SendJsonRequest<ProximityGadgetAircraftJson>("/ClosestAircraft.json?lat=1&lng=1");

            Assert.IsNotNull(json);
            Assert.IsNotNull(json.ClosestAircraft);
        }

        [TestMethod]
        public void WebSite_ClosestAircraft_Picks_Up_Change_To_Configured_Aircraft_List()
        {
            AddBlankAircraft(1, listIndex: 1);
            var aircraft = _AircraftLists[1][0];
            aircraft.Latitude = aircraft.Longitude = 1;
            _Configuration.GoogleMapSettings.ClosestAircraftReceiverId = 1;

            var json = SendJsonRequest<ProximityGadgetAircraftJson>("/ClosestAircraft.json?lat=1&lng=1");
            Assert.IsNotNull(json);
            Assert.IsNull(json.ClosestAircraft);

            _Configuration.GoogleMapSettings.ClosestAircraftReceiverId = 2;
            _ConfigurationStorage.Raise(r => r.ConfigurationChanged += null, EventArgs.Empty);

            json = SendJsonRequest<ProximityGadgetAircraftJson>("/ClosestAircraft.json?lat=1&lng=1");
            Assert.IsNotNull(json);
            Assert.IsNotNull(json.ClosestAircraft);
        }

        [TestMethod]
        public void WebSite_ClosestAircraft_Returns_Correct_Response_If_Receiver_Is_Not_Operating()
        {
            AddBlankAircraft(1);
            var aircraft = _AircraftLists[0][0];
            aircraft.Latitude = aircraft.Longitude = 1;
            _Configuration.GoogleMapSettings.ClosestAircraftReceiverId = 99;

            var json = SendJsonRequest<ProximityGadgetAircraftJson>("/ClosestAircraft.json?lat=1&lng=1");

            Assert.IsNotNull(json);
            Assert.AreEqual("Receiver is offline", json.WarningMessage);
        }

        [TestMethod]
        public void WebSite_ClosestAircraft_Warns_If_Location_Not_Specified()
        {
            var warning = "Position not supplied";

            var json = SendJsonRequest<ProximityGadgetAircraftJson>("/ClosestAircraft.json");
            Assert.AreEqual(warning, json.WarningMessage);

            json = SendJsonRequest<ProximityGadgetAircraftJson>("/ClosestAircraft.json?lat=1");
            Assert.AreEqual(warning, json.WarningMessage);

            json = SendJsonRequest<ProximityGadgetAircraftJson>("/ClosestAircraft.json?lng=1");
            Assert.AreEqual(warning, json.WarningMessage);

            json = SendJsonRequest<ProximityGadgetAircraftJson>("/ClosestAircraft.json?lat=1&lng=1");
            Assert.AreEqual(null, json.WarningMessage);
        }

        [TestMethod]
        [DataSource("Data Source='WebSiteTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "ClosestAircraftJson$")]
        public void WebSite_ClosestAircraft_Fills_ClosestAircraft_Details_Correctly()
        {
            var worksheet = new ExcelWorksheetData(TestContext);

            AddBlankAircraft(1);
            var aircraft = _AircraftLists[0][0];
            aircraft.Latitude = aircraft.Longitude = 1;

            var aircraftProperty = typeof(IAircraft).GetProperty(worksheet.String("AircraftProperty"));
            var aircraftValue = TestUtilities.ChangeType(worksheet.EString("AircraftValue"), aircraftProperty.PropertyType, new CultureInfo("en-GB"));
            aircraftProperty.SetValue(aircraft, aircraftValue, null);

            var json = SendJsonRequest<ProximityGadgetAircraftJson>("/ClosestAircraft.json?lat=1&lng=1");
            Assert.IsNotNull(json.ClosestAircraft);
            var aircraftJson = (ProximityGadgetClosestAircraftJson)json.ClosestAircraft;

            var jsonProperty = typeof(ProximityGadgetClosestAircraftJson).GetProperty(worksheet.String("JsonProperty"));

            var expected = TestUtilities.ChangeType(worksheet.EString("JsonValue"), jsonProperty.PropertyType, new CultureInfo("en-GB"));
            var actual = jsonProperty.GetValue(aircraftJson, null);

            Assert.AreEqual(expected, actual, jsonProperty.Name);
        }

        [TestMethod]
        [DataSource("Data Source='WebSiteTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "ClosestAircraftJson$")]
        public void WebSite_ClosestAircraft_Fills_ClosestAircraft_Details_Correctly_In_Foreign_Regions()
        {
            var worksheet = new ExcelWorksheetData(TestContext);

            using(var switcher = new CultureSwitcher("de-DE")) {
                AddBlankAircraft(1);
                var aircraft = _AircraftLists[0][0];
                aircraft.Latitude = aircraft.Longitude = 1;

                var aircraftProperty = typeof(IAircraft).GetProperty(worksheet.String("AircraftProperty"));
                var aircraftValue = TestUtilities.ChangeType(worksheet.EString("AircraftValue"), aircraftProperty.PropertyType, new CultureInfo("en-GB"));
                aircraftProperty.SetValue(aircraft, aircraftValue, null);

                var json = SendJsonRequest<ProximityGadgetAircraftJson>("/ClosestAircraft.json?lat=1&lng=1");
                Assert.IsNotNull(json.ClosestAircraft);
                var aircraftJson = (ProximityGadgetClosestAircraftJson)json.ClosestAircraft;

                var jsonProperty = typeof(ProximityGadgetClosestAircraftJson).GetProperty(worksheet.String("JsonProperty"));

                var expected = TestUtilities.ChangeType(worksheet.EString("JsonValue"), jsonProperty.PropertyType, new CultureInfo("en-GB"));
                var actual = jsonProperty.GetValue(aircraftJson, null);

                Assert.AreEqual(expected, actual, String.Format("{0}/{1}", jsonProperty.Name, switcher.CultureName));
            }
        }

        [TestMethod]
        [DataSource("Data Source='WebSiteTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "ClosestAircraftSelection$")]
        public void WebSite_ClosestAircraft_Returns_The_Closest_Aircraft()
        {
            var worksheet = new ExcelWorksheetData(TestContext);

            var latitude = worksheet.NDouble("GadgetLat");
            var longitude = worksheet.NDouble("GadgetLng");

            AddBlankAircraft(2);
            for(int i = 1;i <= 2;++i) {
                _AircraftLists[0][i - 1].Callsign = i.ToString();
                _AircraftLists[0][i - 1].Latitude = worksheet.NFloat(String.Format("AC{0}Lat", i));
                _AircraftLists[0][i - 1].Longitude = worksheet.NFloat(String.Format("AC{0}Lng", i));
            }

            var location = new StringBuilder();
            if(latitude != null) location.AppendFormat("?lat={0}", latitude);
            if(longitude != null) {
                location.Append(latitude == null ? "?" : "&");
                location.AppendFormat("lng={0}", longitude);
            }
            var json = SendJsonRequest<ProximityGadgetAircraftJson>(String.Format("/ClosestAircraft.json{0}", location));

            if(worksheet.String("Closest") == null) Assert.IsNotInstanceOfType(json.ClosestAircraft, typeof(ProximityGadgetClosestAircraftJson));
            else Assert.AreEqual(worksheet.String("Closest"), ((ProximityGadgetClosestAircraftJson)json.ClosestAircraft).Callsign);
        }

        [TestMethod]
        public void WebSite_ClosestAircraft_Returns_List_Of_Aircraft_Transmitting_An_Emergency_Squawk()
        {
            AddBlankAircraft(2);

            var json = SendJsonRequest<ProximityGadgetAircraftJson>("/ClosestAircraft.json?lat=1&lng=1");
            Assert.AreEqual(0, json.EmergencyAircraft.Count);

            _AircraftLists[0][0].Emergency = true;
            json = SendJsonRequest<ProximityGadgetAircraftJson>("/ClosestAircraft.json?lat=1&lng=1");
            Assert.AreEqual(1, json.EmergencyAircraft.Count);

            _AircraftLists[0][1].Emergency = true;
            json = SendJsonRequest<ProximityGadgetAircraftJson>("/ClosestAircraft.json?lat=1&lng=1");
            Assert.AreEqual(2, json.EmergencyAircraft.Count);
        }

        [TestMethod]
        [DataSource("Data Source='WebSiteTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "ClosestAircraftJson$")]
        public void WebSite_ClosestAircraft_Fills_EmergencyAircraft_Details_Correctly()
        {
            var worksheet = new ExcelWorksheetData(TestContext);

            if(worksheet.String("AircraftProperty") != "Emergency") {
                AddBlankAircraft(1);
                var aircraft = _AircraftLists[0][0];
                aircraft.Emergency = true;

                var aircraftProperty = typeof(IAircraft).GetProperty(worksheet.String("AircraftProperty"));
                var aircraftValue = TestUtilities.ChangeType(worksheet.EString("AircraftValue"), aircraftProperty.PropertyType, new CultureInfo("en-GB"));
                aircraftProperty.SetValue(aircraft, aircraftValue, null);

                var json = SendJsonRequest<ProximityGadgetAircraftJson>("/ClosestAircraft.json?lat=1&lng=1");
                var aircraftJson = (ProximityGadgetClosestAircraftJson)json.EmergencyAircraft[0];

                var jsonProperty = typeof(ProximityGadgetClosestAircraftJson).GetProperty(worksheet.String("JsonProperty"));

                var expected = TestUtilities.ChangeType(worksheet.EString("JsonValue"), jsonProperty.PropertyType, new CultureInfo("en-GB"));
                var actual = jsonProperty.GetValue(aircraftJson, null);

                Assert.AreEqual(expected, actual, jsonProperty.Name);
            }
        }

        [TestMethod]
        [DataSource("Data Source='WebSiteTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "ClosestAircraftJson$")]
        public void WebSite_ClosestAircraft_Fills_EmergencyAircraft_Details_Correctly_For_Foreign_Regions()
        {
            var worksheet = new ExcelWorksheetData(TestContext);

            if(worksheet.String("AircraftProperty") != "Emergency") {
                using(var switcher = new CultureSwitcher("de-DE")) {
                    AddBlankAircraft(1);
                    var aircraft = _AircraftLists[0][0];
                    aircraft.Emergency = true;

                    var aircraftProperty = typeof(IAircraft).GetProperty(worksheet.String("AircraftProperty"));
                    var aircraftValue = TestUtilities.ChangeType(worksheet.EString("AircraftValue"), aircraftProperty.PropertyType, new CultureInfo("en-GB"));
                    aircraftProperty.SetValue(aircraft, aircraftValue, null);

                    var json = SendJsonRequest<ProximityGadgetAircraftJson>("/ClosestAircraft.json?lat=1&lng=1");
                    var aircraftJson = (ProximityGadgetClosestAircraftJson)json.EmergencyAircraft[0];

                    var jsonProperty = typeof(ProximityGadgetClosestAircraftJson).GetProperty(worksheet.String("JsonProperty"));

                    var expected = TestUtilities.ChangeType(worksheet.EString("JsonValue"), jsonProperty.PropertyType, new CultureInfo("en-GB"));
                    var actual = jsonProperty.GetValue(aircraftJson, null);

                    Assert.AreEqual(expected, actual, jsonProperty.Name);
                }
            }
        }

        [TestMethod]
        [DataSource("Data Source='WebSiteTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "ClosestAircraftTrigonometry$")]
        public void WebSite_ClosestAircraft_Fills_Calculated_Details_Correctly()
        {
            var worksheet = new ExcelWorksheetData(TestContext);

            var address = String.Format("/ClosestAircraft.json?lat={0}&lng={1}", worksheet.String("GadgetLat"), worksheet.String("GadgetLng"));

            AddBlankAircraft(1);
            var aircraft = _AircraftLists[0][0];
            aircraft.Emergency = true;
            bool hasPosition = worksheet.String("AircraftLat") != null;
            if(hasPosition) {
                aircraft.Latitude = worksheet.NFloat("AircraftLat");
                aircraft.Longitude = worksheet.NFloat("AircraftLng");
            }

            var json = SendJsonRequest<ProximityGadgetAircraftJson>(address);

            Assert.AreEqual(worksheet.EString("Bearing"), json.EmergencyAircraft[0].BearingFromHere);
            Assert.AreEqual(worksheet.EString("Distance"), json.EmergencyAircraft[0].DistanceFromHere);
            if(hasPosition) {
                var closestAircraft = (ProximityGadgetClosestAircraftJson)json.ClosestAircraft;
                Assert.AreEqual(worksheet.EString("Distance"), closestAircraft.DistanceFromHere);
                Assert.AreEqual(worksheet.EString("Bearing"),  closestAircraft.BearingFromHere);
            }
        }

        [TestMethod]
        [DataSource("Data Source='WebSiteTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "ClosestAircraftTrigonometry$")]
        public void WebSite_ClosestAircraft_Fills_Calculated_Details_Correctly_For_Foreign_Regions()
        {
            var worksheet = new ExcelWorksheetData(TestContext);

            var address = String.Format("/ClosestAircraft.json?lat={0}&lng={1}", worksheet.String("GadgetLat"), worksheet.String("GadgetLng"));

            AddBlankAircraft(1);
            var aircraft = _AircraftLists[0][0];
            aircraft.Emergency = true;
            bool hasPosition = worksheet.String("AircraftLat") != null;
            if(hasPosition) {
                aircraft.Latitude = worksheet.NFloat("AircraftLat");
                aircraft.Longitude = worksheet.NFloat("AircraftLng");
            }

            using(var switcher = new CultureSwitcher("de-DE")) {
                var json = SendJsonRequest<ProximityGadgetAircraftJson>(address);

                Assert.AreEqual(worksheet.EString("Bearing"), json.EmergencyAircraft[0].BearingFromHere);
                Assert.AreEqual(worksheet.EString("Distance"), json.EmergencyAircraft[0].DistanceFromHere);
                if(hasPosition) {
                    var closestAircraft = (ProximityGadgetClosestAircraftJson)json.ClosestAircraft;
                    Assert.AreEqual(worksheet.EString("Distance"), closestAircraft.DistanceFromHere);
                    Assert.AreEqual(worksheet.EString("Bearing"),  closestAircraft.BearingFromHere);
                }
            }
        }

        [TestMethod]
        public void WebSite_ClosestAircraft_Honours_Configuration_Settings()
        {
            _Configuration.InternetClientSettings.AllowInternetProximityGadgets = false;

            var json = SendJsonRequest<ProximityGadgetAircraftJson>("/ClosestAircraft.json", true);
            Assert.IsNull(json);

            json = SendJsonRequest<ProximityGadgetAircraftJson>("/ClosestAircraft.json", false);
            Assert.IsNotNull(json);

            _Configuration.InternetClientSettings.AllowInternetProximityGadgets = true;
            _ConfigurationStorage.Raise(g => g.ConfigurationChanged += null, EventArgs.Empty);

            json = SendJsonRequest<ProximityGadgetAircraftJson>("/ClosestAircraft.json", true);
            Assert.IsNotNull(json);

            json = SendJsonRequest<ProximityGadgetAircraftJson>("/ClosestAircraft.json", false);
            Assert.IsNotNull(json);
        }

        [TestMethod]
        public void WebSite_ClosestAircraft_Sends_Correct_Cache_Control_Header()
        {
            SendJsonRequest<ProximityGadgetAircraftJson>("/ClosestAircraft.json", false);
            _Response.Verify(r => r.AddHeader("Cache-Control", "max-age=0, no-cache, no-store, must-revalidate"), Times.Once());
        }
        #endregion
    }
}
