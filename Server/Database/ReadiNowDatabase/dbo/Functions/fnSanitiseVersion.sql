-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE FUNCTION [dbo].[fnSanitiseVersion] ( @version NVARCHAR(MAX) )
RETURNS NVARCHAR(MAX)
AS
BEGIN

DECLARE @xml XML
DECLARE @delimiter NVARCHAR(1) = '.'
DECLARE @components TABLE ( Component NVARCHAR( 50 ) )

SET @xml = CAST( ( '<X>' + REPLACE( @version, @delimiter, '</X><X>' ) + '</X>' ) AS XML )

INSERT INTO @components
SELECT dbo.fnConvertToInt(SUBSTRING( C.value( '.', 'NVARCHAR( 10 )'), PATINDEX( '%[^0]%', C.value( '.', 'NVARCHAR( 10 )' ) ), 10 )) AS value
FROM @xml.nodes( 'X' ) AS X( C )

RETURN STUFF
	(
		(
			SELECT @delimiter + a.Component AS [text()]
			FROM @components a
			FOR XML PATH( '' )
		), 1, 1, ''
	)
 
END