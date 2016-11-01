-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE FUNCTION [dbo].[tblFnConvertToLocalTime] (
       @utcDate DATETIME,
       @msTimeZoneId NVARCHAR( 256 ) -- e.g: 'AUS Eastern Standard Time', 'Tasmania Standard Time'
)
RETURNS TABLE
AS
RETURN
(
	SELECT TOP 1
		ut.Time
	FROM
		TimeZone tz 
	JOIN
		Zone z ON
			tz.Zone_id = z.Zone_id
	JOIN
		TimeZoneMap tzm ON
			z.Zone_name = tzm.Olson
	CROSS APPLY
		dbo.tblFnUnixTimeStamp( @utcDate ) ts
	CROSS APPLY
		dbo.tblFnFromUnixTime( ts.TimeStamp + tz.Gmt_offset ) ut
	WHERE
		tz.Time_start < ts.TimeStamp AND
		tzm.Microsoft = @msTimeZoneId
	ORDER BY
		tz.Time_start DESC
)