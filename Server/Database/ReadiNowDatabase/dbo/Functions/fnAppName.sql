-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE FUNCTION [dbo].[fnAppName]
(
	-- Add the parameters for the function here
	@entityUid UNIQUEIDENTIFIER,
	@appVerUid UNIQUEIDENTIFIER
)
RETURNS nvarchar(max)
AS
BEGIN
	DECLARE @name nvarchar(max)
	
	SELECT
		@name = d.Data
	FROM
		AppData_NVarChar d
		WHERE
			d.AppVerUid = @appVerUid
		AND
			d.EntityUid = @entityUid
		AND
			d.FieldUid IN (
			SELECT
				EntityUid
			FROM
				AppData_Alias
			WHERE
				Data = 'name'
			AND
				Namespace = 'core'
			AND
				AliasMarkerId = 0
		)

	RETURN @name

END


