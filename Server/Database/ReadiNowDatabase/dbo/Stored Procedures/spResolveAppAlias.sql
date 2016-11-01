-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE PROCEDURE [dbo].[spResolveAppAlias]
	@upgradeId UNIQUEIDENTIFIER
AS
BEGIN
	SET NOCOUNT ON;

    SELECT TOP 1
		[Alias] = a.Namespace + ':' + a.Data
	FROM
		AppData_Alias a
	WHERE
		a.EntityUid = @upgradeId AND
		a.AliasMarkerId = 0 AND (
			a.Namespace = 'core' OR
			a.Namespace = 'console'
		)
END