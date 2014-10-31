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
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Listener;
using VirtualRadar.Interface.Settings;
using InterfaceFactory;

namespace VirtualRadar.Library.Listener
{
    /// <summary>
    /// The default implementation of <see cref="IPolarPlotter"/>.
    /// </summary>
    class PolarPlotter : IPolarPlotter
    {
        #region Fields
        /// <summary>
        /// The spin lock that protects <see cref="_Slices"/> from multi-threaded access.
        /// </summary>
        private SpinLock _SlicesLock = new SpinLock();

        /// <summary>
        /// The slices created by <see cref="Initialise"/>, filled in by <see cref="AddCoordinate"/> and
        /// exposed by <see cref="TakeSnapshot"/>.
        /// </summary>
        private List<PolarPlotSlice> _Slices = new List<PolarPlotSlice>();

        /// <summary>
        /// The object that sanity-checks altitudes and positions so that we're not recording gibberish.
        /// </summary>
        private IAircraftSanityChecker _SanityChecker;

        /// <summary>
        /// The distance in kilometres over which the receiver can pick up aircraft.
        /// </summary>
        private double _ReceiverRange;

        /// <summary>
        /// True if the events have been hooked.
        /// </summary>
        private bool _HookedEvents;
        #endregion

        #region Properties
        /// <summary>
        /// See interface docs.
        /// </summary>
        public double Latitude { get; private set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public double Longitude { get; private set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public int RoundToDegrees { get; private set; }
        #endregion

        #region Initialise
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="latitude"></param>
        /// <param name="longitude"></param>
        public void Initialise(double latitude, double longitude)
        {
            using(_SlicesLock.AcquireLock()) {
                if(latitude > 90.0 || latitude < -90.0) throw new ArgumentException("latitude");
                if(longitude > 180.0 || longitude < -180.0) throw new ArgumentException("longitude");

                LoadConfiguration();
                Latitude = latitude;
                Longitude = longitude;
                RoundToDegrees = 1;

                _Slices.Clear();
                _Slices.Add(new PolarPlotSlice() { AltitudeLower = int.MinValue, AltitudeHigher = int.MaxValue });
                _Slices.Add(new PolarPlotSlice() { AltitudeLower = int.MinValue, AltitudeHigher = 9999 });
                _Slices.Add(new PolarPlotSlice() { AltitudeLower = 10000,        AltitudeHigher = 19999 });
                _Slices.Add(new PolarPlotSlice() { AltitudeLower = 20000,        AltitudeHigher = 29999 });
                _Slices.Add(new PolarPlotSlice() { AltitudeLower = 30000,        AltitudeHigher = int.MaxValue });

                InitialiseSlices();
            }
        }

        private void InitialiseSlices()
        {
            foreach(var slice in _Slices) {
                for(var bearing = 0;bearing < 360;bearing += 1) {
                    var roundedBearing = RoundBearing(bearing);
                    if(!slice.PolarPlots.ContainsKey(roundedBearing)) {
                        slice.PolarPlots.Add(roundedBearing, new PolarPlot() {
                            Altitude = slice.AltitudeLower,
                            Angle = roundedBearing,
                            Distance = 0.0,
                            Latitude = Latitude,
                            Longitude = Longitude,
                        });
                    }
                }
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="latitude"></param>
        /// <param name="longitude"></param>
        /// <param name="lowSliceAltitude"></param>
        /// <param name="highSliceAltitude"></param>
        /// <param name="sliceHeight"></param>
        /// <param name="roundToDegrees"></param>
        public void Initialise(double latitude, double longitude, int lowSliceAltitude, int highSliceAltitude, int sliceHeight, int roundToDegrees)
        {
            using(_SlicesLock.AcquireLock()) {
                if(latitude > 90.0 || latitude < -90.0) throw new ArgumentException("latitude");
                if(longitude > 180.0 || longitude < -180.0) throw new ArgumentException("longitude");
                if(lowSliceAltitude > highSliceAltitude) throw new ArgumentException("lowSliceAltitude");
                if(roundToDegrees > 180 || roundToDegrees < 1) throw new ArgumentException("roundToDegrees");
                if(sliceHeight < 1) throw new ArgumentException("sliceHeight");

                LoadConfiguration();
                Latitude = latitude;
                Longitude = longitude;
                RoundToDegrees = roundToDegrees;

                _Slices.Clear();
                for(var i = lowSliceAltitude;i <= highSliceAltitude;i += sliceHeight) {
                    _Slices.Add(new PolarPlotSlice() { AltitudeLower = i, AltitudeHigher = Math.Min(highSliceAltitude, i + (sliceHeight - 1)) });
                }
                if(_Slices.Count > 1) _Slices.Add(new PolarPlotSlice() { AltitudeLower = lowSliceAltitude, AltitudeHigher = highSliceAltitude });
                _Slices.Add(new PolarPlotSlice() { AltitudeLower = int.MinValue, AltitudeHigher = int.MaxValue });

                InitialiseSlices();
            }
        }

        /// <summary>
        /// Loads configuration settings while the slice lock is held.
        /// </summary>
        private void LoadConfiguration()
        {
            var configurationStorage = Factory.Singleton.Resolve<IConfigurationStorage>().Singleton;
            var configuration = configurationStorage.Load();

            var receiverRange = configuration.RawDecodingSettings.ReceiverRange;
            if(_SanityChecker == null || receiverRange != _ReceiverRange) {
                _ReceiverRange = receiverRange;
                _SanityChecker = Factory.Singleton.Resolve<IAircraftSanityChecker>();
                foreach(var slice in _Slices) {
                    slice.PolarPlots.Clear();
                }
            }

            if(!_HookedEvents) {
                _HookedEvents = true;
                configurationStorage.ConfigurationChanged += EventHandlerUtils.MakeWeak<EventArgs>(ConfigurationStorage_ConfigurationChanged, r => configurationStorage.ConfigurationChanged -= ConfigurationStorage_ConfigurationChanged);
            }
        }
        #endregion

        #region AddCoordinate
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="aircraftId"></param>
        /// <param name="altitude"></param>
        /// <param name="latitude"></param>
        /// <param name="longitude"></param>
        public void AddCoordinate(int aircraftId, int altitude, double latitude, double longitude)
        {
            if(_SanityChecker != null) {
                var now = DateTime.UtcNow;
                if(_SanityChecker.CheckAltitude(aircraftId, now, altitude) == Certainty.ProbablyRight) {
                    if(_SanityChecker.CheckPosition(aircraftId, now, latitude, longitude) == Certainty.ProbablyRight) {
                        AddCheckedCoordinate(aircraftId, altitude, latitude, longitude);
                    }
                }
            }
        }

        private int RoundBearing(double bearing)
        {
            return (((int)(bearing + (RoundToDegrees / 2.0)) / RoundToDegrees) * RoundToDegrees) % 360;
        }
        #endregion

        #region AddCheckedCoordinate
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="aircraftId"></param>
        /// <param name="altitude"></param>
        /// <param name="latitude"></param>
        /// <param name="longitude"></param>
        public void AddCheckedCoordinate(int aircraftId, int altitude, double latitude, double longitude)
        {
            if(RoundToDegrees > 0) {
                var distance = GreatCircleMaths.Distance(Latitude, Longitude, latitude, longitude);
                var fullBearing = GreatCircleMaths.Bearing(Latitude, Longitude, latitude, longitude, null, false, true);

                if(distance != null && fullBearing != null && distance <= _ReceiverRange) {
                    var roundedBearing = RoundBearing(fullBearing.Value);

                    _SlicesLock.Lock();
                    try {
                        foreach(var slice in _Slices) {
                            if(slice.AltitudeLower <= altitude && slice.AltitudeHigher >= altitude) {
                                PolarPlot plot;
                                if(!slice.PolarPlots.TryGetValue(roundedBearing, out plot)) {
                                    plot = new PolarPlot();
                                    slice.PolarPlots.Add(roundedBearing, plot);
                                }
                                if(distance >= plot.Distance) {
                                    plot.Altitude = altitude;
                                    plot.Angle = roundedBearing;
                                    plot.Distance = distance.Value;
                                    plot.Latitude = latitude;
                                    plot.Longitude = longitude;
                                }
                            }
                        }
                    } finally {
                        _SlicesLock.Unlock();
                    }
                }
            }
        }
        #endregion

        #region TakeSnapshot
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <returns></returns>
        public List<PolarPlotSlice> TakeSnapshot()
        {
            using(var spinLock = _SlicesLock.AcquireLock()) {
                var result = new List<PolarPlotSlice>();
                foreach(var slice in _Slices) {
                    result.Add((PolarPlotSlice)slice.Clone());
                }

                return result;
            }
        }
        #endregion

        #region ClearPolarPlots
        /// <summary>
        /// See interface docs.
        /// </summary>
        public void ClearPolarPlots()
        {
            using(_SlicesLock.AcquireLock()) {
                foreach(var slice in _Slices) {
                    slice.PolarPlots.Clear();
                }
                InitialiseSlices();
            }
        }
        #endregion

        #region Events subscribed
        /// <summary>
        /// Called when the configuration is changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void ConfigurationStorage_ConfigurationChanged(object sender, EventArgs args)
        {
            using(_SlicesLock.AcquireLock()) {
                LoadConfiguration();
            }
        }
        #endregion
    }
}
