-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE FUNCTION [dbo].[fnGetLocalDate] (
	@msTimeZoneId NVARCHAR(256)	-- e.g: 'AUS Eastern Standard Time', 'Tasmania Standard Time'
)
RETURNS DATETIME
AS
BEGIN
	/* Function body */
	DECLARE @return DATETIME
   
	SELECT TOP 1 @return = dbo.fnFromUnixTime(dbo.fnUnixTimeStamp(GETUTCDATE()) + tz.Gmt_offset )
	FROM TimeZone tz 
	JOIN Zone z ON tz.Zone_id = z.Zone_id
	WHERE tz.Time_start < dbo.fnUnixTimeStamp(GETUTCDATE()) 
	AND z.Zone_name = @msTimeZoneId
	ORDER BY tz.Time_start DESC
   
	RETURN @return
END