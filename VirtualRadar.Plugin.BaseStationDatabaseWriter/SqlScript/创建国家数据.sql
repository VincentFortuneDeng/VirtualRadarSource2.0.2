/*
Navicat SQLite Data Transfer

Source Server         : 参照数据
Source Server Version : 30714
Source Host           : :0

Target Server Type    : SQLite
Target Server Version : 30714
File Encoding         : 65001

Date: 2014-06-27 08:01:02
*/

PRAGMA foreign_keys = OFF;

-- ----------------------------
-- Table structure for Country
-- ----------------------------
DROP TABLE IF EXISTS "main"."Country";
CREATE TABLE [Country] (
     [CountryId]            INTEGER PRIMARY KEY AUTOINCREMENT
    ,[Name]                 NVARCHAR(80) COLLATE NOCASE NOT NULL
);

-- ----------------------------
-- Records of Country
-- ----------------------------
INSERT INTO "main"."Country" VALUES (1, 'Afghanistan');
INSERT INTO "main"."Country" VALUES (2, 'Albania');
INSERT INTO "main"."Country" VALUES (3, 'Algeria');
INSERT INTO "main"."Country" VALUES (4, 'Angola');
INSERT INTO "main"."Country" VALUES (5, 'Antigua and Barbuda');
INSERT INTO "main"."Country" VALUES (6, 'Argentina');
INSERT INTO "main"."Country" VALUES (7, 'Armenia');
INSERT INTO "main"."Country" VALUES (8, 'Aruba');
INSERT INTO "main"."Country" VALUES (9, 'Australia');
INSERT INTO "main"."Country" VALUES (10, 'Austria');
INSERT INTO "main"."Country" VALUES (11, 'Azerbaijan');
INSERT INTO "main"."Country" VALUES (12, 'Bahamas');
INSERT INTO "main"."Country" VALUES (13, 'Bahrain');
INSERT INTO "main"."Country" VALUES (14, 'Bangladesh');
INSERT INTO "main"."Country" VALUES (15, 'Barbados');
INSERT INTO "main"."Country" VALUES (16, 'Belarus');
INSERT INTO "main"."Country" VALUES (17, 'Belgium');
INSERT INTO "main"."Country" VALUES (18, 'Belize');
INSERT INTO "main"."Country" VALUES (19, 'Benin');
INSERT INTO "main"."Country" VALUES (20, 'Bermuda');
INSERT INTO "main"."Country" VALUES (21, 'Bhutan');
INSERT INTO "main"."Country" VALUES (22, 'Bolivia');
INSERT INTO "main"."Country" VALUES (23, 'Bosnia and Herzegovina');
INSERT INTO "main"."Country" VALUES (24, 'Botswana');
INSERT INTO "main"."Country" VALUES (25, 'Brazil');
INSERT INTO "main"."Country" VALUES (26, 'Brunei');
INSERT INTO "main"."Country" VALUES (27, 'Bulgaria');
INSERT INTO "main"."Country" VALUES (28, 'Burkina Faso');
INSERT INTO "main"."Country" VALUES (29, 'Burundi');
INSERT INTO "main"."Country" VALUES (30, 'Cambodia');
INSERT INTO "main"."Country" VALUES (31, 'Cameroon');
INSERT INTO "main"."Country" VALUES (32, 'Canada');
INSERT INTO "main"."Country" VALUES (33, 'Cape Verde');
INSERT INTO "main"."Country" VALUES (34, 'Cayman Islands');
INSERT INTO "main"."Country" VALUES (35, 'Central African Republic');
INSERT INTO "main"."Country" VALUES (36, 'Chad');
INSERT INTO "main"."Country" VALUES (37, 'Chile');
INSERT INTO "main"."Country" VALUES (38, 'China');
INSERT INTO "main"."Country" VALUES (39, 'Hong Kong');
INSERT INTO "main"."Country" VALUES (40, 'Macau');
INSERT INTO "main"."Country" VALUES (41, 'Colombia');
INSERT INTO "main"."Country" VALUES (42, 'Comoros');
INSERT INTO "main"."Country" VALUES (43, 'Congo (Kinshasa)');
INSERT INTO "main"."Country" VALUES (44, 'Congo (Brazzaville)');
INSERT INTO "main"."Country" VALUES (45, 'Cook Islands');
INSERT INTO "main"."Country" VALUES (46, 'Costa Rica');
INSERT INTO "main"."Country" VALUES (47, 'Côte d''Ivoire');
INSERT INTO "main"."Country" VALUES (48, 'Croatia');
INSERT INTO "main"."Country" VALUES (49, 'Cuba');
INSERT INTO "main"."Country" VALUES (50, 'Cyprus');
INSERT INTO "main"."Country" VALUES (51, 'Czech Republic');
INSERT INTO "main"."Country" VALUES (52, 'Denmark');
INSERT INTO "main"."Country" VALUES (53, 'Djibouti');
INSERT INTO "main"."Country" VALUES (54, 'Dominican Republic');
INSERT INTO "main"."Country" VALUES (55, 'Ecuador');
INSERT INTO "main"."Country" VALUES (56, 'Egypt');
INSERT INTO "main"."Country" VALUES (57, 'El Salvador');
INSERT INTO "main"."Country" VALUES (58, 'Equatorial Guinea');
INSERT INTO "main"."Country" VALUES (59, 'Eritrea');
INSERT INTO "main"."Country" VALUES (60, 'Estonia');
INSERT INTO "main"."Country" VALUES (61, 'Ethiopia');
INSERT INTO "main"."Country" VALUES (62, 'Falkland Islands');
INSERT INTO "main"."Country" VALUES (63, 'Fiji');
INSERT INTO "main"."Country" VALUES (64, 'Finland');
INSERT INTO "main"."Country" VALUES (65, 'France');
INSERT INTO "main"."Country" VALUES (66, 'Gabon');
INSERT INTO "main"."Country" VALUES (67, 'Gambia');
INSERT INTO "main"."Country" VALUES (68, 'Georgia');
INSERT INTO "main"."Country" VALUES (69, 'Germany');
INSERT INTO "main"."Country" VALUES (70, 'Ghana');
INSERT INTO "main"."Country" VALUES (71, 'Greece');
INSERT INTO "main"."Country" VALUES (72, 'Grenada');
INSERT INTO "main"."Country" VALUES (73, 'Guatemala');
INSERT INTO "main"."Country" VALUES (74, 'Guernsey');
INSERT INTO "main"."Country" VALUES (75, 'Guinea');
INSERT INTO "main"."Country" VALUES (76, 'Guinea-Bissau');
INSERT INTO "main"."Country" VALUES (77, 'Guyana');
INSERT INTO "main"."Country" VALUES (78, 'Haiti');
INSERT INTO "main"."Country" VALUES (79, 'Honduras');
INSERT INTO "main"."Country" VALUES (80, 'Hungary');
INSERT INTO "main"."Country" VALUES (81, 'NATO');
INSERT INTO "main"."Country" VALUES (82, 'ICAO');
INSERT INTO "main"."Country" VALUES (83, 'Iceland');
INSERT INTO "main"."Country" VALUES (84, 'India');
INSERT INTO "main"."Country" VALUES (85, 'Indonesia');
INSERT INTO "main"."Country" VALUES (86, 'Iran');
INSERT INTO "main"."Country" VALUES (87, 'Iraq');
INSERT INTO "main"."Country" VALUES (88, 'Ireland');
INSERT INTO "main"."Country" VALUES (89, 'Isle of Man');
INSERT INTO "main"."Country" VALUES (90, 'Israel');
INSERT INTO "main"."Country" VALUES (91, 'Italy');
INSERT INTO "main"."Country" VALUES (92, 'Jamaica');
INSERT INTO "main"."Country" VALUES (93, 'Japan');
INSERT INTO "main"."Country" VALUES (94, 'Jordan');
INSERT INTO "main"."Country" VALUES (95, 'Kazakhstan');
INSERT INTO "main"."Country" VALUES (96, 'Kenya');
INSERT INTO "main"."Country" VALUES (97, 'Kiribati');
INSERT INTO "main"."Country" VALUES (98, 'North Korea');
INSERT INTO "main"."Country" VALUES (99, 'South Korea');
INSERT INTO "main"."Country" VALUES (100, 'Kuwait');
INSERT INTO "main"."Country" VALUES (101, 'Kyrgyzstan');
INSERT INTO "main"."Country" VALUES (102, 'Laos');
INSERT INTO "main"."Country" VALUES (103, 'Latvia');
INSERT INTO "main"."Country" VALUES (104, 'Lebanon');
INSERT INTO "main"."Country" VALUES (105, 'Lesotho');
INSERT INTO "main"."Country" VALUES (106, 'Liberia');
INSERT INTO "main"."Country" VALUES (107, 'Libya');
INSERT INTO "main"."Country" VALUES (108, 'Lithuania');
INSERT INTO "main"."Country" VALUES (109, 'Luxembourg');
INSERT INTO "main"."Country" VALUES (110, 'Macedonia');
INSERT INTO "main"."Country" VALUES (111, 'Madagascar');
INSERT INTO "main"."Country" VALUES (112, 'Malawi');
INSERT INTO "main"."Country" VALUES (113, 'Malaysia');
INSERT INTO "main"."Country" VALUES (114, 'Maldives');
INSERT INTO "main"."Country" VALUES (115, 'Mali');
INSERT INTO "main"."Country" VALUES (116, 'Malta');
INSERT INTO "main"."Country" VALUES (117, 'Marshall Islands');
INSERT INTO "main"."Country" VALUES (118, 'Mauritania');
INSERT INTO "main"."Country" VALUES (119, 'Mauritius');
INSERT INTO "main"."Country" VALUES (120, 'Mexico');
INSERT INTO "main"."Country" VALUES (121, 'Micronesia');
INSERT INTO "main"."Country" VALUES (122, 'Moldova');
INSERT INTO "main"."Country" VALUES (123, 'Monaco');
INSERT INTO "main"."Country" VALUES (124, 'Mongolia');
INSERT INTO "main"."Country" VALUES (125, 'Montserrat');
INSERT INTO "main"."Country" VALUES (126, 'Montenegro');
INSERT INTO "main"."Country" VALUES (127, 'Morocco');
INSERT INTO "main"."Country" VALUES (128, 'Mozambique');
INSERT INTO "main"."Country" VALUES (129, 'Myanmar');
INSERT INTO "main"."Country" VALUES (130, 'Namibia');
INSERT INTO "main"."Country" VALUES (131, 'Nauru');
INSERT INTO "main"."Country" VALUES (132, 'Nepal');
INSERT INTO "main"."Country" VALUES (133, 'Netherlands');
INSERT INTO "main"."Country" VALUES (134, 'Netherlands Antilles');
INSERT INTO "main"."Country" VALUES (135, 'New Zealand');
INSERT INTO "main"."Country" VALUES (136, 'Nicaragua');
INSERT INTO "main"."Country" VALUES (137, 'Niger');
INSERT INTO "main"."Country" VALUES (138, 'Nigeria');
INSERT INTO "main"."Country" VALUES (139, 'Norway');
INSERT INTO "main"."Country" VALUES (140, 'Oman');
INSERT INTO "main"."Country" VALUES (141, 'Pakistan');
INSERT INTO "main"."Country" VALUES (142, 'Palau');
INSERT INTO "main"."Country" VALUES (143, 'Panama');
INSERT INTO "main"."Country" VALUES (144, 'Papua New Guinea');
INSERT INTO "main"."Country" VALUES (145, 'Paraguay');
INSERT INTO "main"."Country" VALUES (146, 'Perú');
INSERT INTO "main"."Country" VALUES (147, 'Philippines');
INSERT INTO "main"."Country" VALUES (148, 'Poland');
INSERT INTO "main"."Country" VALUES (149, 'Portugal');
INSERT INTO "main"."Country" VALUES (150, 'Qatar');
INSERT INTO "main"."Country" VALUES (151, 'Romania');
INSERT INTO "main"."Country" VALUES (152, 'Russia');
INSERT INTO "main"."Country" VALUES (153, 'Rwanda');
INSERT INTO "main"."Country" VALUES (154, 'Samoa');
INSERT INTO "main"."Country" VALUES (155, 'San Marino');
INSERT INTO "main"."Country" VALUES (156, 'São Tomé and Principe');
INSERT INTO "main"."Country" VALUES (157, 'Saudi Arabia');
INSERT INTO "main"."Country" VALUES (158, 'Senegal');
INSERT INTO "main"."Country" VALUES (159, 'Serbia');
INSERT INTO "main"."Country" VALUES (160, 'Seychelles');
INSERT INTO "main"."Country" VALUES (161, 'Sierra Leone');
INSERT INTO "main"."Country" VALUES (162, 'Singapore');
INSERT INTO "main"."Country" VALUES (163, 'Slovakia');
INSERT INTO "main"."Country" VALUES (164, 'Slovenia');
INSERT INTO "main"."Country" VALUES (165, 'Solomon Islands');
INSERT INTO "main"."Country" VALUES (166, 'Somalia');
INSERT INTO "main"."Country" VALUES (167, 'South Africa');
INSERT INTO "main"."Country" VALUES (168, 'Spain');
INSERT INTO "main"."Country" VALUES (169, 'Sri Lanka');
INSERT INTO "main"."Country" VALUES (170, 'Saint Lucia');
INSERT INTO "main"."Country" VALUES (171, 'Saint Vincent and the Grenadines');
INSERT INTO "main"."Country" VALUES (172, 'Sudan');
INSERT INTO "main"."Country" VALUES (173, 'Suriname');
INSERT INTO "main"."Country" VALUES (174, 'Swaziland');
INSERT INTO "main"."Country" VALUES (175, 'Sweden');
INSERT INTO "main"."Country" VALUES (176, 'Switzerland');
INSERT INTO "main"."Country" VALUES (177, 'Syria');
INSERT INTO "main"."Country" VALUES (178, 'Taiwan');
INSERT INTO "main"."Country" VALUES (179, 'Tajikistan');
INSERT INTO "main"."Country" VALUES (180, 'Tanzania');
INSERT INTO "main"."Country" VALUES (181, 'Thailand');
INSERT INTO "main"."Country" VALUES (182, 'Togo');
INSERT INTO "main"."Country" VALUES (183, 'Tonga');
INSERT INTO "main"."Country" VALUES (184, 'Trinidad and Tobago');
INSERT INTO "main"."Country" VALUES (185, 'Tunisia');
INSERT INTO "main"."Country" VALUES (186, 'Turkey');
INSERT INTO "main"."Country" VALUES (187, 'Turkmenistan');
INSERT INTO "main"."Country" VALUES (188, 'Uganda');
INSERT INTO "main"."Country" VALUES (189, 'Ukraine');
INSERT INTO "main"."Country" VALUES (190, 'United Arab Emirates');
INSERT INTO "main"."Country" VALUES (191, 'United Kingdom');
INSERT INTO "main"."Country" VALUES (192, 'United States');
INSERT INTO "main"."Country" VALUES (193, 'Uruguay');
INSERT INTO "main"."Country" VALUES (194, 'Uzbekistan');
INSERT INTO "main"."Country" VALUES (195, 'Vanuatu');
INSERT INTO "main"."Country" VALUES (196, 'Venezuela');
INSERT INTO "main"."Country" VALUES (197, 'Viet Nam');
INSERT INTO "main"."Country" VALUES (198, 'Yemen');
INSERT INTO "main"."Country" VALUES (199, 'Zambia');
INSERT INTO "main"."Country" VALUES (200, 'Zimbabwe');
INSERT INTO "main"."Country" VALUES (201, 'Unknown or unassigned country');
INSERT INTO "main"."Country" VALUES (202, 'Kosovo');
INSERT INTO "main"."Country" VALUES (203, 'Martinique');
INSERT INTO "main"."Country" VALUES (204, 'Guadeloupe');
INSERT INTO "main"."Country" VALUES (205, 'Saint Helena');
INSERT INTO "main"."Country" VALUES (206, 'Jersey');
INSERT INTO "main"."Country" VALUES (207, 'Puerto Rico');
INSERT INTO "main"."Country" VALUES (208, 'British Indian Ocean Territory');
INSERT INTO "main"."Country" VALUES (209, 'Curaçao');
INSERT INTO "main"."Country" VALUES (210, 'New Caledonia');
INSERT INTO "main"."Country" VALUES (211, 'Saint Kitts and Nevis');
INSERT INTO "main"."Country" VALUES (212, 'U.S. Virgin Islands');
INSERT INTO "main"."Country" VALUES (213, 'Turks and Caicos Islands');
INSERT INTO "main"."Country" VALUES (214, 'Sint Maarten');
INSERT INTO "main"."Country" VALUES (215, 'French Guiana');
INSERT INTO "main"."Country" VALUES (216, 'Réunion');
INSERT INTO "main"."Country" VALUES (217, 'French Polynesia');
INSERT INTO "main"."Country" VALUES (218, 'Western Sahara');
INSERT INTO "main"."Country" VALUES (219, 'Gibraltar');
INSERT INTO "main"."Country" VALUES (220, 'Guam');
INSERT INTO "main"."Country" VALUES (221, 'Caribbean Netherlands');
INSERT INTO "main"."Country" VALUES (222, 'Northern Mariana Islands');
INSERT INTO "main"."Country" VALUES (223, 'Mayotte');
INSERT INTO "main"."Country" VALUES (224, 'Christmas Island');
INSERT INTO "main"."Country" VALUES (225, 'Cocos (Keeling) Islands');
INSERT INTO "main"."Country" VALUES (226, 'Greenland');
INSERT INTO "main"."Country" VALUES (227, 'South Sudan');
INSERT INTO "main"."Country" VALUES (228, 'Saint Martin');
INSERT INTO "main"."Country" VALUES (229, 'Anguilla');
INSERT INTO "main"."Country" VALUES (230, 'British Virgin Islands');
INSERT INTO "main"."Country" VALUES (231, 'American Samoa');
INSERT INTO "main"."Country" VALUES (232, 'Dominica');
INSERT INTO "main"."Country" VALUES (233, 'Norfolk Island');
INSERT INTO "main"."Country" VALUES (234, 'Niue');
INSERT INTO "main"."Country" VALUES (235, 'Wallis and Futuna');
INSERT INTO "main"."Country" VALUES (236, 'Timor-Leste');
INSERT INTO "main"."Country" VALUES (237, 'Faroe Islands');
INSERT INTO "main"."Country" VALUES (238, 'Saint Pierre and Miquelon');

-- ----------------------------
-- Indexes structure for table Country
-- ----------------------------
CREATE UNIQUE INDEX "main"."IX_Country_Name"
ON "Country" ("Name" ASC);
