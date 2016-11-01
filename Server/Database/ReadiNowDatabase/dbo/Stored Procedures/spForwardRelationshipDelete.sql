-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE PROCEDURE [dbo].[spForwardRelationshipDelete]
	@tenantId BIGINT,
	@typeId BIGINT,
	@fromId BIGINT
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @output TABLE
	(
		TenantId BIGINT,
		TypeId BIGINT,
		FromId BIGINT,
		ToId BIGINT
	)

	DELETE FROM
		Relationship
	OUTPUT
		DELETED.TenantId,
		DELETED.TypeId,
		DELETED.FromId,
		DELETED.ToId
	INTO
		@output
	WHERE
		TenantId = @tenantId AND
		TypeId = @typeId AND
		FromId = @fromId

	SELECT
		TenantId,
		TypeId,
		FromId,
		ToId
	FROM
		@output
END