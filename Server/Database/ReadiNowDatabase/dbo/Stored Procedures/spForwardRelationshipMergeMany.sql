-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE PROCEDURE [dbo].[spForwardRelationshipMergeMany]
	@data dbo.RelationshipType READONLY
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

	MERGE
		Relationship d
	USING
		@data AS s
	ON (
		d.TenantId = s.TenantId AND
		d.TypeId = s.TypeId AND
		d.FromId = s.FromId )
	WHEN NOT MATCHED
	THEN
		INSERT (
			TenantId,
			TypeId,
			FromId,
			ToId )
		VALUES (
			s.TenantId,
			s.TypeId,
			s.FromId,
			s.ToId )
	OUTPUT
		DELETED.TenantId,
		DELETED.TypeId,
		DELETED.FromId,
		DELETED.ToId
	INTO
		@output;

	SELECT
		TenantId,
		TypeId,
		FromId,
		ToId
	FROM
		@output
END