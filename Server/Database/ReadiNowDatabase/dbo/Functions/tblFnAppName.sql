-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE FUNCTION [dbo].[tblFnAppName]
(
	@entityUid UNIQUEIDENTIFIER,
	@appVerUid UNIQUEIDENTIFIER
)
RETURNS TABLE
AS
RETURN
(
	SELECT TOP 1
		d.Data AS Name
	FROM
		AppData_NVarChar d
	WHERE
		d.AppVerUid = @appVerUid AND
		d.EntityUid = @entityUid AND
		d.FieldUid IN (
			SELECT
				EntityUid
			FROM
				AppData_Alias
			WHERE
				Data = 'name' AND
				Namespace = 'core' AND
				AliasMarkerId = 0
		)
)