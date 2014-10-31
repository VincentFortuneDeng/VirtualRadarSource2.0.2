﻿// Copyright © Honglin Aviation, Vincent Deng(邓守海)
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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using InterfaceFactory;
using VirtualRadar.Interface;
using VirtualRadar.Interface.BaseStation;
using VirtualRadar.Interface.Database;
using VirtualRadar.Interface.Settings;
using VirtualRadar.Interface.StandingData;
using VirtualRadar.Interface.WebServer;
using VirtualRadar.Interface.WebSite;
using System.Windows.Forms;
using System.Collections.Specialized;

namespace VirtualRadar.WebSite
{
    /// <summary>
    /// Implements <see cref="IWebSite"/>.
    /// </summary>
    class WebSite : IWebSite
    {
        #region Private class - DefaultProvider
        /// <summary>
        /// The default implementation of <see cref="IWebSiteProvider"/>.
        /// </summary>
        class DefaultProvider : IWebSiteProvider
        {
            public bool DirectoryExists(string folder)  { return Directory.Exists(folder); }
            public DateTime UtcNow                      { get { return DateTime.UtcNow; } }
        }
        #endregion

        #region Private class - SimpleRequest
        /// <summary>
        /// The fake request object used by <see cref="RequestSimpleContent"/>.
        /// </summary>
        class SimpleRequest : IRequest, IDisposable
        {
            public long ContentLength64 { get; private set; }

            public NameValueCollection Headers { get; private set; }

            public Stream InputStream { get; private set; }

            public string RawUrl { get; private set; }

            public IPEndPoint RemoteEndPoint { get; private set; }

            public IPEndPoint LocalEndPoint { get; private set; }

            public Uri Url { get; private set; }

            public string UserAgent { get; private set; }

            public string UserHostName { get; private set; }

            public SimpleRequest(string root, string pathAndFile)
            {
                ContentLength64 = 0;
                Headers = new NameValueCollection();
                InputStream = new MemoryStream(new byte[] {});
                RawUrl = String.Format("{0}{1}", root, pathAndFile);
                RemoteEndPoint = new IPEndPoint(IPAddress.Loopback, 10000);
                LocalEndPoint = new IPEndPoint(IPAddress.Loopback, 10001);
                Url = new Uri(String.Format("http://{0}{1}", RemoteEndPoint.Address, RawUrl));
                UserAgent = "FAKE REQUEST";
                UserHostName = "FAKE.HOST.NAME";
            }

            ~SimpleRequest()
            {
                Dispose(false);
            }

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            protected virtual void Dispose(bool disposing)
            {
                if(disposing) {
                    if(InputStream != null) {
                        InputStream.Dispose();
                        InputStream = null;
                    }
                }
            }
        }
        #endregion

        #region Private class - SimpleResponse
        /// <summary>
        /// The fake response object used by <see cref="RequestSimpleContent"/>.
        /// </summary>
        class SimpleResponse : IResponse
        {
            public long ContentLength { get; set; }
            public string MimeType { get; set; }
            public Stream OutputStream { get; private set; }
            public HttpStatusCode StatusCode { get; set; }
            public Encoding ContentEncoding { get; set; }

            public SimpleResponse(Stream outputStream)
            {
                OutputStream = outputStream;
                StatusCode = HttpStatusCode.NotFound;
            }

            public void Abort()                                 { ; }
            public void AddHeader(string name, string value)    { ; }
            public void Close()                                 { ; }
            public void Dispose()                               { ; }
            public void EnableCompression(IRequest request)     { ; }
            public void Redirect(string url)                    { ; }
        }
        #endregion

        #region Fields
        /// <summary>
        /// The object that synchronises threads that are performing authentication tasks for the site.
        /// </summary>
        private object _AuthenticationSyncLock = new object();

        /// <summary>
        /// The user that the server will allow when basic authentication is selected.
        /// </summary>
        private string _BasicAuthenticationUser;

        /// <summary>
        /// The password hash that the server will allow when basic authentication is selected.
        /// </summary>
        private Hash _BasicAuthenticationPasswordHash;

        /// <summary>
        /// The object that will bundle JavaScript in HTML files for us.
        /// </summary>
        private IBundler _Bundler;

        /// <summary>
        /// The object that will minify JavaScript and CSS for us.
        /// </summary>
        private IMinifier _Minifier;

        /// <summary>
        /// The type of proxy that the server is sitting behind.
        /// </summary>
        private ProxyType _ProxyType;

        /// <summary>
        /// The lock object that protects <see cref="_HtmlContentInjectors"/>. Do not call event handlers
        /// with this locked.
        /// </summary>
        private object _HtmlContentInjectorsLock = new object();

