// Copyright © 2013 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

(function(VRS, /** jQuery= */ $, /** object= */ undefined)
{
    // Singleton declaration
    VRS.$$ = VRS.$$ || {};

    // Simple strings.
    /*
       Numbers in braces (e.g. the {0} in 'FL{0}') mark a point where a value will be substituted into the text.
       You can move these markers around (e.g. you can make it '{0}FL') but you must not remove them or alter
       the value within the braces. If you want to use an open or closing brace in these strings then you must
       enter two braces - e.g. to display {FL10} you would have to use '{{FL{0}}}'.

       Quotation marks (i.e. "") can be replaced with more appropriate values. If you want to use an apostrophe
       in the text then put a backslash before it (e.g. 'It\'s never too late') or put the entire text in
       double-quotes instead of single-quotes (e.g. "It's never too late").

       If you want to use a backslash in your text then enter two backslashes - e.g. '\\o/' will display as '\o/'.

       The semi-colon (;) at the end of each line is important. Leave those in place. Only translate the text
       within apostrophes.
    */

    // [[ MARKER START SIMPLE STRINGS ]]

    VRS.$$.Add =                                        '添加';
    VRS.$$.AddCondition =                               '添加情景';
    VRS.$$.AddCriteria =                                '添加规则';
    VRS.$$.AddFilter =                                  '添加过滤器';
    VRS.$$.AircraftNotTransmittingCallsign =            '航空器没有传输其呼号';
    VRS.$$.AircraftClass =                              '航空器类型';
    VRS.$$.Airport =                                    '机场';
    VRS.$$.AirportDataThumbnails =                      '缩略图 (来自airport-data.com)';
    VRS.$$.AllAltitudes =                               '所有高程';
    VRS.$$.AllRows =                                    '所有行';
    VRS.$$.Altitude =                                   '高程';
    VRS.$$.AltitudeAndSpeedGraph =                      '高程&速度图';
    VRS.$$.AltitudeAndVerticalSpeed =                   '高程&垂直速度';
    VRS.$$.AltitudeGraph =                              '高程图';
    VRS.$$.AllAircraft =                                '所有航空器';
    VRS.$$.Amphibian =                                  '两栖';
    VRS.$$.AnnounceSelected =                           '通知所有已选择航空器';
    VRS.$$.Ascending =                                  '升序';
    VRS.$$.AutoSelectAircraft =                         '自动选择航空器';
    VRS.$$.AverageSignalLevel =                         '信号平均水平面';
    VRS.$$.Bearing =                                    '承重';
    VRS.$$.Between =                                    '间隔';
    VRS.$$.Callsign =                                   '呼号';
    VRS.$$.CallsignAndShortRoute =                      '呼号&短路径';
    VRS.$$.CallsignMayNotBeCorrect =                    '可能不正确呼号';
    VRS.$$.CentreOnSelectedAircraft =                   '在地图上显示航空器';
    VRS.$$.Civil =                                      '民航';
    VRS.$$.CivilOrMilitary =                            '民航/空军';
    VRS.$$.ClosestToCurrentLocation =                   '距离当前最近';
    VRS.$$.CofACategory =                               'C/A(适航证) 类别';                     // certificate of airworthiness category
    VRS.$$.CofAExpiry =                                 'C/A(适航证) 期限';                       // certificate of airworthiness expiry
    VRS.$$.Columns =                                    '列';
    VRS.$$.Contains =                                   '包含';
    VRS.$$.CountAdsb =                                  'ADS-B计数';
    VRS.$$.Country =                                    '国家';
    VRS.$$.CountModeS =                                 'Mode-S计数';
    VRS.$$.CountPositions =                             '位置计数';
    VRS.$$.Criteria =                                   '规则';
    VRS.$$.CurrentLocationInstruction =                 '设置你的当前位置请单击 "设置当前位置" 并拖拽标记.';
    VRS.$$.CurrentRegDate =                             '当前段日期';
    VRS.$$.Date =                                       '日期';
    VRS.$$.DateTimeShort =                              '{0} {1}';                          // Where "{0}" is a date, e.g. 10/10/2013; and "{1}" is a time, e.g. 17:41:32.
    VRS.$$.DefaultSetting =                             '< Default >';
    VRS.$$.DegreesAbbreviation =                        '{0}°';
    VRS.$$.DeRegDate =                                  '注册日期';
    VRS.$$.DesktopPage =                                '桌面版本页面';
    VRS.$$.DesktopReportPage =                          '桌面版本报告页面';
    VRS.$$.DetailItems =                                '航空器详细项';
    VRS.$$.DetailPanel =                                '详情面板';
    VRS.$$.DisableAutoSelect =                          '禁止自动选择';
    VRS.$$.Distance =                                   '距离';
    VRS.$$.Distances =                                  '距离';
    VRS.$$.DoNotShow =                                  '不显示';
    VRS.$$.Duration =                                   '持续时间';
    VRS.$$.Electric =                                   '电力';
    VRS.$$.EnableAutoSelect =                           '开启自动选择';
    VRS.$$.EnableFilters =                              '启用过滤器';
    VRS.$$.EnableInfoWindow =                           '启用信息窗口';
    VRS.$$.End =                                        '结束';
    VRS.$$.EndTime =                                    '结束时间';
    VRS.$$.EndsWith =                                   '结束:';
    VRS.$$.Engines =                                    '引擎';
    VRS.$$.EngineType =                                 '引擎类型';
    VRS.$$.Equals =                                     '是';
    VRS.$$.Feet =                                       '英尺';
    VRS.$$.FeetAbbreviation =                           '{0} ft';
    VRS.$$.FeetPerMinuteAbbreviation =                  '{0} ft/m';
    VRS.$$.FeetPerSecondAbbreviation =                  '{0} ft/s';
    VRS.$$.FetchPage =                                  '获取';
    VRS.$$.FillOpacity =                                '填充透明度';
    VRS.$$.Filters =                                    '过滤器';
    VRS.$$.FindAllPermutationsOfCallsign =              '查询所有呼号排列';
    VRS.$$.FirstAltitude =                              '首次高程';
    VRS.$$.FirstHeading =                               '首次航向';
    VRS.$$.FirstFlightLevel =                           '首次飞行水平面';
    VRS.$$.FirstLatitude =                              '首次纬度';
    VRS.$$.FirstLongitude =                             '首次精度';
    VRS.$$.FirstOnGround =                              '首次地面位置';
    VRS.$$.FirstRegDate =                               '首次注册日期';
    VRS.$$.FirstSpeed =                                 '首次速度';
    VRS.$$.FirstSquawk =                                '首次呼叫';
    VRS.$$.FirstVerticalSpeed =                         '首次垂直速度';
    VRS.$$.FlightDetailShort =                          '详情';
    VRS.$$.FlightLevel =                                '飞行水平面';
    VRS.$$.FlightLevelAbbreviation =                    'FL{0}';
    VRS.$$.FlightLevelAndVerticalSpeed =                'FL & VSI';
    VRS.$$.FlightLevelHeightUnit =                      '飞行水平面高度单位';
    VRS.$$.FlightLevelTransitionAltitude =              '飞行水平面跃迁高程';
    VRS.$$.FlightsCount =                               '航班数量';
    VRS.$$.FlightsListShort =                           '航班';
    VRS.$$.FlightSimPage =                              '飞行模拟页面';
    VRS.$$.FlightSimTitle =                             'ADS-B Server';
    VRS.$$.ForcePhoneOff =                              '非移动设备';                      // As in "force the page to ignore the fact that this is a smart phone"
    VRS.$$.ForcePhoneOn =                               '移动设备';                          // As in "force the page to pretend that this is a smart phone"
    VRS.$$.ForceTabletOff =                             '非平板设备';                     // As in "force the page to ignore the fact that this is a tablet PC"
    VRS.$$.ForceTabletOn =                              '平板设备';                         // As in "force the page to use the settings for a tablet PC"
    VRS.$$.FromAltitude =                               '高程表 {0}';
    VRS.$$.FromToAltitude =                             '{0} 至 {1}';
    VRS.$$.FromToDate =                                 '{0} 至 {1}';
    VRS.$$.FromToFlightLevel =                          '{0} 至 {1}';
    VRS.$$.FromToSpeed =                                '{0} 至 {1}';
    VRS.$$.FromToSquawk =                               '{0} 至 {1}';
    VRS.$$.FurthestFromCurrentLocation =                '距离当前最远';
    VRS.$$.GenericName =                                '通用名';
    VRS.$$.GoogleMapsCouldNotBeLoaded =                 '地图无法加载';
    VRS.$$.GotoCurrentLocation =                        '转到当前位置';
    VRS.$$.GotoSelectedAircraft =                       '转到选择航空器';
    VRS.$$.GroundAbbreviation =                         'GND';
    VRS.$$.GroundVehicle =                              '地面车辆';
    VRS.$$.Gyrocopter =                                 '螺旋翼';
    VRS.$$.HadAlert =                                   '曾报警';
    VRS.$$.HadEmergency =                               '曾遇险';
    VRS.$$.HadSPI =                                     '曾被识别';                        // SPI is the name of a pulse in Mode-S, used when ATC has asked for ident from aircraft.
    VRS.$$.Heading =                                    '航向';
    VRS.$$.Heights =                                    '高度';
    VRS.$$.Helicopter =                                 '直升机';
    VRS.$$.Help =                                       '帮助';
    VRS.$$.HideAircraftNotOnMap =                       '在地图上隐藏航空器';
    VRS.$$.HideEmptyPinTextLines =                      '隐藏空标签线';
    VRS.$$.HideNoPosition =                             '包含位置';
    VRS.$$.HighContrastMap =                            '对比度';                         // <-- please try to keep this one short, it appears as a button on the map and there may not be a lot of room
    VRS.$$.Icao =                                       'ICAO';
    VRS.$$.Index =                                      '索引';
    VRS.$$.IsMilitary =                                 '空军';
    VRS.$$.Interesting =                                '关注';
    VRS.$$.IntervalSeconds =                            '更新间隔(秒)';
    VRS.$$.Jet =                                        '喷气式';
    VRS.$$.JustPositions =                              '位置';
    VRS.$$.KilometreAbbreviation =                      '{0} km';
    VRS.$$.Kilometres =                                 '公里';
    VRS.$$.KilometresPerHour =                          '公里/小时';
    VRS.$$.KilometresPerHourAbbreviation =              '{0} km/h';
    VRS.$$.Knots =                                      '节';
    VRS.$$.KnotsAbbreviation =                          '{0} kts';
    VRS.$$.LandPlane =                                  '陆上';
    VRS.$$.LastAltitude =                               '最后高程';
    VRS.$$.LastFlightLevel =                            '最后飞行水平面';
    VRS.$$.LastHeading =                                '最后航向';
    VRS.$$.LastOnGround =                               '最后地面位置';
    VRS.$$.LastLatitude =                               '最后纬度';
    VRS.$$.LastLongitude =                              '最后经度';
    VRS.$$.LastSpeed =                                  '最后速度';
    VRS.$$.LastSquawk =                                 '最后呼叫';
    VRS.$$.LastVerticalSpeed =                          '最后垂直速度';
    VRS.$$.Latitude =                                   '纬度';
    VRS.$$.Layout =                                     '布局';
    VRS.$$.Layout1 =                                    '经典';
    VRS.$$.Layout2 =                                    '高详情, 地图在上';
    VRS.$$.Layout3 =                                    '高详情, 地图在下';
    VRS.$$.Layout4 =                                    '高列表, 地图在上';
    VRS.$$.Layout5 =                                    '高列表, 地图在下';
    VRS.$$.Layout6 =                                    '高详情和列表';
    VRS.$$.ListAircraftClass =                          'A/C(适航证) 类型';
    VRS.$$.ListAirportDataThumbnails =                  '缩略图 (来自airport-data.com)';
    VRS.$$.ListAltitude =                               '高程';
    VRS.$$.ListAltitudeAndVerticalSpeed =               '高程&垂直速度';
    VRS.$$.ListAverageSignalLevel =                     '信号平均水平面';
    VRS.$$.ListBearing =                                '承重';
    VRS.$$.ListCallsign =                               '呼号';
    VRS.$$.ListCivOrMil =                               '民航/空军';
    VRS.$$.ListCofACategory =                           'C/A(适航证) 类别';                 // Certificate of airworthiness category
    VRS.$$.ListCofAExpiry =                             'C/A(适航证) 期限';               // Certificate of airworthiness expiry
    VRS.$$.ListCountAdsb =                              'ADS-B 消息';
    VRS.$$.ListCountMessages =                          '消息';
    VRS.$$.ListCountModeS =                             'Mode-S 消息';
    VRS.$$.ListCountPositions =                         '位置消息';
    VRS.$$.ListCountry =                                '国家';
    VRS.$$.ListCurrentRegDate =                         '当前注册日期';             // Date of current registration
    VRS.$$.ListDeRegDate =                              '准飞注册日期';              // as in the date it was taken off the register
    VRS.$$.ListDistance =                               '距离';
    VRS.$$.ListDuration =                               '持续时间';
    VRS.$$.ListEndTime =                                '最后消息';             // As in the date and time of the last message.
    VRS.$$.ListEngines =                                '引擎';
    VRS.$$.ListFirstAltitude =                          '首次高程';
    VRS.$$.ListFirstFlightLevel =                       '首次飞行水平面';
    VRS.$$.ListFirstHeading =                           '首次航向';
    VRS.$$.ListFirstLatitude =                          '首次纬度';
    VRS.$$.ListFirstLongitude =                         '首次经度';
    VRS.$$.ListFirstOnGround =                          '首次地面位置';
    VRS.$$.ListFirstRegDate =                           '首次注册';               // Date of first registration
    VRS.$$.ListFirstSpeed =                             '首次速度';
    VRS.$$.ListFirstSquawk =                            '首次呼叫';
    VRS.$$.ListFirstVerticalSpeed =                     '首次垂直速度';
    VRS.$$.ListFlightLevel =                            '飞行水平面';
    VRS.$$.ListFlightLevelAndVerticalSpeed =            '飞行水平面&垂直速度';
    VRS.$$.ListFlightsCount =                           '航空器数量';
    VRS.$$.ListGenericName =                            '通用名';
    VRS.$$.ListHadAlert =                               '曾报警';
    VRS.$$.ListHadEmergency =                           '曾遇险';
    VRS.$$.ListHadSPI =                                 '曾被识别';                      // Name of a pulse in Mode-S, may not need translation. Used when ATC has asked for ident from aircraft.
    VRS.$$.ListHeading =                                '航向';
    VRS.$$.ListIcao =                                   'ICAO标识';
    VRS.$$.ListInteresting =                            '关注';
    VRS.$$.ListLastAltitude =                           '最后高程';
    VRS.$$.ListLastFlightLevel =                        '最后飞行水平面';
    VRS.$$.ListLastHeading =                            '最后航向';
    VRS.$$.ListLastLatitude =                           '最后纬度';
    VRS.$$.ListLastLongitude =                          '最后经度';
    VRS.$$.ListLastOnGround =                           '最后地面位置';
    VRS.$$.ListLastSpeed =                              '最后速度';
    VRS.$$.ListLastSquawk =                             '最后呼叫';
    VRS.$$.ListLastVerticalSpeed =                      '最后垂直速度';
    VRS.$$.ListLatitude =                               '纬度';
    VRS.$$.ListLongitude =                              '经度';
    VRS.$$.ListNotes =                                  '注意';
    VRS.$$.ListManufacturer =                           '制造商';
    VRS.$$.ListMaxTakeoffWeight =                       '最大起飞承重';
    VRS.$$.ListModel =                                  '型号';
    VRS.$$.ListModelIcao =                              '类型';
    VRS.$$.ListModeSCountry =                           'Mode-S 国家';
    VRS.$$.ListModelSilhouette =                        '头像';
    VRS.$$.ListModelSilhouetteAndOpFlag =               '标志';
    VRS.$$.ListOperator =                               '操作员';
    VRS.$$.ListOperatorFlag =                           '标志';
    VRS.$$.ListOperatorIcao =                           '操作员ICAO标识';
    VRS.$$.ListOwnershipStatus =                        '所属状态';
    VRS.$$.ListPicture =                                '图片';
    VRS.$$.ListPopularName =                            '俗名';
    VRS.$$.ListPreviousId =                             '曾用ID';
    VRS.$$.ListReceiver =                               '接收器';
    VRS.$$.ListRegistration =                           '注册';
    VRS.$$.ListRowNumber =                              '行';
    VRS.$$.ListRoute =                                  '航线';
    VRS.$$.ListSerialNumber =                           '串口';
    VRS.$$.ListSignalLevel =                            '信号水平面';
    VRS.$$.ListSpecies =                                '特殊';
    VRS.$$.ListSpeed =                                  '速度';
    VRS.$$.ListSquawk =                                 '呼号';
    VRS.$$.ListStartTime =                              '开始时间';
    VRS.$$.ListStatus =                                 '状态';
    VRS.$$.ListTotalHours =                             '小时总计';
    VRS.$$.ListUserTag =                                '标记';
    VRS.$$.ListVerticalSpeed =                          '垂直速度';
    VRS.$$.ListWtc =                                    'WTC';
    VRS.$$.ListYearBuilt =                              '制造';
    VRS.$$.Longitude =                                  '经度';
    VRS.$$.Manufacturer =                               '制造商';
    VRS.$$.Map =                                        '地图';
    VRS.$$.MaxTakeoffWeight =                           '最大起飞承重';
    VRS.$$.Menu =                                       '菜单';
    VRS.$$.MenuBack =                                   '返回';
    VRS.$$.MessageCount =                               '消息计数';
    VRS.$$.MetreAbbreviation =                          '{0} m';
    VRS.$$.MetrePerSecondAbbreviation =                 '{0} m/sec';
    VRS.$$.MetrePerMinuteAbbreviation =                 '{0} m/min';
    VRS.$$.Metres =                                     '米';
    VRS.$$.MilesPerHour =                               '英里/小时';
    VRS.$$.MilesPerHourAbbreviation =                   '{0} mph';
    VRS.$$.Military =                                   '空军';
    VRS.$$.MobilePage =                                 '移动设备页面';
    VRS.$$.MobileReportPage =                           '移动设备报告页面';
    VRS.$$.Model =                                      '型号';
    VRS.$$.ModelIcao =                                  '型号代码';
    VRS.$$.ModeSCountry =                               'Mode-S 国家';
    VRS.$$.MovingMap =                                  '移动地图';
    VRS.$$.MuteOff =                                    '静音关闭';
    VRS.$$.MuteOn =                                     '静音开启';
    VRS.$$.NauticalMileAbbreviation =                   '{0} nmi';
    VRS.$$.NauticalMiles =                              '海里';
    VRS.$$.No =                                         '否';
    VRS.$$.NoLocalStorage = '该浏览器不支持本地存储. 您的配置信息将不能保存.\n\n如果您在"私人模式"访问请尝试转换关闭状态. 私人模式在某些浏览器上不能进行本地存储。';
    VRS.$$.None =                                       '无';
    VRS.$$.Notes =                                      '注意';
    VRS.$$.NoSettingsFound =                            '设置未找到';
    VRS.$$.NotBetween =                                 '不在之间';
    VRS.$$.NotContains =                                '不包含';
    VRS.$$.NotEndsWith =                                '不以结尾';
    VRS.$$.NotEquals =                                  '不是';
    VRS.$$.NotStartsWith =                              '不以开始';
    VRS.$$.OffRadarAction =                             '当航空器超出范围:';
    VRS.$$.OffRadarActionWait =                         '取消选择航空器';
    VRS.$$.OffRadarActionEnableAutoSelect =             '启用自动选择';
    VRS.$$.OffRadarActionNothing =                      '空';
    VRS.$$.OfPages =                                    ': {0:N0}';                            // As in "1 of 10" pages
    VRS.$$.OnlyAircraftOnMap =                          '仅列出可显示';
    VRS.$$.OnlyAutoSelected =                           '仅通知自动选择航班详细';
    VRS.$$.Operator =                                   '操作员';
    VRS.$$.OperatorCode =                               '操作员代码';
    VRS.$$.OperatorFlag =                               '操作员标志';
    VRS.$$.Options =                                    '选项';
    VRS.$$.OwnershipStatus =                            '所属状态';
    VRS.$$.PageAircraft =                               '航空器';
    VRS.$$.AircraftDetailShort =                        '详情';
    VRS.$$.PageFirst =                                  '首页';
    VRS.$$.PageGeneral =                                '普通页';
    VRS.$$.PageLast =                                   '尾页';
    VRS.$$.PageList =                                   '列表页';
    VRS.$$.PageListShort =                              '列表';
    VRS.$$.PageMapShort =                               '地图';
    VRS.$$.PageNext =                                   '下一页';
    VRS.$$.PagePrevious =                               '前一页';
    VRS.$$.PaneAircraftDisplay =                        '航空器显示';
    VRS.$$.PaneAircraftTrails =                         '航空器轨迹';
    VRS.$$.PaneAudio =                                  '声音';
    VRS.$$.PaneAutoSelect =                             '自动选择';
    VRS.$$.PaneCurrentLocation =                        '当前位置';
    VRS.$$.PaneDataFeed =                               '数据源';
    VRS.$$.PaneDetailSettings =                         '航空器详情';
    VRS.$$.PaneInfoWindow =                             '航空器信息窗口';
    VRS.$$.PaneListSettings =                           '列表设置';
    VRS.$$.PaneManyAircraft =                           '多重航空器报告';
    VRS.$$.PanePermanentLink =                          '永久链接';
    VRS.$$.PaneRangeCircles =                           '范围圈';
    VRS.$$.PaneReceiverRange =                          '接收器范围';
    VRS.$$.PaneSingleAircraft =                         '单个航空器报告';
    VRS.$$.PaneSortAircraftList =                       '排序航空器列表';
    VRS.$$.PaneSortReport =                             '排序报告';
    VRS.$$.PaneUnits =                                  '单位';
    VRS.$$.Pause =                                      '暂停';
    VRS.$$.PinTextNumber =                              '航空器标签线 {0}';
    VRS.$$.PopularName =                                '俗称';
    VRS.$$.PositionAndAltitude =                        '位置和高程';
    VRS.$$.PositionAndSpeed =                           '位置和速度';
    VRS.$$.Picture =                                    '图片';
    VRS.$$.PictureOrThumbnails =                        '图片或缩略图';
    VRS.$$.PinTextLines =                               '标签线数量';
    VRS.$$.Piston =                                     '活塞';
    VRS.$$.Pixels =                                     '像素';
    VRS.$$.PoweredByVRS =                               '宏林航空提供技术支持';
    VRS.$$.PreviousId =                                 '曾用ID';
    VRS.$$.Quantity =                                   '数量';
    VRS.$$.RadioMast =                                  '天线';
    VRS.$$.RangeCircleEvenColour =                      '偶数圈颜色';
    VRS.$$.RangeCircleOddColour =                       '奇数圈颜色';
    VRS.$$.RangeCircles =                               '圈范围';
    VRS.$$.Receiver =                                   '接收器';
    VRS.$$.ReceiverRange =                              '接收器范围';
    VRS.$$.Refresh =                                    '刷新';
    VRS.$$.Registration =                               '注册';
    VRS.$$.RegistrationAndIcao =                        '注册和ICAO标识';
    VRS.$$.Remove =                                     '删除';
    VRS.$$.RemoveAll =                                  '删除所有';
    VRS.$$.ReportCallsignInvalid =                      '呼号报告';
    VRS.$$.ReportCallsignValid =                        '呼号报告: {0}';
    VRS.$$.ReportEmpty =                                '没有找到符合该规则的记录';
    VRS.$$.ReportFreeForm =                             '自由格式报告';
    VRS.$$.ReportIcaoInvalid =                          'ICAO 报告';
    VRS.$$.ReportIcaoValid =                            'ICAO 报告: {0}';
    VRS.$$.ReportRegistrationInvalid =                  '注册报告';
    VRS.$$.ReportRegistrationValid =                    '注册报告: {0}';
    VRS.$$.ReportTodaysFlights =                        '当日航班';
    VRS.$$.ReportYesterdaysFlights =                    '昨日航班';
    VRS.$$.Reports =                                    '报告';
    VRS.$$.ReportsAreDisabled =                         '服务器权限禁止运行报告';
    VRS.$$.Resume =                                     '恢复';
    VRS.$$.Route =                                      '航线';
    VRS.$$.RouteShort =                                 '航线 (短)';
    VRS.$$.RouteFull =                                  '航线 (全)';
    VRS.$$.RouteNotKnown =                              '位置航线';
    VRS.$$.RowNumber =                                  '行数';
    VRS.$$.Rows =                                       '行';
    VRS.$$.RunReport =                                  '运行报告';
    VRS.$$.SeaPlane =                                   '水上';
    VRS.$$.Select =                                     '选择';
    VRS.$$.SeparateTwoValues =                          ' 和 ';
    VRS.$$.SerialNumber =                               '串口';
    VRS.$$.ServerFetchFailedTitle =                     '获取失败';
    VRS.$$.ServerFetchFailedBody =                      '无法从服务器获取. 错误 "{0}" 状态 "{1}".';
    VRS.$$.ServerFetchTimedOut =                        '请求超时.';
    VRS.$$.ServerReportExceptionBody =                  '在生成报告时服务器遇到异常. 异常 "{0}"';
    VRS.$$.ServerReportExceptionTitle =                 '服务器异常';
    VRS.$$.SetCurrentLocation =                         '设置当前位置';
    VRS.$$.Settings =                                   '设置';
    VRS.$$.SettingsPage =                               '设置页面';
    VRS.$$.Shortcuts =                                  '快捷键';
    VRS.$$.ShowAltitudeStalk =                          '显示高程线';
    VRS.$$.ShowCurrentLocation =                        '显示当前位置';
    VRS.$$.ShowDetail =                                 '显示详情';
    VRS.$$.ShowForAllAircraft =                         '显示所有航空器';
    VRS.$$.ShowEmptyValues =                            '显示空值';
    VRS.$$.ShowForSelectedOnly =                        '仅显示选择的航空器';
    VRS.$$.ShowRangeCircles =                           '显示范围圈';
    VRS.$$.ShowShortTrails =                            '显示短轨迹';
    VRS.$$.ShowUnits =                                  '显示单位';
    VRS.$$.ShowVsiInSeconds =                           '显示每秒垂直速度';
    VRS.$$.SignalLevel =                                '信号水平面';
    VRS.$$.Silhouette =                                 '头像';
    VRS.$$.SilhouetteAndOpFlag =                        '头像和标志';
    VRS.$$.SiteTimedOut =                               '在非交互期间该站点暂停. 关闭消息框恢复更新站点.';
    VRS.$$.SortBy =                                     '排序';
    VRS.$$.Species =                                    '特殊';
    VRS.$$.Speed =                                      '速度';
    VRS.$$.SpeedGraph =                                 '速度图';
    VRS.$$.Speeds =                                     '速度';
    VRS.$$.Squawk =                                     '呼叫';
    VRS.$$.Start =                                      '开始';
    VRS.$$.StartsWith =                                 '开始:';
    VRS.$$.StartTime =                                  '开始时间';
    VRS.$$.Status =                                     '规定';
    VRS.$$.StatuteMileAbbreviation =                    '{0} mi';
    VRS.$$.StatuteMiles =                               '规定英里';
    VRS.$$.StorageEngine =                              '引擎舱';
    VRS.$$.StorageSize =                                '舱尺寸';
    VRS.$$.StrokeOpacity =                              '冲程';
    VRS.$$.SubmitRoute =                                '提交航线';
    VRS.$$.SubmitRouteCorrection =                      '提交航线修正';
    VRS.$$.SuppressAltitudeStalkWhenZoomedOut =         '放大超过范围时限制高程线';
    VRS.$$.ThenBy =                                     '继续';
    VRS.$$.Tiltwing =                                   '倾斜翼';
    VRS.$$.TimeTracked =                                '时间跟踪';
    VRS.$$.TitleAircraftDetail =                        '航空器详情';
    VRS.$$.TitleAircraftList =                          '航空器列表';
    VRS.$$.TitleFlightDetail =                          '详情';
    VRS.$$.TitleFlightsList =                           '航班';
    VRS.$$.ToAltitude =                                 '至 {0}';
    VRS.$$.TitleSiteTimedOut =                          '超时';
    VRS.$$.TotalHours =                                 '小时总计';
    VRS.$$.TrackingCountAircraft =                      '跟踪 {0:N0} 航空器';
    VRS.$$.TrackingCountAircraftOutOf =                 '跟踪 {0:N0} 航空器 (越界 {1:N0})';
    VRS.$$.Turbo =                                      '涡轮增压';
    VRS.$$.UseBrowserLocation =                         '使用GPS定位';
    VRS.$$.UseRelativeDates =                           '使用相关日期';
    VRS.$$.UserTag =                                    '使用标签';
    VRS.$$.VerticalSpeed =                              '垂直速度';
    VRS.$$.VirtualRadar =                               'ADS-B Server';
    VRS.$$.Volume25 =                                   '音量 25%';
    VRS.$$.Volume50 =                                   '音量 50%';
    VRS.$$.Volume75 =                                   '音量 75%';
    VRS.$$.Volume100 =                                  '音量 100%';
    VRS.$$.VrsVersion =                                 '版本 {0}';
    VRS.$$.WakeTurbulenceCategory =                     '尾流';
    VRS.$$.Warning =                                    '警告';
    VRS.$$.WorkingInOfflineMode =                       '工作在离线模式';
    VRS.$$.WtcLight =                                   '轻微';
    VRS.$$.WtcMedium =                                  '中等';
    VRS.$$.WtcHeavy =                                   '严重';
    VRS.$$.YearBuilt =                                  '制造年份';
    VRS.$$.Yes =                                        '是';

    // Date picker text
    VRS.$$.DateClose =                                  '完成';                         // Keep this short
    VRS.$$.DateCurrent =                                '今天';                        // Keep this short
    VRS.$$.DateNext =                                   '下一页';                         // Keep this short
    VRS.$$.DatePrevious =                               '前一页';                         // Keep this short
    VRS.$$.DateWeekAbbr =                               '周';                           // Keep this very short
    VRS.$$.DateYearSuffix =                             '年';                             // This is displayed after the year
    // If your language has a different month format when days preceed months, and the date picker
    // should be using that month format, then set this to true. Otherwise leave at false.
    VRS.$$.DateUseGenetiveMonths =                      false;

    // Text-to-speech formatting
    VRS.$$.SayCallsign =                                '呼号 {0}.';
    VRS.$$.SayHyphen =                                  '连字符';
    VRS.$$.SayIcao =                                    'ICOA标识 {0}.';
    VRS.$$.SayModelIcao =                               '类型 {0}.';
    VRS.$$.SayOperator =                                '操作员 {0}.';
    VRS.$$.SayRegistration =                            '注册 {0}.';
    VRS.$$.SayRouteNotKnown =                           '航线未知.';
    VRS.$$.SayFromTo =                                  '行程从 {0} 至 {1}.';
    VRS.$$.SayFromToVia =                               '行程从 {0} 经过 {1} 至 {2}.';

    VRS.$$.SayAlpha =                                   'alfuh';
    VRS.$$.SayBravo =                                   'bravo';
    VRS.$$.SayCharlie =                                 'charlie';
    VRS.$$.SayDelta =                                   'delta';
    VRS.$$.SayEcho =                                    'echo';
    VRS.$$.SayFoxtrot =                                 'foxed-rot';
    VRS.$$.SayGolf =                                    'golf';
    VRS.$$.SayHotel =                                   'hotel';
    VRS.$$.SayIndia =                                   'india';
    VRS.$$.SayJuliet =                                  'juliet';
    VRS.$$.SayKilo =                                    'key-low';
    VRS.$$.SayLima =                                    'leamah';
    VRS.$$.SayMike =                                    'mike';
    VRS.$$.SayNovember =                                'november';
    VRS.$$.SayOscar =                                   'oscar';
    VRS.$$.SayPapa =                                    'papa';
    VRS.$$.SayQuebec =                                  'quebec';
    VRS.$$.SayRomeo =                                   'romeo';
    VRS.$$.SaySierra =                                  'sierra';
    VRS.$$.SayTango =                                   'tango';
    VRS.$$.SayUniform =                                 'uniform';
    VRS.$$.SayVictor =                                  'victor';
    VRS.$$.SayWhiskey =                                 'whiskey';
    VRS.$$.SayXRay =                                    'x-ray';
    VRS.$$.SayYankee =                                  'yankee';
    VRS.$$.SayZulu =                                    'zulu';

    // [[ MARKER END SIMPLE STRINGS ]]

    /*
        See the notes below against 'Complicated strings'. This function takes an array of stopovers in a route and
        joins them together into a single sentence for the text-to-speech conversion. So if it is passed an arary
        of "First stopover", "Second stopover" and "Third stopover" then it will return the string
        "First stopover, Second stopover and Third stopover".
     */
    VRS.$$.sayStopovers = function(stopovers)
    {
        var result = '';
        var length = stopovers.length;
        for(var i = 0;i < length;++i) {
            var stopover = stopovers[i];
            var isFirstStopover = i === 0;
            var isLastStopover = i + 1 === length;
            var isMiddleStopover = !isFirstStopover && !isLastStopover;

            if(isLastStopover)        result += ' 和 ';
            else if(isMiddleStopover) result += ', ';

            result += stopover;
        }

        return result;
    };

    // Complicated strings
    /*
       These are javascript functions that take a number of parameters and format some text from them. A function is
       always of this form:
         VRS.$$.<name of function> = function(parameter 1, parameter 2, ..., parameter n)
         {
            ... body of function ...
         };
       Only translate text within single apostrophes in functions. If the English version of a function will suffice
       for your language then delete the function entirely so that the site falls back onto the English version.

       If you are not comfortable with translating text within functions then let me know how the text should be
       displayed in your language and I'll do the function for you.
    */

    /**
     * Returns an elapsed time as a string.
     * @param {number} hours
     * @param {number} minutes
     * @param {number} seconds
     * @param {bool=} showZeroHours
     * @returns {string}
     */
    VRS.$$.formatHoursMinutesSeconds = function(hours, minutes, seconds, showZeroHours)
    {
        /*
            jQuery Globalize only allows formatting of full date-times, which is no good when we want to display spans
            of time larger than 24 hours. The English version of this returns either H:MM:SS or MM:SS depending on
            whether hours is zero and whether showZeroHours is true or false.
        */
        var result = '';
        if(hours || showZeroHours) result = hours.toString() + ':';
        result += VRS.stringUtility.formatNumber(minutes, '00') + ':';
        result += VRS.stringUtility.formatNumber(seconds, '00');

        return result;
    };

    /**
     * Returns the count of engines and the engine type as a translated string.
     * @param {string} countEngines
     * @param {string} engineType
     * @returns {string}
     */
    VRS.$$.formatEngines = function(countEngines, engineType)
    {
        /*
           Returns a string showing the count of engines and the engine type. Examples in English are:
             countEngines = '1' and engine type = VRS.EngineType.Jet:     'Single jet'
             countEngines = '10' and engine type = VRS.EngineType.Piston: '10 piston'
        */
        var result = '';

        switch(countEngines) {
            case 'C':       result = '耦合'; break;
            case '1':       result = '独立'; break;
            case '2':       result = '成对'; break;
            case '3':       result = '三个'; break;
            case '4':       result = '四个'; break;
            case '5':       result = '五个'; break;
            case '6':       result = '六个'; break;
            case '7':       result = '七个'; break;
            case '8':       result = '八个'; break;
            default:        result = countEngines; break;
        }

        switch(engineType) {
            case VRS.EngineType.Electric:   result += ' 电力'; break;
            case VRS.EngineType.Jet:        result += ' 喷气'; break;
            case VRS.EngineType.Piston:     result += ' 活塞'; break;
            case VRS.EngineType.Turbo:      result += ' 涡轮增压'; break;
        }

        return result;
    };

    /**
     * Translates the wake turbulence category description.
     * @param {string} category
     * @param {bool} ignoreNone
     * @param {bool} expandedDescription
     * @returns {string}
     */
    VRS.$$.formatWakeTurbulenceCategory = function(category, ignoreNone, expandedDescription)
    {
        /*
           Returns a string showing the wake turbulence category. What makes this different from a simple
           substitution is that in some places I want to show the weight limits for each category. In
           English these follow the category - e.g. Light (up to 7 tons) - but this may not be appropriate
           in other locales.
        */

        var result = '';
        if(category) {
            switch(category) {
                case VRS.WakeTurbulenceCategory.None:   if(!ignoreNone) result = '无'; break;
                case VRS.WakeTurbulenceCategory.Light:  result = '轻微'; break;
                case VRS.WakeTurbulenceCategory.Medium: result = '中等'; break;
                case VRS.WakeTurbulenceCategory.Heavy:  result = '严重'; break;
                default: throw '未知尾流类型 ' + category;  // Do not translate this line
            }

            if(expandedDescription && result) {
                switch(category) {
                    case VRS.WakeTurbulenceCategory.Light:  result += ' (达到七吨)'; break;
                    case VRS.WakeTurbulenceCategory.Medium: result += ' (达到一百三十五吨)'; break;
                    case VRS.WakeTurbulenceCategory.Heavy:  result += ' (超过一百三十五吨)'; break;
                }
            }
        }

        return result;
    };

    /**
     * Returns the full route details.
     * @param {string} from
     * @param {string} to
     * @param {string[]} via
     * @returns {string}
     */
    VRS.$$.formatRoute = function(from, to, via)
    {
        /*
            Returns a string showing the full route. From and to are strings describing the airport (in English - these
            come out of a database of thousands of English airport descriptions, it would be a nightmare to translate them)
            and via is an array of strings describing airports. In English the end result would be one of:
                From AIRPORT to AIRPORT
                To AIRPORT
                From AIRPORT to AIRPORT via AIRPORT
                From AIRPORT to AIRPORT via AIRPORT, AIRPORT (..., AIRPORT)
                To AIRPORT via AIRPORT
                To AIRPORT via AIRPORT, AIRPORT (..., AIRPORT)
         */
        var result = '';
        if(from) result = '从 ' + from;
        if(to) {
            if(result.length) result += ' 到 ';
            else              result = '到 ';
            result += to;
        }
        var stopovers = via ? via.length : 0;
        if(stopovers > 0) {
            result += ' 经过';
            for(var i = 0;i < stopovers;++i) {
                var stopover = via[i].val;
                if(i > 0) result += ',';
                result += ' ' + stopover;
            }
        }

        return result;
    };

    /**
     * Translates the country name.
     * @param {string} englishCountry
     * @returns {string}
     */
    VRS.$$.translateCountry = function(englishCountry)
    {
        /*
            Returns a translation of the country. If you are happy with English country names then just delete this function
            and the English version will be used. Otherwise you can delete the following line:
        */

        return englishCountry;

        /*
            and then remove the "//" from the start of every line after these comments and fill in the translations for the
            lines that start with 'case'. The format for every translated country should be:
                case 'English country name':  return 'ENTER YOUR TRANSLATION HERE';
            Unfortunately the English country names are coming out of the StandingData.sqb database file on the server. You
            can either extract them from there (if you can use sqlite3) or email me and I'll send you a version of this
            function with all of the English country names filled in for the current set of countries. You'll need to update
            this as-and-when the countries change. There are currently about 250 countries in the Standing Data database but
            any that you do not provide a translation for will just be shown in English. Delete the case lines for countries
            where your language's name is the same as the English name.

            If you need to use an apostrophe in your translation then change the single-quotes around the name to double-
            quotes, e.g.
                case 'Ivory Coast':     return "Republique de Cote d'Ivoire";
        */

        // switch(englishCountry) {
        //     case 'Germany':          return 'Allemagne';
        //     case 'United Kingdom':   return 'Grande-Bretagne';
        //     default:                 return englishCountry;
        // }
    };
}(window.VRS = window.VRS || {}, jQuery));
