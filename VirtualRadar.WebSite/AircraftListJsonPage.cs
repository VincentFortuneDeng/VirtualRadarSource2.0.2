﻿// Copyright © 2010 onwards, Andrew Whewell
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
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Web;
using VirtualRadar.Interface;
using VirtualRadar.Interface.BaseStation;
using VirtualRadar.Interface.StandingData;
using VirtualRadar.Interface.WebServer;
using VirtualRadar.Interface.WebSite;
using InterfaceFactory;
using System.Globalization;
using VirtualRadar.Interface.Settings;

namespace VirtualRadar.WebSite
{
    /// <summary>
    /// Responds to requests for aircraft lists in JSON file result.
    /// </summary>
    class AircraftListJsonPage : Page
    {
        /// <summary>
        /// The object that will do the work of producing JSON files from aircraft lists.
        /// </summary>
        private AircraftListJsonBuilder _Builder;

        /// <summary>
        /// The singleton receiver manager - we retain a reference to it to save having to constantly resolve the singleton, it will never change
        /// over the life of the program.
        /// </summary>
        private IFeedManager _ReceiverManager;

        /// <summary>
        /// The unique ID of the receiver to use when no feed ID has been specified in the request.
        /// </summary>
        private int _DefaultReceiverId;

        /// <summary>
        /// The tick on which the configuration was changed.
        /// </summary>
        private long _ConfigurationChangedTick;

        /// <summary>
        /// An aircraft list that is permanently empty.
        /// </summary>
        private IAircraftList _EmptyAircraftList;

        /// <summary>
        /// Gets or sets the aircraft list that is keeping track of aircraft in a flight simulator.
        /// </summary>
        public ISimpleAircraftList FlightSimulatorAircraftList { get; set; }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="webSite"></param>
        public AircraftListJsonPage(WebSite webSite) : base(webSite)
        {
            _EmptyAircraftList = Factory.Singleton.Resolve<ISimpleAircraftList>();
            _ReceiverManager = Factory.Singleton.Resolve<IFeedManager>().Singleton;
        }

        /// <summary>
        /// See base class.
        /// </summary>
        /// <param name="configuration"></param>
        protected override void DoLoadConfiguration(Configuration configuration)
        {
            _DefaultReceiverId = configuration.GoogleMapSettings.WebSiteReceiverId;
            _ConfigurationChangedTick = Provider.UtcNow.Ticks;
        }

        /// <summary>
        /// See base class.
        /// </summary>
        /// <param name="server"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        protected override bool DoHandleRequest(IWebServer server, RequestReceivedEventArgs args)
        {
            bool result = false;

            if(args.PathAndFile.Equals("/AircraftList.json", StringComparison.OrdinalIgnoreCase)) {
                var feedText = args.QueryString["feed"];
                int feed;
                if(String.IsNullOrEmpty(feedText) || !int.TryParse(feedText, NumberStyles.None, CultureInfo.InvariantCulture, out feed)) {
                    feed = _DefaultReceiverId;
                }
                var receiver = _ReceiverManager.GetByUniqueId(feed);
                var aircraftList = receiver == null ? _EmptyAircraftList : receiver.AircraftList;
                result = HandleAircraftListJson(args, feed, aircraftList, false);
            } else if(args.PathAndFile.Equals("/FlightSimList.json", StringComparison.OrdinalIgnoreCase)) {
                result = HandleAircraftListJson(args, -1, FlightSimulatorAircraftList, true);
            }

            return result;
        }

        /// <summary>
        /// Sends the appropriate AircraftList.json content in response to the request passed across.
        /// </summary>
        /// <param name="args"></param>
        /// <param name="feedId"></param>
        /// <param name="aircraftList"></param>
        /// <param name="isFlightSimulator"></param>
        /// <returns>Always returns true - this just helps to make the caller's code a little more compact.</returns>
        private bool HandleAircraftListJson(RequestReceivedEventArgs args, int feedId, IAircraftList aircraftList, bool isFlightSimulator)
        {
            if(_Builder == null) _Builder = new AircraftListJsonBuilder(Provider);

            if(aircraftList == null) args.Response.StatusCode = HttpStatusCode.InternalServerError;
            else {
                var buildArgs = ConstructBuildArgs(args, feedId, aircraftList, isFlightSimulator);
                var json = _Builder.Build(buildArgs);

                if(buildArgs.PreviousDataVersion > -1 && buildArgs.PreviousDataVersion <= _ConfigurationChangedTick) {
                    json.ServerConfigChanged = true;
                }

                Responder.SendJson(args.Request, args.Response, json, args.QueryString["callback"], null);
                args.Classification = ContentClassification.Json;
            }

            return true;
        }

