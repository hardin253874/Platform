-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE FUNCTION [dbo].[fnConvertToUtc] (
       @localDate DATETIME,
       @msTimeZoneId NVARCHAR(256) -- e.g: 'AUS Eastern Standard Time', 'Tasmania Standard Time'
)
RETURNS DATETIME
AS
BEGIN
       /* Function body */
       DECLARE @return DATETIME
   
       SELECT TOP 1 @return = dbo.fnFromUnixTime(dbo.fnUnixTimeStamp(@localDate) - tz.Gmt_offset )
       FROM TimeZone tz 
		JOIN Zone z ON tz.Zone_id = z.Zone_id
		JOIN TimeZoneMap tzm ON z.Zone_name = tzm.Olson
       WHERE tz.Time_start < dbo.fnUnixTimeStamp(@localDate) 
		AND tzm.Microsoft = @msTimeZoneId
       ORDER BY tz.Time_start DESC
   
       RETURN @return
END