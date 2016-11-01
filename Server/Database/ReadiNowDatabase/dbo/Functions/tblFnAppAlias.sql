-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE FUNCTION [dbo].[tblFnAppAlias]
(
	@entityUid UNIQUEIDENTIFIER,
	@appVerUid UNIQUEIDENTIFIER
)
RETURNS TABLE
AS
RETURN
(
	SELECT TOP 1
		d.[Namespace] + ':' + d.[Data] AS Alias
	FROM
		AppData_Alias d
	WHERE
		d.EntityUid = @entityUid AND
		d.FieldUid IN (
			SELECT
				EntityUid
			FROM
				AppData_Alias
			WHERE
				Data = 'alias' AND
				Namespace = 'core'AND
				AliasMarkerId = 0
		)
)