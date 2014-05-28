// This adds a new link to the aircraft detail panel (and any other places on the site that show aircraft links)

// Only proceed if the web page has link handlers loaded
if(VRS && VRS.LinkRenderHandler && VRS.linkRenderHandlers) {

    // Add our own entry to the enumeration of link sites
    VRS.LinkSite['ExamplePlugin'] = 'examplePlugin';

    // Add a simple link to our Index.html page to any panel that shows default aircraft links.
    // VRS.Aircraft is defined in aircraft.js in the standard VRS web site. The functions that
    // take an aircraft are passed the currently selected aircraft.
    VRS.linkRenderHandlers.push(
        new VRS.LinkRenderHandler({
            linkSite:           VRS.LinkSite.ExamplePlugin,                                     // <-- this must be the value you added to VRS.LinkSite above
            displayOrder:       10000,      // <-- If this is > 0 then the link is rendered anywhere that default aircraft links are used on the site
            canLinkAircraft:    function(/** VRS.Aircraft */ aircraft) { return true; },        // <-- see aircraft.js, return true if you want your link rendered for the aircraft passed across
            hasChanged:         function(/** VRS.Aircraft */ aircraft) { return false; },       // <-- see aircraft.js, return true if a value that you rely upon has changed on the aircraft
            title:              function(/** VRS.Aircraft */ aircraft) { return '示例'; },   // <-- see aircraft.js, return the text to show for the link.
            buildUrl:           function(/** VRS.Aircraft */ aircraft) { return 'Example/Index.html?icao=' + encodeURIComponent(aircraft.formatIcao()); },      // <-- see aircraft.js, return the HREF for the link
            target:             'example'                                                       // <-- return the target page for the link. Omit if you don't want a target (not recommended).
        })
    );
}