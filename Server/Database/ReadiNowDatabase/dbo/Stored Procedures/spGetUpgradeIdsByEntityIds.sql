-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE PROCEDURE [dbo].[spGetUpgradeIdsByEntityIds]
	@tenantId BIGINT,
	@data dbo.UniqueIdListType READONLY
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		a.Id,
		a.UpgradeId
	FROM
		Entity a
	INNER JOIN
		@data d
	ON
		a.Id = d.Id 
	WHERE
	a.TenantId = @tenantId
END