// Copyright © 2010 onwards, Andrew Whewell
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
using VirtualRadar.Interface.Database;
using VirtualRadar.Interface.WebServer;
using VirtualRadar.Interface.WebSite;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Settings;
using VirtualRadar.Interface.StandingData;
using InterfaceFactory;
using System.Diagnostics;

namespace VirtualRadar.Plugin.AircraftTrackLog
{
    /// <summary>
    /// Serves pages of report rows.
    /// </summary>
    class ReportTrailsJsonPage : Page
    {
        #region Private class - Parameters
        /// <summary>
        /// A private class that holds the parameters passed to us by the Javascript via query strings etc.
        /// on the URL.
        /// </summary>
        class Parameters 
        {
            public string StartTime { get; set; }

            public string ICAO24 { get; set; }
        }
        #endregion

        #region Fields
        /// <summary>
        /// 记录飞机轨迹日志对象
        /// </summary>
        private ITrackFlightLog _TrackFlightLog;
        #endregion

        #region Properties
        

       
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new object.
        /// </summary>
        public ReportTrailsJsonPage(IWebSite webSite)
            : base(webSite)
        {
            _TrackFlightLog = Factory.Singleton.Resolve<ITrackFlightLog>().Singleton;
        }
        #endregion

        #region DoLoadConfiguration
        /// <summary>
        /// See base class.
        /// </summary>
        /// <param name="configuration"></param>
        protected override void DoLoadConfiguration(Configuration configuration)
        {
            base.DoLoadConfiguration(configuration);
        }
        #endregion

        #region DoHandleRequest
        /// <summary>
        /// See base class.
        /// </summary>
        /// <param name="server"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        protected override bool DoHandleRequest(RequestReceivedEventArgs args)
        {
            bool result = false;

            if(args.PathAndFile.Equals("/Trail/ReportTrails.json", StringComparison.OrdinalIgnoreCase)) {
                result = true;

                FlightTrailReportJson json = new FlightTrailReportJson();
                var startTime = Provider.UtcNow;

                try {

                    var parameters = ExtractParameters(args);
                    json = CreateReportTrails(parameters);
                 
                } catch(Exception ex) {
                    Debug.WriteLine(String.Format("ReportTrailsJsonPage.DoHandleRequest caught exception {0}", ex.ToString()));
                    ILog log = Factory.Singleton.Resolve<ILog>().Singleton;
                    log.WriteLine("An exception was encountered during the processing of a report trails: {0}", ex.ToString());
                    //if(json == null) json = new FlightTrailReportJson();
                    json.ErrorText = String.Format("An exception was encounted during the processing of the report trails, see log for full details: {0}", ex.Message);
                }

                json.ProcessingTime = String.Format("{0:N3}", (Provider.UtcNow - startTime).TotalSeconds);
                Responder.SendJson(args.Request, args.Response, json, null, null);
                /*if(json == null) json = (ReportRowsJson)Activator.CreateInstance(expectedJsonType);
                json.ProcessingTime = String.Format("{0:N3}", (Provider.UtcNow - startTime).TotalSeconds);
                json.OperatorFlagsAvailable = _ShowOperatorFlags;
                json.SilhouettesAvailable = _ShowSilhouettes;
                
                args.Classification = ContentClassification.Json;*/
            }

            return result;
        }

        

        /// <summary>
        /// Extracts the parameters from the query string portion of the URL.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        private Parameters ExtractParameters(RequestReceivedEventArgs args)
        {
            var result = new Parameters() {
                StartTime = QueryString(args, "startTime", false),
                ICAO24 = QueryString(args, "aircraftID", true),
            };

            return result;
        }


        /// <summary>
        /// Creates the JSON for a report that describes a single aircraft and the flights it has undertaken.
        /// </summary>
        /// <param name="args"></param>
        /// <param name="parameters"></param>
        /// <param name="findByIcao"></param>
        /// <returns></returns>
        private FlightTrailReportJson CreateReportTrails(Parameters parameters)
        {
            Factory.Singleton.Resolve<ILog>().Singleton.WriteLine(parameters.StartTime);
            FlightTrailReportJson json = new FlightTrailReportJson() {
                CountRows = 0,
                StartTime =DateTime.Parse(parameters.StartTime),
                ICAO24 = parameters.ICAO24,
            };

            json.Flights = _TrackFlightLog.ReadFlightTrail(json.StartTime, json.ICAO24);
            
            json.CountRows = json.Flights.Count;

            return json;
        }

        #endregion
    }
}
