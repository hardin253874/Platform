-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE PROCEDURE [dbo].[spDebugCascade]
(
	@entityId BIGINT,
	@tenantId BIGINT
)
AS
BEGIN
	DECLARE @batchGuid UNIQUEIDENTIFIER = NEWID( )

	INSERT INTO
		Batch ( BatchGuid )
	VALUES
		( @batchGuid )

	DECLARE @batchId BIGINT = SCOPE_IDENTITY( )

	INSERT INTO
		EntityBatch ( BatchId, EntityId )
	VALUES
		( @batchId, @entityId )
	
	EXEC spDetermineCascadeBatch @batchId, @tenantId
	
	SELECT
		eb.EntityId,
		n.Data,
		rn.Data
	FROM
		EntityBatch  eb
	LEFT JOIN
		Entity e ON
			e.Id = eb.EntityId AND
			e.TenantId = @tenantId
	JOIN
		Relationship r ON
			e.TenantId = r.TenantId AND
			e.Id = r.FromId
	CROSS APPLY
		dbo.tblFnAliasNsId( 'isOfType', 'core', @tenantId ) iot
	OUTER APPLY
		dbo.tblFnFieldNVarCharA( eb.EntityId, @tenantId, 'name', 'core' ) n
	OUTER APPLY
		dbo.tblFnFieldNVarCharA( r.ToId, @tenantId, 'name', 'core' ) rn
	WHERE
		r.TypeId = iot.EntityId AND
		BatchId = @batchId
	ORDER BY
		r.ToId
end