        /// <summary>
        /// Creates an object that holds all of the aircraft list arguments that were extracted from the request.
        /// </summary>
        /// <param name="args"></param>
        /// <param name="feedId"></param>
        /// <param name="aircraftList"></param>
        /// <param name="isFlightSimulator"></param>
        /// <returns></returns>
        private AircraftListJsonBuilderArgs ConstructBuildArgs(RequestReceivedEventArgs args, int feedId, IAircraftList aircraftList, bool isFlightSimulator)
        {
            var result = new AircraftListJsonBuilderArgs() {
                AircraftList =          aircraftList,
                BrowserLatitude =       QueryNDouble(args, "lat"),
                BrowserLongitude =      QueryNDouble(args, "lng"),
                Filter =                isFlightSimulator ? null : ConstructFilter(args),
                IsFlightSimulatorList = isFlightSimulator,
                IsInternetClient =      args.IsInternetRequest,
                PreviousDataVersion =   QueryLong(args, "ldv", -1),
                ReceiverManager =       _ReceiverManager,
                ResendTrails =          QueryString(args, "refreshTrails", false) == "1",
                SelectedAircraftId =    QueryInt(args, "selAc", -1),
                SourceFeedId =          feedId,
            };

            var trFmt = QueryString(args, "trFmt", true);
            if(!String.IsNullOrEmpty(trFmt)) {
                switch(trFmt) {
                    case "F":       result.TrailType = TrailType.Full; break;
                    case "FA":      result.TrailType = TrailType.FullAltitude; break;
                    case "FS":      result.TrailType = TrailType.FullSpeed; break;
                    case "S":       result.TrailType = TrailType.Short; break;
                    case "SA":      result.TrailType = TrailType.ShortAltitude; break;
                    case "SS":      result.TrailType = TrailType.ShortSpeed; break;
                }
            }

            for(int sortColumnCount = 0;sortColumnCount < 2;++sortColumnCount) {
                var sortColumn = QueryString(args, String.Format("sortBy{0}", sortColumnCount + 1), true);
                var sortOrder = QueryString(args, String.Format("sortOrder{0}", sortColumnCount + 1), true);
                if(String.IsNullOrEmpty(sortColumn) || String.IsNullOrEmpty(sortOrder)) break;
                result.SortBy.Add(new KeyValuePair<string,bool>(sortColumn, sortOrder == "ASC"));
            }
            if(result.SortBy.Count == 0) result.SortBy.Add(new KeyValuePair<string,bool>(AircraftComparerColumn.FirstSeen, false));

            var previousAircraftIds = args.Request.Headers["X-VirtualRadarServer-AircraftIds"];
            if(!String.IsNullOrEmpty(previousAircraftIds)) {
                var decodedPreviousAircraftIds = HttpUtility.UrlDecode(previousAircraftIds);
                foreach(var chunk in decodedPreviousAircraftIds.Split(',')) {
                    int id;
                    if(int.TryParse(chunk, out id)) result.PreviousAircraft.Add(id);
                }
            }

            return result;
        }

        /// <summary>
        /// Extract the filter arguments from the request and returns them collected into an object.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        private AircraftListJsonBuilderFilter ConstructFilter(RequestReceivedEventArgs args)
        {
            var result = new AircraftListJsonBuilderFilter();

            foreach(string name in args.QueryString) {
                if(name.Length < 3) continue;
                var caselessName = name.ToUpper();
                var partialName = caselessName.Substring(0, 3);
                switch(partialName.ToUpper()) {
                    case "FAI":     if(caselessName.StartsWith("FAIR"))     result.Airport = DecodeStringFilter(name, args.QueryString[name]); break;
                    case "FAL":     if(caselessName.StartsWith("FALT"))     result.Altitude = DecodeIntRangeFilter(result.Altitude, name, args.QueryString[name]); break;
                    case "FCA":     if(caselessName.StartsWith("FCALL"))    result.Callsign = DecodeStringFilter(name, args.QueryString[name]); break;
                    case "FCO":     if(caselessName.StartsWith("FCOU"))     result.Icao24Country = DecodeStringFilter(name, args.QueryString[name]); break;
                    case "FDS":     if(caselessName.StartsWith("FDST"))     result.Distance = DecodeDoubleRangeFilter(result.Distance, name, args.QueryString[name]); break;
                    case "FEG":     if(caselessName.StartsWith("FEGT"))     result.EngineType = DecodeEnumFilter<EngineType>(name, args.QueryString[name]); break;
                    case "FIC":     if(caselessName.StartsWith("FICO"))     result.Icao24 = DecodeStringFilter(name, args.QueryString[name]); break;
                    case "FIN":     if(caselessName.StartsWith("FINT"))     result.IsInteresting = DecodeBoolFilter(name, args.QueryString[name]); break;
                    case "FMI":     if(caselessName.StartsWith("FMIL"))     result.IsMilitary = DecodeBoolFilter(name, args.QueryString[name]); break;
                    case "FNO":     if(caselessName.StartsWith("FNOPOS"))   result.MustTransmitPosition = DecodeBoolFilter(name, args.QueryString[name]); break;
                    case "FOP":                                             result.Operator = DecodeStringFilter(name, args.QueryString[name]); break;
                    case "FRE":     if(caselessName.StartsWith("FREG"))     result.Registration = DecodeStringFilter(name, args.QueryString[name]); break;
                    case "FSP":     if(caselessName.StartsWith("FSPC"))     result.Species = DecodeEnumFilter<Species>(name, args.QueryString[name]); break;
                    case "FSQ":     if(caselessName.StartsWith("FSQK"))     result.Squawk = DecodeIntRangeFilter(result.Squawk, name, args.QueryString[name]); break;
                    case "FTY":     if(caselessName.StartsWith("FTYP"))     result.Type = DecodeStringFilter(name, args.QueryString[name]); break;
                    case "FWT":     if(caselessName.StartsWith("FWTC"))     result.WakeTurbulenceCategory = DecodeEnumFilter<WakeTurbulenceCategory>(name, args.QueryString[name]); break;
                }
            }

            double? northBounds = QueryNDouble(args, "fNBnd");
            double? eastBounds = QueryNDouble(args, "fEBnd");
            double? southBounds = QueryNDouble(args, "fSBnd");
            double? westBounds = QueryNDouble(args, "fWBnd");

            if(northBounds != null && southBounds != null && westBounds != null && eastBounds != null) {
                result.PositionWithin = new Pair<Coordinate>(
                    new Coordinate((float)northBounds, (float)westBounds),
                    new Coordinate((float)southBounds, (float)eastBounds)
                );
            }

            return result;
        }
    }
}
