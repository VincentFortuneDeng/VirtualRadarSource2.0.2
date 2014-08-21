function LocalMapType() { }

LocalMapType.prototype.tileSize = new google.maps.Size(256, 256);
LocalMapType.prototype.maxZoom = 17;   //地图显示最大级别
LocalMapType.prototype.minZoom = 13;    //地图显示最小级别
LocalMapType.prototype.name = "离线地图";
LocalMapType.prototype.alt = "显示本地地图数据";
LocalMapType.prototype.getTile = function(coord, zoom, ownerDocument) {
    var img = ownerDocument.createElement("img");
    img.style.width = this.tileSize.width + "px";
    img.style.height = this.tileSize.height + "px";
    //地图存放路径
    //谷歌矢量图 maptile/googlemaps/roadmap
    //谷歌影像图 maptile/googlemaps/roadmap
    //MapABC地图 maptile/mapabc/
    //strURL = "maptile/googlemaps/roadmap/";

    mapPicDir = "maptile/googlemaps/roadmap/";
    var curSize = Math.pow(2, zoom);
    strURL = mapPicDir + zoom + "/" + (coord.x % curSize) + "/" + (coord.y % curSize) + ".JPG";
    //strURL = "E:/地图文件/谷歌地图中国0-12级googlemaps/googlemaps/roadmap/" + zoom + "/" + (coord.x % curSize )+ "/" + (coord.y % curSize)+ ".PNG";

    img.src = strURL;
    return img;
};