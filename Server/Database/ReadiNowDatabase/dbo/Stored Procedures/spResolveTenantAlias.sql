-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE PROCEDURE [dbo].[spResolveTenantAlias]
	@upgradeId UNIQUEIDENTIFIER,
	@tenantId BIGINT
AS
BEGIN
	SET NOCOUNT ON;

    SELECT TOP 1
		[Alias] = a.Namespace + ':' + a.Data
	FROM
		Entity e
	JOIN
		Data_Alias a ON
			e.TenantId = a.TenantId AND
			e.Id = a.EntityId
	WHERE
		e.TenantId = @tenantId AND
		e.UpgradeId = @upgradeId AND
		a.AliasMarkerId = 0 AND (
			a.Namespace = 'core' OR
			a.Namespace = 'console'
		)
END