-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE PROCEDURE [dbo].[spForwardRelationshipDeleteMany]
	@data dbo.ForwardRelationshipType READONLY
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

	DELETE
		Relationship
	OUTPUT
		DELETED.TenantId,
		DELETED.TypeId,
		DELETED.FromId,
		DELETED.ToId
	INTO
		@output
	FROM
		Relationship a
	JOIN
		@data d
	ON
		a.TenantId = d.TenantId AND
		a.TypeId = d.TypeId AND
		a.FromId = d.FromId

	SELECT
		TenantId,
		TypeId,
		FromId,
		ToId
	FROM
		@output
END