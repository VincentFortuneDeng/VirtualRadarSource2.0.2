﻿/**
 * @license Copyright © 2013 onwards, Andrew Whewell
 * All rights reserved.
 *
 * Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
 *    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
 *    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
 *    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */
/**
 * @fileoverview A jQueryUI plugin that displays the location of an aircraft for the reports.
 */

(function(VRS, $, /** object= */ undefined)
{
    //region Global options
    VRS.globalOptions = VRS.globalOptions || {};
    VRS.globalOptions.reportMapClass = VRS.globalOptions.reportMapClass || 'reportMap';                                                                             // The class to use for the map widget container.
    VRS.globalOptions.reportMapScrollToAircraft = VRS.globalOptions.reportMapScrollToAircraft !== undefined ? VRS.globalOptions.reportMapScrollToAircraft : true;   // True if the map should automatically scroll to show the selected aircraft's path.
    VRS.globalOptions.reportMapShowPath = VRS.globalOptions.reportMapShowPath !== undefined ? VRS.globalOptions.reportMapShowPath : true;                           // True if a line should be drawn between the start and end points of the aircraft's path.
    VRS.globalOptions.reportMapStartSelected = VRS.globalOptions.reportMapStartSelected !== undefined ? VRS.globalOptions.reportMapStartSelected : false;           // True if the start point should be displayed in the selected colours, false if the end point should show in selected colours.
        //endregion

    //region ReportMapState
    /**
     * The state carried by the report map widget.
     * @constructor
     */
    VRS.ReportMapState = function()
    {
        /**
         * The element that contains the map.
         * @type {jQuery}
         */
        this.mapContainer = null;

        /**
         * The direct reference to the map plugin.
         * @type {VRS.vrsMap}
         */
        this.mapPlugin = null;

        /**
         * The plotter that this object uses to draw aircraft onto the map.
         * @type {VRS.AircraftPlotter}
         */
        this.aircraftPlotter = null;

        /**
         * The list of aircraft that we plot onto the map. It's just the same aircraft twice - once for the marker on
         * the start position and once for the marker on the end position.
         * @type {VRS.AircraftCollection}
         */
        this.aircraftList = new VRS.AircraftCollection();

        this.trailedFlights = null;

        /**
         * The next aircraft ID to assign to a fake VRS.Aircraft object.
         * @type {number}
         */
        this.nextAircraftId = 1;

        /**
         * The ID in the aircraft list of the fake VRS.Aircraft for the first marker to show for the aircraft.
         * @type {number}
         */
        this.firstPositionAircraftId = 0;

        /**
         * The ID in the aircraft list of the fake VRS.Aircraft for the last marker to show for the aircraft.
         * @type {number}
         */
        this.lastPositionAircraftId = 0;

        /**
         * The flight to display to the user, if any.
         * @type {VRS_JSON_REPORT_FLIGHT}
         */
        this.selectedFlight = null;

        /**
         * The entry in the aircraft list that pertains to the "selected" aircraft, i.e. the one we show in selected
         * colours. The end marker is always shown in selected colours.
         * @type {VRS.Aircraft}
         */
        this.selectedAircraft = null;

        /**
         * The hook result for the selected flight changed event.
         * @type {Object}
         */
        this.selectedFlightChangedHookResult = null;

        /**
         * The hook result for the selected flight changed event.
         * @type {Object}
         */
        //this.trailFetchedHookResult = null;

        /**
         * The hook result for the locale changed event.
         * @type {Object}
         */
        this.localeChangedHookResult = null;
    };
    //endregion

    //region jQueryUIHelper
    VRS.jQueryUIHelper = VRS.jQueryUIHelper || {};
    /**
     * Returns the report map widget attached to the jQuery element passed across.
     * @param {jQuery} jQueryElement
     * @returns {VRS.vrsReportMap}
     */
    VRS.jQueryUIHelper.getReportMapPlugin = function(jQueryElement) { return jQueryElement.data('vrsVrsReportMap'); };

    /**
     * Returns the default options for a report map widget with optional overrides.
     * @param {VRS_OPTIONS_REPORTMAP=} overrides
     * @returns {VRS_OPTIONS_REPORTMAP}
     */
    VRS.jQueryUIHelper.getReportMapOptions = function(overrides)
    {
        return $.extend({
            name:                       'default',                                      // The name to save state under.
            report:                     null,                                           // The report whose flights we're going to display. If no report is supplied then aircraft must be manually rendered, if it's supplied then it renders them automatically.
            plotterOptions:             null,                                           // The mandatory plotter options to use when plotting aircraft.
            elementClasses:             '',                                             // Additional classes to add to the element.
            unitDisplayPreferences:     undefined,                                      // The unit display preferences to use when displaying the marker.
            mapOptionOverrides:         {},                                             // Overrides to apply to the map.
            mapControls:                [],                                             // A list of controls to add to the map.
            scrollToAircraft:           VRS.globalOptions.reportMapScrollToAircraft,    // True if the map should automatically scroll to show the aircraft's path.
            showPath:                   VRS.globalOptions.reportMapShowPath,            // True if a line should be drawn between the start and end points of the aircraft's path.
            startSelected:              VRS.globalOptions.reportMapStartSelected,       // True if the start point should be shown in selected colours, false if the end point is shown in selected colours.

            __nop: null
        }, overrides);
    }
    //endregion

    //region vrsReportMap
    /**
     * A jQuery widget that can display a single aircraft's location on a map for a report.
     * @namespace VRS.vrsReportMap
     */
    $.widget('vrs.vrsReportMap', {
        //region -- options
        /** @type {VRS_OPTIONS_REPORTMAP} */
        options: VRS.jQueryUIHelper.getReportMapOptions(),
        //endregion

        //region -- Properties
        isOpen: function()
        {
            var state = this._getState();
            return state.mapPlugin && state.mapPlugin.isOpen();
        },
        //endregion

        //region -- _getState, _create, _destroy
        /**
         * Returns the state object for the plugin, creating it if it's not already there.
         * @returns {VRS.ReportMapState}
         * @private
         */
        _getState: function()
        {
            var result = this.element.data('reportMapState');
            if(result === undefined) {
                result = new VRS.ReportMapState();
                this.element.data('reportMapState', result);
            }

            return result;
        },

        /**
         * Creates the UI for the widget.
         * @private
         */
        _create: function()
        {
            var state = this._getState();
            var options = this.options;

            this.element.addClass(VRS.globalOptions.reportMapClass);
            if(options.elementClasses) this.element.addClass(options.elementClasses);
            this._createMap(state);
        },

        /**
         * Releases all of the resources allocated to the widget and undoes any UI changes.
         * @private
         */
        _destroy: function()
        {
            var state = this._getState();
            var options = this.options;

            /*if (state.trailFetchedHookResult && options.report) options.report.unhook(state.trailFetchedHookResult);
            state.trailFetchedHookResult = null;*/

            if (state.selectedFlightChangedHookResult && options.report) options.report.unhook(state.selectedFlightChangedHookResult);
            state.selectedFlightChangedHookResult = null;

            if(state.localeChangedHookResult && VRS.globalisation) VRS.globalisation.unhook(state.localeChangedHookResult);
            state.localeChangedHookResult = null;

            if(state.aircraftPlotter) state.aircraftPlotter.dispose();
            state.aircraftPlotter = null;

            if(state.mapPlugin) state.mapPlugin.destroy();
            if(state.mapContainer) state.mapContainer.remove();
            state.mapPlugin = state.mapContainer = null;

            this.element.removeClass(VRS.globalOptions.reportMapClass);
        },
        //endregion

        //region -- _createMap
        /**
         * Creates the map container and populates it with a map.
         * @param {VRS.ReportMapState} state
         * @private
         */
        _createMap: function(state)
        {
            var options = this.options;

            if(options.report) {
                state.selectedFlightChangedHookResult = options.report.hookSelectedFlightCHanged(this._selectedFlightChanged, this);
                //state.trailFetchedHookResult = options.report.hookTrailFetched(this._trailFetched, this);
            }

            /** @type {VRS_OPTIONS_MAP} */
            var mapOptions = {
                name:                   'report-' + options.name,
                scrollwheel:            true,
                draggable:              !VRS.globalOptions.isMobile,
                useServerDefaults:      true,
                loadMarkerWithLabel:    true,
                autoSaveState:          true,
                useStateOnOpen:         true,
                mapControls:            options.mapControls,
                controlStyle:           VRS.MapControlStyle.DropdownMenu
            };
            $.extend(mapOptions, options.mapOptionOverrides);
            mapOptions.afterOpen = $.proxy(this._mapCreated, this);

            state.mapContainer = $('<div/>')
                .appendTo(this.element);
            state.mapContainer.vrsMap(VRS.jQueryUIHelper.getMapOptions(mapOptions));
        },

        /**
         * Called once the map has been created. Completes the setup.
         * @private
         */
        _mapCreated: function()
        {
            var state = this._getState();
            var options = this.options;

            // It is possible for this to get called on an object that has been destroyed and resurrected. We can detect
            // when this happens - the container will no longer exist.
            if(state.mapContainer) {
                state.mapPlugin = VRS.jQueryUIHelper.getMapPlugin(state.mapContainer);

                if(!state.mapPlugin.isOpen()) {
                    if(options.mapControls) {
                        $.each(options.mapControls, function(/** number */ idx, /** VRS_MAP_CONTROL */ control) {
                            state.mapContainer.children().first().prepend(control.control);
                        });
                    }
                } else {
                    state.aircraftPlotter = new VRS.AircraftPlotter({
                        plotterOptions:         options.plotterOptions,
                        map:                    state.mapContainer,
                        unitDisplayPreferences: options.unitDisplayPreferences,
                        getAircraft:            $.proxy(this._getAircraft, this),
                        getSelectedAircraft:    $.proxy(this._getSelectedAircraft, this),
                        getCustomPinTexts:      function(/** VRS.Aircraft */ aircraft) {
                            return [ aircraft.id === state.firstPositionAircraftId ? VRS.$$.Start : VRS.$$.End ];
                        }
                    });
                }

                VRS.globalisation.hookLocaleChanged(this._localeChanged, this);
            }
        },
        //endregion

        //region -- _buildFakeVrsAircraft
        /**
         * Constructs a list of aircraft for the selected flight. It is this list that the plotter will end up showing
         * as markers on the map. The list can only have two entries at most - both for the same aircraft, the first
         * shows the start position and the second shows its final position. To distinguish between the two "aircraft"
         * they are given different IDs - ID 1 is the start position of the aircraft, ID 2 is the end position.
         * @params {VRS.ReportMapState} state
         * @private
         */
        _buildFakeVrsAircraft: function(state)//模拟航空器轨迹
        {
            var options = this.options;

            state.aircraftList = new VRS.AircraftCollection();
            state.selectedAircraft = null;

            var flight = state.selectedFlight;

            if(flight && ((flight.fLat && flight.fLng) || (flight.lLat && flight.lLng))) {
                var first = null, last = null;
                if(flight.fLat && flight.fLng) {
                    first = VRS.Report.convertFlightToVrsAircraft(flight, true);
                    first.id = state.firstPositionAircraftId = state.nextAircraftId++;
                    state.aircraftList[first.id] = first;
                }
                if(flight.lLat && flight.lLng) {
                    last = VRS.Report.convertFlightToVrsAircraft(flight, false);
                    last.id = state.lastPositionAircraftId = state.nextAircraftId++;
                    state.aircraftList[last.id] = last;
                }

                state.selectedAircraft = options.startSelected ? first : last;

                if (options.showPath && first && last) {
                    if (state.trailedFlights) {
                        //alert(state.trailedFlights.length);
                        var length = state.trailedFlights.length;
                        for (var i = 0; i < length; ++i) {
                            //alert(state.trailedFlights[i].lLat + '-' + state.trailedFlights[i].lLng.val + '-' + state.trailedFlights[i].lTrk.val + '-' + state.trailedFlights[i].lAlt.val + '-' + state.trailedFlights[i].lSpd.val);
                            last.fullTrail.arr.push(new VRS.FullTrailValue(state.trailedFlights[i].lLat, state.trailedFlights[i].lLng, state.trailedFlights[i].lTrk, state.trailedFlights[i].lAlt, state.trailedFlights[i].lSpd));
                        }
                        //alert(last.fullTrail.arr);
                        //alert(first.latitude.val + '-' + first.longitude.val + '-' + first.heading.val + '-' + first.altitude.val + '-' + first.speed.val);
                        //alert(last.latitude.val + '-' + last.longitude.val + '-' + last.heading.val + '-' + last.altitude.val + '-' + last.speed.val);
                        //alert('buildFakeVrsAircraft end');
                    }

                    else {
                        last.fullTrail.arr.push(new VRS.FullTrailValue(first.latitude.val, first.longitude.val, first.heading.val, first.altitude.val, first.speed.val));
                        last.fullTrail.arr.push(new VRS.FullTrailValue(last.latitude.val, last.longitude.val, last.heading.val, last.altitude.val, last.speed.val));
                        //alert("fullTrail");
                        //alert();
                    }
                }
            }
        },
        //endregion

        //region -- Events subscribed
        /**
         * Returns a collection of aircraft to plot on the map. See _buildFakeVrsAircraft.
         * @private
         * @returns {VRS.AircraftCollection}
         */
        _getAircraft: function()
        {
            var state = this._getState();
            return state.aircraftList;
        },

        /**
         * Returns the aircraft to paint in selected colours. See _buildFakeVrsAircraft.
         * @private
         * @returns {VRS.Aircraft}
         */
        _getSelectedAircraft: function()
        {
            var state = this._getState();
            return state.selectedAircraft;
        },

        /**
         * Called when the user chooses another language.
         * @private
         */
        _localeChanged: function()
        {
            var state = this._getState();
            if(state.aircraftPlotter) state.aircraftPlotter.plot(true, true);
        },

        /**
         * Called when the report indicates that the selected flight has been changed.
         * @private
         */
        _selectedFlightChanged: function()
        {
            //alert('_selectedFlightChanged Map');
            this.showFlight(this.options.report.getSelectedFlight(), this.options.report.getFlightTrails());
            //alert("Map selectedFlightChanged out");
        },

        /**
         * Called when the report indicates that the selected flight has been changed.
         * @private
         */
        /*_trailFetched: function ()
        {
            //alert("_trailFetched map");
            this.showFlight(this.options.report.getSelectedFlight(),this.options.report.getFlightTrails());
        },*/

        /**
         * Shows the flight's details on the map.
         * @param {VRS_JSON_REPORT_FLIGHT} flight
         */
        showFlight: function(flight,trails)
        {
            //alert("Map showFlight in");
            var state = this._getState();
            //("Map getState out");
            state.selectedFlight = flight;
            //alert(state.trailedFlights);
            //alert(this.options.report);
            state.trailedFlights = trails;
            
            //alert("Map getFlightTrails");

            var options = this.options;
            
            this._buildFakeVrsAircraft(state);
            
            var applyWhenReady = function() {
                var map = state.aircraftPlotter ? state.aircraftPlotter.getMap() : null;
                if(!map || !map.isReady()) {
                    setTimeout(applyWhenReady, 100);
                } else {
                    var fromAircraft = state.aircraftList.findAircraftById(state.firstPositionAircraftId);
                    var toAircraft = fromAircraft ? state.aircraftList.findAircraftById(state.lastPositionAircraftId) : null;

                    if(options.scrollToAircraft && (fromAircraft || toAircraft)) {
                        if(fromAircraft.hasPosition() && toAircraft.hasPosition() && (fromAircraft.latitude.val !== toAircraft.latitude.val || fromAircraft.longitude.val !== toAircraft.longitude.val)) {
                            var bounds = VRS.greatCircle.arrangeTwoPointsIntoBounds(
                                !fromAircraft ? null : { lat: fromAircraft.latitude.val, lng: fromAircraft.longitude.val },
                                !toAircraft ? null : { lat: toAircraft.latitude.val, lng: toAircraft.longitude.val }
                            );
                            if(bounds) map.fitBounds(bounds);
                        } else if(fromAircraft.hasPosition()) map.panTo(fromAircraft.getPosition());
                        else if(toAircraft.hasPosition()) map.panTo(toAircraft.getPosition());
                    }

                    state.aircraftPlotter.plot(true, true);
                }
            };

            applyWhenReady();
        },
        //endregion

        __nop: null
    });
    //endregion
}(window.VRS = window.VRS || {}, jQuery));