        /// <summary>
        /// The list of content injectors.
        /// </summary>
        private List<HtmlContentInjector> _HtmlContentInjectors = new List<HtmlContentInjector>();

        /// <summary>
        /// A list of objects that can supply content for us.
        /// </summary>
        private List<Page> _Pages = new List<Page>();

        /// <summary>
        /// The page that will deal with requests for aircraft lists.
        /// </summary>
        private AircraftListJsonPage _AircraftListJsonPage;

        /// <summary>
        /// The page that deals with serving requests from the file system.
        /// </summary>
        private FileSystemPage _FileSystemPage;

        /// <summary>
        /// The page that will deal with requests for images.
        /// </summary>
        private ImagePage _ImagePage;

        /// <summary>
        /// The page that handles requests from the proximity gadget.
        /// </summary>
        private ClosestAircraftJsonPage _ClosestAircraftJsonPage;

        /// <summary>
        /// The page that handles requests for report rows.
        /// </summary>
        private ReportRowsJsonPage _ReportRowsJsonPage;
        #endregion

        #region Properties
        /// <summary>
        /// See interface docs.
        /// </summary>
        public IWebSiteProvider Provider { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public ISimpleAircraftList FlightSimulatorAircraftList
        {
            get { return _AircraftListJsonPage.FlightSimulatorAircraftList; }
            set { _AircraftListJsonPage.FlightSimulatorAircraftList = value; }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public IBaseStationDatabase BaseStationDatabase
        {
            get { return _ReportRowsJsonPage.BaseStationDatabase; }
            set { _ReportRowsJsonPage.BaseStationDatabase = value; }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public IStandingDataManager StandingDataManager
        {
            get { return _ReportRowsJsonPage.StandingDataManager; }
            set { _ReportRowsJsonPage.StandingDataManager = value; }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public IWebServer WebServer { get; private set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new object.
        /// </summary>
        public WebSite()
        {
            Provider = new DefaultProvider();

            _AircraftListJsonPage = new AircraftListJsonPage(this);
            _ClosestAircraftJsonPage = new ClosestAircraftJsonPage(this);
            _FileSystemPage = new FileSystemPage(this);
            _ImagePage = new ImagePage(this);
            _ReportRowsJsonPage = new ReportRowsJsonPage(this);
        }
        #endregion

        #region AttachSiteToServer, LoadConfiguration
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="server"></param>
        public void AttachSiteToServer(IWebServer server)
        {
            if(server == null) throw new ArgumentNullException("server");
            if(WebServer != server) {
                if(WebServer != null) throw new InvalidOperationException("The web site can only be attached to one server");

                var configurationStorage = Factory.Singleton.Resolve<IConfigurationStorage>().Singleton;
                configurationStorage.ConfigurationChanged += ConfigurationStorage_ConfigurationChanged;

                WebServer = server;
                server.Root = "/VirtualRadar";

                var installerSettingsStorage = Factory.Singleton.Resolve<IInstallerSettingsStorage>();
                var installerSettings = installerSettingsStorage.Load();
                server.Port = installerSettings.WebServerPort;

                server.AuthenticationRequired += Server_AuthenticationRequired;

                _Bundler = Factory.Singleton.Resolve<IBundler>();
                _Bundler.AttachToWebSite(this);

                _Minifier = Factory.Singleton.Resolve<IMinifier>();
                _Minifier.Initialise();

                _Pages.Add(_FileSystemPage);
                _Pages.Add(new TextPage(this));
                _Pages.Add(_AircraftListJsonPage);
                _Pages.Add(_ImagePage);
                _Pages.Add(new ServerConfigJsonPage(this));
                _Pages.Add(new AudioPage(this));
                _Pages.Add(new FaviconPage(this));
                _Pages.Add(_ReportRowsJsonPage);
                _Pages.Add(_ClosestAircraftJsonPage);
                _Pages.Add(new AirportDataProxyPage(this));
                _Pages.Add(new PolarPlotJsonPage(this));

                foreach(var page in _Pages) {
                    page.Provider = Provider;
                }

                LoadConfiguration();

                server.RequestReceived += Server_RequestReceived;
            }
        }

        /// <summary>
        /// Loads and applies the configuration from disk.
        /// </summary>
        /// <returns>True if the server should be restarted because of changes to the configuration.</returns>
        private bool LoadConfiguration()
        {
            var configuration = Factory.Singleton.Resolve<IConfigurationStorage>().Singleton.Load();

            bool result = false;
            lock(_AuthenticationSyncLock) {
                _BasicAuthenticationUser = configuration.WebServerSettings.BasicAuthenticationUser;
                _BasicAuthenticationPasswordHash = configuration.WebServerSettings.BasicAuthenticationPasswordHash;
                if(WebServer.AuthenticationScheme != configuration.WebServerSettings.AuthenticationScheme) {
                    result = true;
                    WebServer.AuthenticationScheme = configuration.WebServerSettings.AuthenticationScheme;
                }
            }

            _ProxyType = configuration.GoogleMapSettings.ProxyType;

            foreach(var page in _Pages) {
                page.LoadConfiguration(configuration);
            }

            return result;
        }
        #endregion

        #region AddSiteRoot, RemoveSiteRoot, IsSiteRootActive, GetSiteRootFolders
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="siteRoot"></param>
        public void AddSiteRoot(SiteRoot siteRoot)
        {
            _FileSystemPage.AddSiteRoot(siteRoot);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="siteRoot"></param>
        public void RemoveSiteRoot(SiteRoot siteRoot)
        {
            _FileSystemPage.RemoveSiteRoot(siteRoot);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="siteRoot"></param>
        /// <param name="folderMustMatch"></param>
        /// <returns></returns>
        public bool IsSiteRootActive(SiteRoot siteRoot, bool folderMustMatch)
        {
            return _FileSystemPage.IsSiteRootActive(siteRoot, folderMustMatch);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <returns></returns>
        public List<string> GetSiteRootFolders()
        {
            return _FileSystemPage.GetSiteRootFolders();
        }
        #endregion

        #region RequestContent, RequestSimpleContent
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="args"></param>
        public void RequestContent(RequestReceivedEventArgs args)
        {
            if(args == null) throw new ArgumentNullException("args");

            if(args.PathAndFile == "/") RedirectToDefaultPage(args);
            else {
                foreach(var page in _Pages) {
                    page.HandleRequest(WebServer, args);
                }
            }
        }

        /// <summary>
        /// Redirects the browser to the appropriate default page.
        /// </summary>
        /// <param name="args"></param>
        private void RedirectToDefaultPage(RequestReceivedEventArgs args)
        {
            switch(args.Request.Url.Scheme) {
                case "http":
                case "https":
                    var hostName = _ProxyType == ProxyType.Reverse ? args.Request.LocalEndPoint.Address.ToString() : args.Request.UserHostName;
                    if(_ProxyType == ProxyType.Reverse) {
                        var isDefaultPort = false;
                        switch(args.Request.Url.Scheme) {
                            case "http":        isDefaultPort = args.Request.LocalEndPoint.Port == 80; break;
                            case "https":       isDefaultPort = args.Request.LocalEndPoint.Port == 443; break;
                        }
                        if(!isDefaultPort) hostName = String.Format("{0}:{1}", hostName, args.Request.LocalEndPoint.Port);
                    }

                    string redirectUrl = String.Format("{0}://{1}{2}{3}{4}",
                        /* 0 */ args.Request.Url.Scheme,
                        /* 1 */ hostName,
                        /* 2 */ args.Request.RawUrl,
                        /* 3 */ args.Request.RawUrl.EndsWith("/") ? "" : "/",
                        /* 4 */ args.IsAndroid || args.IsIPad || args.IsIPhone || args.IsIPod ? "mobile.html" : "desktop.html");
                    args.Handled = true;
                    args.Response.Redirect(redirectUrl);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="pathAndFile"></param>
        /// <returns></returns>
        public SimpleContent RequestSimpleContent(string pathAndFile)
        {
            if(pathAndFile == null) throw new ArgumentNullException("pathAndFile");
            var result = new SimpleContent();

            const string root = "/Root";
            using(var simpleRequest = new SimpleRequest(root, pathAndFile)) {
                using(var memoryStream = new MemoryStream()) {
                    var simpleResponse = new SimpleResponse(memoryStream);
                    var args = new RequestReceivedEventArgs(simpleRequest, simpleResponse, root);
                    RequestContent(args);

                    result.HttpStatusCode = simpleResponse.StatusCode;
                    result.Content = memoryStream.ToArray();
                }
            }

            return result;
        }
        #endregion

        #region BundleHtml, MinifyJavaScript, MinifyCss
        /// <summary>
        /// Bundles multiple JavaScript loads in the HTML into a single JavaScript load.
        /// </summary>
        /// <param name="requestPathAndFile"></param>
        /// <param name="textContent"></param>
        /// <returns></returns>
        internal string BundleHtml(string requestPathAndFile, TextContent textContent)
        {
            var result = _Bundler.BundleHtml(requestPathAndFile, textContent.Content);
            textContent.Content = result;

            return result;
        }

        /// <summary>
        /// Minifies JavaScript on behalf of a page.
        /// </summary>
        /// <param name="textContent"></param>
        /// <returns></returns>
        internal string MinifyJavaScript(TextContent textContent)
        {
            var result = _Minifier.MinifyJavaScript(textContent.Content);
            textContent.Content = result;

            return result;
        }

        /// <summary>
        /// Minifies CSS on behalf of a page.
        /// </summary>
        /// <param name="textContent"></param>
        /// <returns></returns>
        internal string MinifyCss(TextContent textContent)
        {
            // The minifier is breaking jQueryUI's css, we'll cut it out for now. // return _Minifier.MinifyCss(css);
            return textContent.Content;
        }
        #endregion

        #region AddHtmlContentInjector, RemoveHtmlContentInjector
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="contentInjector"></param>
        public void AddHtmlContentInjector(HtmlContentInjector contentInjector)
        {
            if(contentInjector == null) throw new ArgumentNullException("contentInjector");
            lock(_HtmlContentInjectorsLock) _HtmlContentInjectors.Add(contentInjector);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="contentInjector"></param>
        public void RemoveHtmlContentInjector(HtmlContentInjector contentInjector)
        {
            lock(_HtmlContentInjectorsLock) _HtmlContentInjectors.Remove(contentInjector);
        }

        /// <summary>
        /// Injects content into HTML files.
        /// </summary>
        /// <param name="pathAndFile"></param>
        /// <param name="textContent"></param>
        /// <returns></returns>
        internal string InjectHtmlContent(string pathAndFile, TextContent textContent)
        {
            var result = textContent.Content;

            List<HtmlContentInjector> injectors;
            lock(_HtmlContentInjectorsLock) {
                injectors = _HtmlContentInjectors.Where(r =>
                    !String.IsNullOrEmpty(r.Element) &&
                    r.Content != null &&
                    (r.PathAndFile == null || r.PathAndFile.Equals(pathAndFile, StringComparison.OrdinalIgnoreCase))
                ).ToList();
            }
            if(injectors.Any()) {
                var document = new HtmlAgilityPack.HtmlDocument() {
                    OptionCheckSyntax = false,
                    OptionDefaultStreamEncoding = textContent.Encoding,
                };
                document.LoadHtml(result);

                var modified = false;
                foreach(var injector in injectors.OrderByDescending(r => r.Priority)) {
                    var elements = document.DocumentNode.Descendants(injector.Element);
                    var element = injector.AtStart ? elements.FirstOrDefault() : elements.LastOrDefault();
                    var content = element == null ? null : injector.Content();
                    if(element != null && !String.IsNullOrEmpty(content)) {
                        var subDocument = new HtmlAgilityPack.HtmlDocument() {
                            OptionCheckSyntax = false,
                        };
                        subDocument.LoadHtml(injector.Content());

                        if(injector.AtStart) element.PrependChild(subDocument.DocumentNode);
                        else                 element.AppendChild(subDocument.DocumentNode);
                        modified = true;
                    }
                }

                if(modified) {
                    using(var stream = new MemoryStream()) {
                        document.Save(stream);
                        stream.Position = 0;
                        using(var streamReader = new StreamReader(stream, textContent.Encoding, true)) {
                            result = streamReader.ReadToEnd();
                        }
                    }
                }
            }

            textContent.Content = result;
            return result;
        }
        #endregion

        #region Events consumed
        /// <summary>
        /// Handles changes to the configuration.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void ConfigurationStorage_ConfigurationChanged(object sender, EventArgs args)
        {
            if(WebServer != null && LoadConfiguration()) {
                WebServer.Online = false;
                WebServer.Online = true;
            }
        }

        /// <summary>
        /// Handles the authentication events from the server.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void Server_AuthenticationRequired(object sender, AuthenticationRequiredEventArgs args)
        {
            lock(_AuthenticationSyncLock) {
                if(!args.IsHandled && WebServer.AuthenticationScheme == AuthenticationSchemes.Basic) {
                    args.IsAuthenticated = args.User != null && args.User.Equals(_BasicAuthenticationUser, StringComparison.OrdinalIgnoreCase);
                    if(args.IsAuthenticated) args.IsAuthenticated = _BasicAuthenticationPasswordHash.PasswordMatches(args.Password);
                    args.IsHandled = true;
                }
            }
        }

        /// <summary>
        /// Handles the request for content by a server.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void Server_RequestReceived(object sender, RequestReceivedEventArgs args)
        {
            RequestContent(args);
        }
        #endregion
    }
}
