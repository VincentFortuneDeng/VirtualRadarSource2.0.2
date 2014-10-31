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
using System.Linq;
using System.Text;
using InterfaceFactory;
using VirtualRadar.Interface;
using VirtualRadar.Interface.WebServer;
using VirtualRadar.Interface.WebSite;

namespace VirtualRadar.WebSite
{
    /// <summary>
    /// Responds to requests for polar plot data.
    /// </summary>
    class PolarPlotJsonPage : Page
    {
        #region Fields
        /// <summary>
        /// The singleton feed manager.
        /// </summary>
        private IFeedManager _FeedManager;

        /// <summary>
        /// A copy of the configuration setting for polar plot permissions.
        /// </summary>
        private bool _InternetClientCanShowPolarPlot;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="webSite"></param>
        public PolarPlotJsonPage(WebSite webSite) : base(webSite)
        {
            _FeedManager = Factory.Singleton.Resolve<IFeedManager>().Singleton;
        }
        #endregion

        #region DoLoadConfiguration
        /// <summary>
        /// Handles changes to the configuration.
        /// </summary>
        /// <param name="configuration"></param>
        protected override void DoLoadConfiguration(Interface.Settings.Configuration configuration)
        {
            _InternetClientCanShowPolarPlot = configuration.InternetClientSettings.CanShowPolarPlots;
            base.DoLoadConfiguration(configuration);
        }
        #endregion

        #region DoHandleRequest
        protected override bool DoHandleRequest(IWebServer server, RequestReceivedEventArgs args)
        {
            var result = false;

            if(args.PathAndFile.Equals("/PolarPlot.json", StringComparison.OrdinalIgnoreCase)) {
                result = true;

                var allowRequest = _InternetClientCanShowPolarPlot || !args.IsInternetRequest;
                var feedId = allowRequest ? QueryInt(args, "feedId", -1) : -1;
                var json = new PolarPlotsJson() {
                    FeedId = feedId,
                };

                if(allowRequest) {
                    var feed = _FeedManager.GetByUniqueId(feedId);
                    var polarPlotter = feed == null || feed.AircraftList == null ? null : feed.AircraftList.PolarPlotter;
                    if(polarPlotter != null) {
                        foreach(var slice in polarPlotter.TakeSnapshot()) {
                            var jsonSlice = new PolarPlotsSliceJson() {
                                StartAltitude = slice.AltitudeLower,
                                FinishAltitude = slice.AltitudeHigher,
                            };
                            json.Slices.Add(jsonSlice);

                            foreach(var kvp in slice.PolarPlots.OrderBy(r => r.Key)) {
                                var plot = kvp.Value;
                                jsonSlice.Plots.Add(new PolarPlotJson() {
                                    Latitude = (float)plot.Latitude,
                                    Longitude = (float)plot.Longitude,
                                });
                            }
                        }
                    }
                }

                Responder.SendJson(args.Request, args.Response, json, null, null);
            }

            return result;
        }
        #endregion
    }
}
