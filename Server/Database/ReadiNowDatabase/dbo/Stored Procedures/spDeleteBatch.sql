-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE PROCEDURE [dbo].[spDeleteBatch]
(
	@batchId BIGINT,
	@tenantId BIGINT,
	@showOutput BIT = 1,
	@excludeExternallyReferencedEntities BIT = 0
)
AS
BEGIN
	-- If excludeExternallyReferencedEntities is set, the externally referenced
	-- entities must reside in the # temp table '#externallyReferencedEntities'
	-- on the current connection.

	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	-- Extends the batch to include cascaded entries
	EXEC spDetermineCascadeBatch @batchId, @tenantId, @excludeExternallyReferencedEntities		

	DECLARE @transactionId BIGINT

	EXEC spCreateRestorePoint @tenantId, DEFAULT, DEFAULT, DEFAULT, DEFAULT, @transactionId OUTPUT

	DECLARE @existingContextInfo VARBINARY(128) = CONTEXT_INFO()
	DECLARE @existingContext VARCHAR(128)

	IF ( @existingContextInfo IS NOT NULL )
	BEGIN
		SET @existingContext  = REPLACE( CAST( CAST( @existingContextInfo AS VARCHAR( 128 ) ) COLLATE SQL_Latin1_General_CP1_CS_AS AS VARCHAR( 128 ) ), CHAR( 0 ), '' )
	END

	DECLARE @userId BIGINT = NULL

	IF ( @existingContext IS NOT NULL AND LEN( @existingContext ) > 0 )
	BEGIN
		IF ( SUBSTRING( @existingContext, 1, 2 ) = 'u:' )
		BEGIN
			DECLARE @userEndPosition INT = CHARINDEX(',', @existingContext )

			DECLARE @userIdString VARCHAR( 128 )

			IF ( @userEndPosition > 2 )
			BEGIN
				SET @userIdString = SUBSTRING( @existingContext, 3, @userEndPosition - 3 )

				SET @userId = TRY_CONVERT( BIGINT, @userIdString )
			END
			ELSE
			BEGIN
				IF ( LEN( @existingContext ) > 2 )
				BEGIN
					SET @userIdString = SUBSTRING( @existingContext, 3, LEN( @existingContext ) - 2 )

					SET @userId = TRY_CONVERT( BIGINT, @userIdString )
				END
			END
		END
	END

	IF ( @userId IS NOT NULL )
	BEGIN
		DECLARE @newContextInfo VARBINARY(128) = CONVERT( VARBINARY(128), 'u:' + CAST( @userId AS VARCHAR(128) ) + ',' + CAST( @transactionId AS VARCHAR( 128 ) ) )
		SET CONTEXT_INFO @newContextInfo
	END
	
	-- Delete from Data_Alias
	DELETE
		d
	FROM
		Data_Alias d
	JOIN
		EntityBatch eb ON
			d.TenantId = @tenantId AND
			d.EntityId = eb.EntityId
	WHERE
		eb.BatchId = @batchId

	DELETE
		d
	FROM
		Data_Alias d
	JOIN
		EntityBatch eb ON
			d.TenantId = @tenantId AND
			d.FieldId = eb.EntityId
	WHERE
		eb.BatchId = @batchId

	-- Delete from Data_Bit
	DELETE
		d
	FROM
		Data_Bit d
	JOIN
		EntityBatch eb ON
			d.TenantId = @tenantId AND
			d.EntityId = eb.EntityId
	WHERE
		eb.BatchId = @batchId

	DELETE
		d
	FROM
		Data_Bit d
	JOIN
		EntityBatch eb ON
			d.TenantId = @tenantId AND
			d.FieldId = eb.EntityId
	WHERE
		eb.BatchId = @batchId

	-- Delete from Data_DateTime
	DELETE
		d
	FROM
		Data_DateTime d
	JOIN
		EntityBatch eb ON
			d.TenantId = @tenantId AND
			d.EntityId = eb.EntityId
	WHERE
		eb.BatchId = @batchId

	DELETE
		d
	FROM
		Data_DateTime d
	JOIN
		EntityBatch eb ON
			d.TenantId = @tenantId AND
			d.FieldId = eb.EntityId
	WHERE
		eb.BatchId = @batchId

	-- Delete from Data_Decimal
	DELETE
		d
	FROM
		Data_Decimal d
	JOIN
		EntityBatch eb ON
			d.TenantId = @tenantId AND
			d.EntityId = eb.EntityId
	WHERE
		eb.BatchId = @batchId

	DELETE
		d
	FROM
		Data_Decimal d
	JOIN
		EntityBatch eb ON
			d.TenantId = @tenantId AND
			d.FieldId = eb.EntityId
	WHERE
		eb.BatchId = @batchId

	-- Delete from Data_Guid
	DELETE
		d
	FROM
		Data_Guid d
	JOIN
		EntityBatch eb ON
			d.TenantId = @tenantId AND
			d.EntityId = eb.EntityId
	WHERE
		eb.BatchId = @batchId

	DELETE
		d
	FROM
		Data_Guid d
	JOIN
		EntityBatch eb ON
			d.TenantId = @tenantId AND
			d.FieldId = eb.EntityId
	WHERE
		eb.BatchId = @batchId

	-- Delete from Data_Int
	DELETE
		d
	FROM
		Data_Int d
	JOIN
		EntityBatch eb ON
			d.TenantId = @tenantId AND
			d.EntityId = eb.EntityId
	WHERE
		eb.BatchId = @batchId

	DELETE
		d
	FROM
		Data_Int d
	JOIN
		EntityBatch eb ON
			d.TenantId = @tenantId AND
			d.FieldId = eb.EntityId
	WHERE
		eb.BatchId = @batchId

	-- Delete from Data_NVarChar
	DELETE
		d
	FROM
		Data_NVarChar d WITH ( INDEX ( PK_Data_NVarChar ) )
	JOIN
		EntityBatch eb ON
			d.TenantId = @tenantId AND
			d.EntityId = eb.EntityId
	WHERE
		eb.BatchId = @batchId

	DELETE
		d
	FROM
		Data_NVarChar d WITH ( INDEX ( IDX_Data_NVarChar_SearchField ) )
	JOIN
		EntityBatch eb ON
			d.TenantId = @tenantId AND
			d.FieldId = eb.EntityId
	WHERE
		eb.BatchId = @batchId

	-- Delete from Data_Xml
	DELETE
		d
	FROM
		Data_Xml d
	JOIN
		EntityBatch eb ON
			d.TenantId = @tenantId AND
			d.EntityId = eb.EntityId
	WHERE
		eb.BatchId = @batchId

	DELETE
		d
	FROM
		Data_Xml d
	JOIN
		EntityBatch eb ON
			d.TenantId = @tenantId AND
			d.FieldId = eb.EntityId
	WHERE
		eb.BatchId = @batchId
	
	IF @showOutput = 1
	BEGIN
		DECLARE @isOfType BIGINT = dbo.fnAliasNsId( 'isOfType', 'core', @tenantId )
	
		SELECT
			EntityId = FromId,
			TypeId = ToId
		FROM
			Relationship r
		JOIN
			EntityBatch eb ON
				r.FromId = eb.EntityId AND
				r.TypeId = @isOfType AND
				eb.BatchId = @batchId AND
				r.TenantId = @tenantId
		ORDER BY
			r.FromId
	END

	-- Note: Need to test for FromId, ToId, TypeId, as these are no longer included in the cascade result due to nullable entity ids.
	DELETE
		r
	FROM
		Relationship r
	JOIN
		EntityBatch eb ON
			r.FromId = eb.EntityId AND
			eb.BatchId = @batchId AND
			r.TenantId = @tenantId
	
	DELETE
		r
	FROM
		Relationship r
	JOIN
		EntityBatch eb ON
			r.ToId = eb.EntityId AND
			r.TenantId = @tenantId AND
			eb.BatchId = @batchId AND
			r.TenantId = @tenantId

	DELETE
		r
	FROM
		Relationship r
	JOIN
		EntityBatch eb ON
			r.TypeId = eb.EntityId AND
			r.TenantId = @tenantId AND
			eb.BatchId = @batchId AND
			r.TenantId = @tenantId

	-- Note: don't need to test for TypeId, as already included in cascaded result
	-- Note: turn count on - this is the only count reported, the total number of entities deleted

	SET NOCOUNT OFF;

	DELETE
			e
		FROM
			Entity e
		JOIN
			EntityBatch eb ON
			e.Id = eb.EntityId
		WHERE
			eb.BatchId = @batchId AND
			e.TenantId = @tenantId

	SET NOCOUNT ON;

	-- Clear the current batch items
	DELETE
		eb
	FROM
		EntityBatch eb
	WHERE
		eb.BatchId = @batchId

	IF ( @existingContextInfo IS NOT NULL )
	BEGIN
		SET CONTEXT_INFO @existingContextInfo
	END
		
END
