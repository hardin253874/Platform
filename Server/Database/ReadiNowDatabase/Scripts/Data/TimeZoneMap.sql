-- Copyright 2011-2016 Global Software Innovation Pty Ltd

/*
Post-Deployment Script Template							
--------------------------------------------------------------------------------------
 This file contains SQL statements that will be appended to the build script.		
 Use SQLCMD syntax to include a file in the post-deployment script.			
 Example:      :r .\myfile.sql								
 Use SQLCMD syntax to reference a variable in the post-deployment script.		
 Example:      :setvar TableName MyTable							
               SELECT * FROM [$(TableName)]					
--------------------------------------------------------------------------------------
*/

TRUNCATE TABLE [dbo].[TimeZoneMap]

GO

INSERT INTO [dbo].[TimeZoneMap] (
	[Olson],
	[Microsoft]) 
VALUES
	(N'Asia/Kabul', N'Afghanistan Standard Time'),
	(N'America/Anchorage', N'Alaskan Standard Time'),
	(N'America/Juneau', N'Alaskan Standard Time'),
	(N'America/Nome', N'Alaskan Standard Time'),
	(N'America/Sitka', N'Alaskan Standard Time'),
	(N'America/Yakutat', N'Alaskan Standard Time'),
	(N'Asia/Riyadh', N'Arab Standard Time'),
	(N'Asia/Bahrain', N'Arab Standard Time'),
	(N'Asia/Kuwait', N'Arab Standard Time'),
	(N'Asia/Qatar', N'Arab Standard Time'),
	(N'Asia/Aden', N'Arab Standard Time'),
	(N'Asia/Dubai', N'Arabian Standard Time'),
	(N'Asia/Muscat', N'Arabian Standard Time'),
	(N'Etc/GMT-4', N'Arabian Standard Time'),
	(N'Asia/Baghdad', N'Arabic Standard Time'),
	(N'America/Buenos_Aires', N'Argentina Standard Time'),
	(N'America/Argentina/La_Rioja', N'Argentina Standard Time'),
	(N'America/Argentina/Rio_Gallegos', N'Argentina Standard Time'),
	(N'America/Argentina/Salta', N'Argentina Standard Time'),
	(N'America/Argentina/San_Juan', N'Argentina Standard Time'),
	(N'America/Argentina/San_Luis', N'Argentina Standard Time'),
	(N'America/Argentina/Tucuman', N'Argentina Standard Time'),
	(N'America/Argentina/Ushuaia', N'Argentina Standard Time'),
	(N'America/Catamarca', N'Argentina Standard Time'),
	(N'America/Cordoba', N'Argentina Standard Time'),
	(N'America/Jujuy', N'Argentina Standard Time'),
	(N'America/Mendoza', N'Argentina Standard Time'),
	(N'America/Halifax', N'Atlantic Standard Time'),
	(N'Atlantic/Bermuda', N'Atlantic Standard Time'),
	(N'America/Glace_Bay', N'Atlantic Standard Time'),
	(N'America/Goose_Bay', N'Atlantic Standard Time'),
	(N'America/Moncton', N'Atlantic Standard Time'),
	(N'America/Thule', N'Atlantic Standard Time'),
	(N'Australia/Darwin', N'AUS Central Standard Time'),
	(N'Australia/Sydney', N'AUS Eastern Standard Time'),
	(N'Australia/Melbourne', N'AUS Eastern Standard Time'),
	(N'Asia/Baku', N'Azerbaijan Standard Time'),
	(N'Atlantic/Azores', N'Azores Standard Time'),
	(N'America/Scoresbysund', N'Azores Standard Time'),
	(N'America/Bahia', N'Bahia Standard Time'),
	(N'Asia/Dhaka', N'Bangladesh Standard Time'),
	(N'Asia/Thimphu', N'Bangladesh Standard Time'),
	(N'America/Regina', N'Canada Central Standard Time'),
	(N'America/Swift_Current', N'Canada Central Standard Time'),
	(N'Atlantic/Cape_Verde', N'Cape Verde Standard Time'),
	(N'Etc/GMT+1', N'Cape Verde Standard Time'),
	(N'Asia/Yerevan', N'Caucasus Standard Time'),
	(N'Australia/Adelaide', N'Cen. Australia Standard Time'),
	(N'Australia/Broken_Hill', N'Cen. Australia Standard Time'),
	(N'America/Guatemala', N'Central America Standard Time'),
	(N'America/Belize', N'Central America Standard Time'),
	(N'America/Costa_Rica', N'Central America Standard Time'),
	(N'Pacific/Galapagos', N'Central America Standard Time'),
	(N'America/Tegucigalpa', N'Central America Standard Time'),
	(N'America/Managua', N'Central America Standard Time'),
	(N'America/El_Salvador', N'Central America Standard Time'),
	(N'Etc/GMT+6', N'Central America Standard Time'),
	(N'Asia/Almaty', N'Central Asia Standard Time'),
	(N'Antarctica/Vostok', N'Central Asia Standard Time'),
	(N'Indian/Chagos', N'Central Asia Standard Time'),
	(N'Asia/Bishkek', N'Central Asia Standard Time'),
	(N'Asia/Qyzylorda', N'Central Asia Standard Time'),
	(N'Etc/GMT-6', N'Central Asia Standard Time'),
	(N'America/Cuiaba', N'Central Brazilian Standard Time'),
	(N'America/Campo_Grande', N'Central Brazilian Standard Time'),
	(N'Europe/Budapest', N'Central Europe Standard Time'),
	(N'Europe/Tirane', N'Central Europe Standard Time'),
	(N'Europe/Prague', N'Central Europe Standard Time'),
	(N'Europe/Podgorica', N'Central Europe Standard Time'),
	(N'Europe/Belgrade', N'Central Europe Standard Time'),
	(N'Europe/Ljubljana', N'Central Europe Standard Time'),
	(N'Europe/Bratislava', N'Central Europe Standard Time'),
	(N'Europe/Warsaw', N'Central European Standard Time'),
	(N'Europe/Sarajevo', N'Central European Standard Time'),
	(N'Europe/Zagreb', N'Central European Standard Time'),
	(N'Europe/Skopje', N'Central European Standard Time'),
	(N'Pacific/Guadalcanal', N'Central Pacific Standard Time'),
	(N'Antarctica/Macquarie', N'Central Pacific Standard Time'),
	(N'Pacific/Ponape', N'Central Pacific Standard Time'),
	(N'Pacific/Kosrae', N'Central Pacific Standard Time'),
	(N'Pacific/Noumea', N'Central Pacific Standard Time'),
	(N'Pacific/Efate', N'Central Pacific Standard Time'),
	(N'Etc/GMT-11', N'Central Pacific Standard Time'),
	(N'America/Chicago', N'Central Standard Time'),
	(N'America/Winnipeg', N'Central Standard Time'),
	(N'America/Rainy_River', N'Central Standard Time'),
	(N'America/Rankin_Inlet', N'Central Standard Time'),
	(N'America/Resolute', N'Central Standard Time'),
	(N'America/Matamoros', N'Central Standard Time'),
	(N'America/Indiana/Knox', N'Central Standard Time'),
	(N'America/Indiana/Tell_City', N'Central Standard Time'),
	(N'America/Menominee', N'Central Standard Time'),
	(N'America/North_Dakota/Beulah', N'Central Standard Time'),
	(N'America/North_Dakota/Center', N'Central Standard Time'),
	(N'America/North_Dakota/New_Salem', N'Central Standard Time'),
	(N'CST6CDT', N'Central Standard Time'),
	(N'America/Mexico_City', N'Central Standard Time (Mexico)'),
	(N'America/Bahia_Banderas', N'Central Standard Time (Mexico)'),
	(N'America/Cancun', N'Central Standard Time (Mexico)'),
	(N'America/Merida', N'Central Standard Time (Mexico)'),
	(N'America/Monterrey', N'Central Standard Time (Mexico)'),
	(N'Asia/Shanghai', N'China Standard Time'),
	(N'Asia/Chongqing', N'China Standard Time'),
	(N'Asia/Harbin', N'China Standard Time'),
	(N'Asia/Kashgar', N'China Standard Time'),
	(N'Asia/Urumqi', N'China Standard Time'),
	(N'Asia/Hong_Kong', N'China Standard Time'),
	(N'Asia/Macau', N'China Standard Time'),
	(N'Etc/GMT+12', N'Dateline Standard Time'),
	(N'Africa/Nairobi', N'E. Africa Standard Time'),
	(N'Antarctica/Syowa', N'E. Africa Standard Time'),
	(N'Africa/Djibouti', N'E. Africa Standard Time'),
	(N'Africa/Asmera', N'E. Africa Standard Time'),
	(N'Africa/Addis_Ababa', N'E. Africa Standard Time'),
	(N'Indian/Comoro', N'E. Africa Standard Time'),
	(N'Indian/Antananarivo', N'E. Africa Standard Time'),
	(N'Africa/Khartoum', N'E. Africa Standard Time'),
	(N'Africa/Mogadishu', N'E. Africa Standard Time'),
	(N'Africa/Juba', N'E. Africa Standard Time'),
	(N'Africa/Dar_es_Salaam', N'E. Africa Standard Time'),
	(N'Africa/Kampala', N'E. Africa Standard Time'),
	(N'Indian/Mayotte', N'E. Africa Standard Time'),
	(N'Etc/GMT-3', N'E. Africa Standard Time'),
	(N'Australia/Brisbane', N'E. Australia Standard Time'),
	(N'Australia/Lindeman', N'E. Australia Standard Time'),
	(N'Asia/Nicosia', N'E. Europe Standard Time'),
	(N'America/Sao_Paulo', N'E. South America Standard Time'),
	(N'America/Araguaina', N'E. South America Standard Time'),
	(N'America/New_York', N'Eastern Standard Time'),
	(N'America/Nassau', N'Eastern Standard Time'),
	(N'America/Toronto', N'Eastern Standard Time'),
	(N'America/Iqaluit', N'Eastern Standard Time'),
	(N'America/Montreal', N'Eastern Standard Time'),
	(N'America/Nipigon', N'Eastern Standard Time'),
	(N'America/Pangnirtung', N'Eastern Standard Time'),
	(N'America/Thunder_Bay', N'Eastern Standard Time'),
	(N'America/Grand_Turk', N'Eastern Standard Time'),
	(N'America/Detroit', N'Eastern Standard Time'),
	(N'America/Indiana/Petersburg', N'Eastern Standard Time'),
	(N'America/Indiana/Vincennes', N'Eastern Standard Time'),
	(N'America/Indiana/Winamac', N'Eastern Standard Time'),
	(N'America/Kentucky/Monticello', N'Eastern Standard Time'),
	(N'America/Louisville', N'Eastern Standard Time'),
	(N'EST5EDT', N'Eastern Standard Time'),
	(N'Africa/Cairo', N'Egypt Standard Time'),
	(N'Asia/Gaza', N'Egypt Standard Time'),
	(N'Asia/Hebron', N'Egypt Standard Time'),
	(N'Asia/Yekaterinburg', N'Ekaterinburg Standard Time'),
	(N'Pacific/Fiji', N'Fiji Standard Time'),
	(N'Europe/Kiev', N'FLE Standard Time'),
	(N'Europe/Mariehamn', N'FLE Standard Time'),
	(N'Europe/Sofia', N'FLE Standard Time'),
	(N'Europe/Tallinn', N'FLE Standard Time'),
	(N'Europe/Helsinki', N'FLE Standard Time'),
	(N'Europe/Vilnius', N'FLE Standard Time'),
	(N'Europe/Riga', N'FLE Standard Time'),
	(N'Europe/Simferopol', N'FLE Standard Time'),
	(N'Europe/Uzhgorod', N'FLE Standard Time'),
	(N'Europe/Zaporozhye', N'FLE Standard Time'),
	(N'Asia/Tbilisi', N'Georgian Standard Time'),
	(N'Europe/London', N'GMT Standard Time'),
	(N'Atlantic/Canary', N'GMT Standard Time'),
	(N'Atlantic/Faeroe', N'GMT Standard Time'),
	(N'Europe/Guernsey', N'GMT Standard Time'),
	(N'Europe/Dublin', N'GMT Standard Time'),
	(N'Europe/Isle_of_Man', N'GMT Standard Time'),
	(N'Europe/Jersey', N'GMT Standard Time'),
	(N'Europe/Lisbon', N'GMT Standard Time'),
	(N'Atlantic/Madeira', N'GMT Standard Time'),
	(N'America/Godthab', N'Greenland Standard Time'),
	(N'Atlantic/Reykjavik', N'Greenwich Standard Time'),
	(N'Africa/Ouagadougou', N'Greenwich Standard Time'),
	(N'Africa/Abidjan', N'Greenwich Standard Time'),
	(N'Africa/El_Aaiun', N'Greenwich Standard Time'),
	(N'Africa/Accra', N'Greenwich Standard Time'),
	(N'Africa/Banjul', N'Greenwich Standard Time'),
	(N'Africa/Conakry', N'Greenwich Standard Time'),
	(N'Africa/Bissau', N'Greenwich Standard Time'),
	(N'Africa/Monrovia', N'Greenwich Standard Time'),
	(N'Africa/Bamako', N'Greenwich Standard Time'),
	(N'Africa/Nouakchott', N'Greenwich Standard Time'),
	(N'Atlantic/St_Helena', N'Greenwich Standard Time'),
	(N'Africa/Freetown', N'Greenwich Standard Time'),
	(N'Africa/Dakar', N'Greenwich Standard Time'),
	(N'Africa/Sao_Tome', N'Greenwich Standard Time'),
	(N'Africa/Lome', N'Greenwich Standard Time'),
	(N'Europe/Bucharest', N'GTB Standard Time'),
	(N'Europe/Athens', N'GTB Standard Time'),
	(N'Europe/Chisinau', N'GTB Standard Time'),
	(N'Pacific/Honolulu', N'Hawaiian Standard Time'),
	(N'Pacific/Rarotonga', N'Hawaiian Standard Time'),
	(N'Pacific/Tahiti', N'Hawaiian Standard Time'),
	(N'Pacific/Johnston', N'Hawaiian Standard Time'),
	(N'Etc/GMT+10', N'Hawaiian Standard Time'),
	(N'Asia/Calcutta', N'India Standard Time'),
	(N'Asia/Tehran', N'Iran Standard Time'),
	(N'Asia/Jerusalem', N'Israel Standard Time'),
	(N'Asia/Amman', N'Jordan Standard Time'),
	(N'Europe/Kaliningrad', N'Kaliningrad Standard Time'),
	(N'Europe/Minsk', N'Kaliningrad Standard Time'),
	(N'Asia/Seoul', N'Korea Standard Time'),
	(N'Asia/Pyongyang', N'Korea Standard Time'),
	(N'Asia/Magadan', N'Magadan Standard Time'),
	(N'Asia/Anadyr', N'Magadan Standard Time'),
	(N'Asia/Kamchatka', N'Magadan Standard Time'),
	(N'Indian/Mauritius', N'Mauritius Standard Time'),
	(N'Indian/Reunion', N'Mauritius Standard Time'),
	(N'Indian/Mahe', N'Mauritius Standard Time'),
	(N'Asia/Beirut', N'Middle East Standard Time'),
	(N'America/Montevideo', N'Montevideo Standard Time'),
	(N'Africa/Casablanca', N'Morocco Standard Time'),
	(N'America/Denver', N'Mountain Standard Time'),
	(N'America/Edmonton', N'Mountain Standard Time'),
	(N'America/Cambridge_Bay', N'Mountain Standard Time'),
	(N'America/Inuvik', N'Mountain Standard Time'),
	(N'America/Yellowknife', N'Mountain Standard Time'),
	(N'America/Ojinaga', N'Mountain Standard Time'),
	(N'America/Boise', N'Mountain Standard Time'),
	(N'America/Shiprock', N'Mountain Standard Time'),
	(N'MST7MDT', N'Mountain Standard Time'),
	(N'America/Chihuahua', N'Mountain Standard Time (Mexico)'),
	(N'America/Mazatlan', N'Mountain Standard Time (Mexico)'),
	(N'Asia/Rangoon', N'Myanmar Standard Time'),
	(N'Indian/Cocos', N'Myanmar Standard Time'),
	(N'Asia/Novosibirsk', N'N. Central Asia Standard Time'),
	(N'Asia/Novokuznetsk', N'N. Central Asia Standard Time'),
	(N'Asia/Omsk', N'N. Central Asia Standard Time'),
	(N'Africa/Windhoek', N'Namibia Standard Time'),
	(N'Asia/Katmandu', N'Nepal Standard Time'),
	(N'Pacific/Auckland', N'New Zealand Standard Time'),
	(N'Antarctica/South_Pole', N'New Zealand Standard Time'),
	(N'Antarctica/McMurdo', N'New Zealand Standard Time'),
	(N'America/St_Johns', N'Newfoundland Standard Time'),
	(N'Asia/Irkutsk', N'North Asia East Standard Time'),
	(N'Asia/Krasnoyarsk', N'North Asia Standard Time'),
	(N'America/Santiago', N'Pacific SA Standard Time'),
	(N'Antarctica/Palmer', N'Pacific SA Standard Time'),
	(N'America/Los_Angeles', N'Pacific Standard Time'),
	(N'America/Vancouver', N'Pacific Standard Time'),
	(N'America/Dawson', N'Pacific Standard Time'),
	(N'America/Whitehorse', N'Pacific Standard Time'),
	(N'America/Tijuana', N'Pacific Standard Time'),
	(N'PST8PDT', N'Pacific Standard Time'),
	(N'America/Santa_Isabel', N'Pacific Standard Time (Mexico)'),
	(N'Asia/Karachi', N'Pakistan Standard Time'),
	(N'America/Asuncion', N'Paraguay Standard Time'),
	(N'Europe/Paris', N'Romance Standard Time'),
	(N'Europe/Brussels', N'Romance Standard Time'),
	(N'Europe/Copenhagen', N'Romance Standard Time'),
	(N'Europe/Madrid', N'Romance Standard Time'),
	(N'Africa/Ceuta', N'Romance Standard Time'),
	(N'Europe/Moscow', N'Russian Standard Time'),
	(N'Europe/Samara', N'Russian Standard Time'),
	(N'Europe/Volgograd', N'Russian Standard Time'),
	(N'America/Cayenne', N'SA Eastern Standard Time'),
	(N'Antarctica/Rothera', N'SA Eastern Standard Time'),
	(N'America/Fortaleza', N'SA Eastern Standard Time'),
	(N'America/Belem', N'SA Eastern Standard Time'),
	(N'America/Maceio', N'SA Eastern Standard Time'),
	(N'America/Recife', N'SA Eastern Standard Time'),
	(N'America/Santarem', N'SA Eastern Standard Time'),
	(N'Atlantic/Stanley', N'SA Eastern Standard Time'),
	(N'America/Paramaribo', N'SA Eastern Standard Time'),
	(N'Etc/GMT+3', N'SA Eastern Standard Time'),
	(N'America/Bogota', N'SA Pacific Standard Time'),
	(N'America/Coral_Harbour', N'SA Pacific Standard Time'),
	(N'America/Guayaquil', N'SA Pacific Standard Time'),
	(N'America/Port-au-Prince', N'SA Pacific Standard Time'),
	(N'America/Jamaica', N'SA Pacific Standard Time'),
	(N'America/Cayman', N'SA Pacific Standard Time'),
	(N'America/Panama', N'SA Pacific Standard Time'),
	(N'America/Lima', N'SA Pacific Standard Time'),
	(N'Etc/GMT+5', N'SA Pacific Standard Time'),
	(N'America/La_Paz', N'SA Western Standard Time'),
	(N'America/Antigua', N'SA Western Standard Time'),
	(N'America/Anguilla', N'SA Western Standard Time'),
	(N'America/Aruba', N'SA Western Standard Time'),
	(N'America/Barbados', N'SA Western Standard Time'),
	(N'America/St_Barthelemy', N'SA Western Standard Time'),
	(N'America/Kralendijk', N'SA Western Standard Time'),
	(N'America/Manaus', N'SA Western Standard Time'),
	(N'America/Boa_Vista', N'SA Western Standard Time'),
	(N'America/Eirunepe', N'SA Western Standard Time'),
	(N'America/Porto_Velho', N'SA Western Standard Time'),
	(N'America/Rio_Branco', N'SA Western Standard Time'),
	(N'America/Blanc-Sablon', N'SA Western Standard Time'),
	(N'America/Curacao', N'SA Western Standard Time'),
	(N'America/Dominica', N'SA Western Standard Time'),
	(N'America/Santo_Domingo', N'SA Western Standard Time'),
	(N'America/Grenada', N'SA Western Standard Time'),
	(N'America/Guadeloupe', N'SA Western Standard Time'),
	(N'America/Guyana', N'SA Western Standard Time'),
	(N'America/St_Kitts', N'SA Western Standard Time'),
	(N'America/St_Lucia', N'SA Western Standard Time'),
	(N'America/Marigot', N'SA Western Standard Time'),
	(N'America/Martinique', N'SA Western Standard Time'),
	(N'America/Montserrat', N'SA Western Standard Time'),
	(N'America/Puerto_Rico', N'SA Western Standard Time'),
	(N'America/Lower_Princes', N'SA Western Standard Time'),
	(N'America/Port_of_Spain', N'SA Western Standard Time'),
	(N'America/St_Vincent', N'SA Western Standard Time'),
	(N'America/Tortola', N'SA Western Standard Time'),
	(N'America/St_Thomas', N'SA Western Standard Time'),
	(N'Etc/GMT+4', N'SA Western Standard Time'),
	(N'Pacific/Apia', N'Samoa Standard Time'),
	(N'Asia/Bangkok', N'SE Asia Standard Time'),
	(N'Antarctica/Davis', N'SE Asia Standard Time'),
	(N'Indian/Christmas', N'SE Asia Standard Time'),
	(N'Asia/Jakarta', N'SE Asia Standard Time'),
	(N'Asia/Pontianak', N'SE Asia Standard Time'),
	(N'Asia/Phnom_Penh', N'SE Asia Standard Time'),
	(N'Asia/Vientiane', N'SE Asia Standard Time'),
	(N'Asia/Hovd', N'SE Asia Standard Time'),
	(N'Asia/Saigon', N'SE Asia Standard Time'),
	(N'Etc/GMT-7', N'SE Asia Standard Time'),
	(N'Asia/Singapore', N'Singapore Standard Time'),
	(N'Asia/Brunei', N'Singapore Standard Time'),
	(N'Asia/Makassar', N'Singapore Standard Time'),
	(N'Asia/Kuala_Lumpur', N'Singapore Standard Time'),
	(N'Asia/Kuching', N'Singapore Standard Time'),
	(N'Asia/Manila', N'Singapore Standard Time'),
	(N'Etc/GMT-8', N'Singapore Standard Time'),
	(N'Africa/Johannesburg', N'South Africa Standard Time'),
	(N'Africa/Bujumbura', N'South Africa Standard Time'),
	(N'Africa/Gaborone', N'South Africa Standard Time'),
	(N'Africa/Lubumbashi', N'South Africa Standard Time'),
	(N'Africa/Maseru', N'South Africa Standard Time'),
	(N'Africa/Blantyre', N'South Africa Standard Time'),
	(N'Africa/Maputo', N'South Africa Standard Time'),
	(N'Africa/Kigali', N'South Africa Standard Time'),
	(N'Africa/Mbabane', N'South Africa Standard Time'),
	(N'Africa/Lusaka', N'South Africa Standard Time'),
	(N'Africa/Harare', N'South Africa Standard Time'),
	(N'Etc/GMT-2', N'South Africa Standard Time'),
	(N'Asia/Colombo', N'Sri Lanka Standard Time'),
	(N'Asia/Damascus', N'Syria Standard Time'),
	(N'Asia/Taipei', N'Taipei Standard Time'),
	(N'Australia/Hobart', N'Tasmania Standard Time'),
	(N'Australia/Currie', N'Tasmania Standard Time'),
	(N'Asia/Tokyo', N'Tokyo Standard Time'),
	(N'Asia/Jayapura', N'Tokyo Standard Time'),
	(N'Pacific/Palau', N'Tokyo Standard Time'),
	(N'Asia/Dili', N'Tokyo Standard Time'),
	(N'Etc/GMT-9', N'Tokyo Standard Time'),
	(N'Pacific/Tongatapu', N'Tonga Standard Time'),
	(N'Pacific/Enderbury', N'Tonga Standard Time'),
	(N'Pacific/Fakaofo', N'Tonga Standard Time'),
	(N'Etc/GMT-13', N'Tonga Standard Time'),
	(N'Europe/Istanbul', N'Turkey Standard Time'),
	(N'Asia/Ulaanbaatar', N'Ulaanbaatar Standard Time'),
	(N'Asia/Choibalsan', N'Ulaanbaatar Standard Time'),
	(N'America/Indianapolis', N'US Eastern Standard Time'),
	(N'America/Indiana/Marengo', N'US Eastern Standard Time'),
	(N'America/Indiana/Vevay', N'US Eastern Standard Time'),
	(N'America/Phoenix', N'US Mountain Standard Time'),
	(N'America/Dawson_Creek', N'US Mountain Standard Time'),
	(N'America/Creston', N'US Mountain Standard Time'),
	(N'America/Hermosillo', N'US Mountain Standard Time'),
	(N'Etc/GMT+7', N'US Mountain Standard Time'),
	(N'Etc/GMT', N'UTC'),
	(N'America/Danmarkshavn', N'UTC'),
	(N'Etc/GMT-12', N'UTC+12'),
	(N'Pacific/Tarawa', N'UTC+12'),
	(N'Pacific/Majuro', N'UTC+12'),
	(N'Pacific/Kwajalein', N'UTC+12'),
	(N'Pacific/Nauru', N'UTC+12'),
	(N'Pacific/Funafuti', N'UTC+12'),
	(N'Pacific/Wake', N'UTC+12'),
	(N'Pacific/Wallis', N'UTC+12'),
	(N'Etc/GMT+2', N'UTC-02'),
	(N'America/Noronha', N'UTC-02'),
	(N'Atlantic/South_Georgia', N'UTC-02'),
	(N'Etc/GMT+11', N'UTC-11'),
	(N'Pacific/Pago_Pago', N'UTC-11'),
	(N'Pacific/Niue', N'UTC-11'),
	(N'Pacific/Midway', N'UTC-11'),
	(N'America/Caracas', N'Venezuela Standard Time'),
	(N'Asia/Vladivostok', N'Vladivostok Standard Time'),
	(N'Asia/Sakhalin', N'Vladivostok Standard Time'),
	(N'Asia/Ust-Nera', N'Vladivostok Standard Time'),
	(N'Australia/Perth', N'W. Australia Standard Time'),
	(N'Antarctica/Casey', N'W. Australia Standard Time'),
	(N'Africa/Lagos', N'W. Central Africa Standard Time'),
	(N'Africa/Luanda', N'W. Central Africa Standard Time'),
	(N'Africa/Porto-Novo', N'W. Central Africa Standard Time'),
	(N'Africa/Kinshasa', N'W. Central Africa Standard Time'),
	(N'Africa/Bangui', N'W. Central Africa Standard Time'),
	(N'Africa/Brazzaville', N'W. Central Africa Standard Time'),
	(N'Africa/Douala', N'W. Central Africa Standard Time'),
	(N'Africa/Algiers', N'W. Central Africa Standard Time'),
	(N'Africa/Libreville', N'W. Central Africa Standard Time'),
	(N'Africa/Malabo', N'W. Central Africa Standard Time'),
	(N'Africa/Niamey', N'W. Central Africa Standard Time'),
	(N'Africa/Ndjamena', N'W. Central Africa Standard Time'),
	(N'Africa/Tunis', N'W. Central Africa Standard Time'),
	(N'Etc/GMT-1', N'W. Central Africa Standard Time'),
	(N'Europe/Berlin', N'W. Europe Standard Time'),
	(N'Europe/Andorra', N'W. Europe Standard Time'),
	(N'Europe/Vienna', N'W. Europe Standard Time'),
	(N'Europe/Zurich', N'W. Europe Standard Time'),
	(N'Europe/Busingen', N'W. Europe Standard Time'),
	(N'Europe/Gibraltar', N'W. Europe Standard Time'),
	(N'Europe/Rome', N'W. Europe Standard Time'),
	(N'Europe/Vaduz', N'W. Europe Standard Time'),
	(N'Europe/Luxembourg', N'W. Europe Standard Time'),
	(N'Africa/Tripoli', N'W. Europe Standard Time'),
	(N'Europe/Monaco', N'W. Europe Standard Time'),
	(N'Europe/Malta', N'W. Europe Standard Time'),
	(N'Europe/Amsterdam', N'W. Europe Standard Time'),
	(N'Europe/Oslo', N'W. Europe Standard Time'),
	(N'Europe/Stockholm', N'W. Europe Standard Time'),
	(N'Arctic/Longyearbyen', N'W. Europe Standard Time'),
	(N'Europe/San_Marino', N'W. Europe Standard Time'),
	(N'Europe/Vatican', N'W. Europe Standard Time'),
	(N'Asia/Tashkent', N'West Asia Standard Time'),
	(N'Antarctica/Mawson', N'West Asia Standard Time'),
	(N'Asia/Oral', N'West Asia Standard Time'),
	(N'Asia/Aqtau', N'West Asia Standard Time'),
	(N'Asia/Aqtobe', N'West Asia Standard Time'),
	(N'Indian/Maldives', N'West Asia Standard Time'),
	(N'Indian/Kerguelen', N'West Asia Standard Time'),
	(N'Asia/Dushanbe', N'West Asia Standard Time'),
	(N'Asia/Ashgabat', N'West Asia Standard Time'),
	(N'Asia/Samarkand', N'West Asia Standard Time'),
	(N'Etc/GMT-5', N'West Asia Standard Time'),
	(N'Pacific/Port_Moresby', N'West Pacific Standard Time'),
	(N'Antarctica/DumontDUrville', N'West Pacific Standard Time'),
	(N'Pacific/Truk', N'West Pacific Standard Time'),
	(N'Pacific/Guam', N'West Pacific Standard Time'),
	(N'Pacific/Saipan', N'West Pacific Standard Time'),
	(N'Etc/GMT-10', N'West Pacific Standard Time'),
	(N'Asia/Yakutsk', N'Yakutsk Standard Time'),
	(N'Asia/Khandyga', N'Yakutsk Standard Time')