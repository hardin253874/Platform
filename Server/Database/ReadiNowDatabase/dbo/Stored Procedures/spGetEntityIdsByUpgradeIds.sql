-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE PROCEDURE [dbo].[spGetEntityIdsByUpgradeIds]
	@tenantId BIGINT,
	@data dbo.UniqueGuidListType READONLY
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		a.UpgradeId,
		a.Id		
	FROM
		Entity a
	INNER JOIN
		@data d
	ON
		a.UpgradeId = d.Id 
	WHERE
	a.TenantId = @tenantId
END