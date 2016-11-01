-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE FUNCTION [dbo].[fnAppAlias]
(
	-- Add the parameters for the function here
	@entityUid UNIQUEIDENTIFIER
)
RETURNS nvarchar(max)
AS
BEGIN
	DECLARE @alias nvarchar(max)

	SELECT
		@alias = d.Namespace + ':' + d.Data
	FROM
		AppData_Alias d
		WHERE
			d.EntityUid = @entityUid
		AND
			d.FieldUid IN (
			SELECT
				EntityUid
			FROM
				AppData_Alias
			WHERE
				Data = 'alias'
			AND
				Namespace = 'core'
			AND
				AliasMarkerId = 0
		)

	RETURN @alias

END



