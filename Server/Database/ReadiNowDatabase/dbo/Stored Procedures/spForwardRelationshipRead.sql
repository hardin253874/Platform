-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE PROCEDURE [dbo].[spForwardRelationshipRead]
	@tenantId BIGINT,
	@typeId BIGINT,
	@fromId BIGINT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		a.TenantId,
		a.TypeId,
		a.FromId,
		a.ToId
	FROM
		Relationship a
	WHERE
		TenantId = @tenantId AND
		TypeId = @typeId AND
		FromId = @fromId
END