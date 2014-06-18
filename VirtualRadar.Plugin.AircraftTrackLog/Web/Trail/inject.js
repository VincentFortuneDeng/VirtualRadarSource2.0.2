// This adds a new link to the aircraft detail panel (and any other places on the site that show aircraft links)
//if(VRS.globalOptions.reportUrl) {
    //alert("Hello World!!");
    //VRS.globalDispatch.hook(VRS.globalEvent.bootstrapCreated, function(bootStrap) {
        //alert(VRS.globalOptions.isMobile);
        //VRS.jQueryUIHelper.getMenuPlugin($('#map'));
        //alert(bootStrap.globalOptions.reportUrl);
        //bootStrap.reportUrl = VRS.globalOptions.isMobile ? 'Trail/mobileReport.html' : 'Trail/desktopReport.html';
        //alert(bootStrap.reportUrl);
    //});
//}


//VRS.BootstrapMap.reportUrl = VRS.globalOptions.isMobile ? 'Trail/mobileReport.html' : 'Trail/desktopReport.html';

// Only proceed if the web page has link handlers loaded
if(VRS && VRS.LinkRenderHandler && VRS.linkRenderHandlers) {

    // Add our own entry to the enumeration of link sites
    VRS.LinkSite['AircraftTrackLogPlugin'] = 'aircraftTrackLogPlugin';

    // Add a simple link to our Index.html page to any panel that shows default aircraft links.
    // VRS.Aircraft is defined in aircraft.js in the standard VRS web site. The functions that
    // take an aircraft are passed the currently selected aircraft.
    VRS.linkRenderHandlers.push(
        new VRS.LinkRenderHandler({
            linkSite:           VRS.LinkSite.AircraftTrackLogPlugin,                                     // <-- this must be the value you added to VRS.LinkSite above
            displayOrder:       10000,      // <-- If this is > 0 then the link is rendered anywhere that default aircraft links are used on the site
            canLinkAircraft:    function(/** VRS.Aircraft */ aircraft) { return true; },        // <-- see aircraft.js, return true if you want your link rendered for the aircraft passed across
            hasChanged:         function(/** VRS.Aircraft */ aircraft) { return false; },       // <-- see aircraft.js, return true if a value that you rely upon has changed on the aircraft
            title:              function(/** VRS.Aircraft */ aircraft) { return '航空器历史轨迹'; },   // <-- see aircraft.js, return the text to show for the link.
            buildUrl:           function(/** VRS.Aircraft */ aircraft) { return encodeURIComponent(VRS.globalOptions.isMobile ? 'Trail/mobileReport.html' : 'Trail/desktopReport.html') +  '?icao-Q=' + encodeURIComponent(aircraft.formatIcao()) + '&sort1=' + encodeURIComponent(VRS.ReportSortColumn.Date) + '&sortAsc1=0&sort2=none'; },      // <-- see aircraft.js, return the HREF for the link
            /*'icao-Q':   selectedAircraft.icao.val,
                        'sort1':    VRS.ReportSortColumn.Date,
                        'sortAsc1': 0,
                        'sort2':    'none'*/
            target:             function(/** VRS.Aircraft */ aircraft) { return '历史轨迹' + encodeURIComponent(aircraft.formatIcao()); }                                                       // <-- return the target page for the link. Omit if you don't want a target (not recommended).
        })
    );
}