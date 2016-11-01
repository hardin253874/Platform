-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE PROC [dbo].[spGetMsTimeZoneName]
    @olsonTimeZoneName NVARCHAR( 256 )
AS              
SET NOCOUNT ON

SELECT
	tzm.Microsoft
FROM
	[dbo].[TimeZoneMap] tzm
INNER JOIN
	[dbo].[Zone] z ON
		tzm.Olson = z.Zone_name
WHERE
	[Olson] = @olsonTimeZoneName
				
SET NOCOUNT OFF