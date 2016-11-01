-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE PROCEDURE [dbo].[spDetermineCascadeBatch]
(
	@batchId BIGINT,
	@tenantId BIGINT,
	@excludeExternallyReferencedEntities BIT = 0
)
AS
BEGIN
	SET NOCOUNT ON

	-- If excludeExternallyReferencedEntities is set, the externally referenced
	-- entities must reside in the # temp table '#externallyReferencedEntities'
	-- on the current connection.

	-- To resolve undefined references create the temp table if it doesn't exist
	DECLARE @managedTempTable BIT = 0

	IF OBJECT_ID('tempdb..#externallyReferencedEntities') IS NULL
	BEGIN
		CREATE TABLE #externallyReferencedEntities ( Id BIGINT )

		SET @managedTempTable = 1
	END

	-- Usage:
	-- 1. Generate a new @batchId = newid()
	-- 2. Insert a row or rows of EntityIds into EntityBatch, using batch id
	-- 3. Call spDetermineCascadeBatch, passing the batch id
	-- 4. spDetermineCascadeBatch will add additional rows to the batch
	-- 5. view results by selecting from EntityBatch with the same batch id
	-- 6. Delete the batch from EntityBatch when you're done

	CREATE TABLE #thisIter 
	(
		EntityId BIGINT
	)
	CREATE CLUSTERED INDEX [#ixthisIterEntityId] ON [#thisIter] (EntityId)
	
	CREATE TABLE #nextIter 
	(
		EntityId BIGINT
	)	
	CREATE CLUSTERED INDEX [#ixnextIterEntityId] ON [#nextIter] (EntityId)


	INSERT INTO
		#thisIter
	SELECT
		EntityId
	FROM
		EntityBatch
	WHERE
		BatchId = @batchId

	DECLARE @cascadeDeleteFrom BIGINT = dbo.fnAliasNsId( 'cascadeDelete', 'core', @tenantId )
	DECLARE @cascadeDeleteTo BIGINT = dbo.fnAliasNsId( 'cascadeDeleteTo', 'core', @tenantId )

	WHILE EXISTS (
		SELECT
			1
		FROM
			#thisIter )
	BEGIN

		-- START identifying cascades:

		-- Cascade from one entity to another through cascadeDeleteFrom relationships
		INSERT INTO
			#nextIter
		SELECT
			r.FromId
		FROM
			Relationship r
		JOIN
			#thisIter cur ON
				r.TenantId = @tenantId AND
				r.ToId = cur.EntityId
		JOIN
			Data_Bit d ON
				d.TenantId = @tenantId
				AND d.EntityId = r.TypeId
				AND d.FieldId = @cascadeDeleteFrom
				AND d.Data = 1
		WHERE
			r.TenantId = @tenantId
			-- Check that there aren't other many-to-many relationships holding onto it
			-- I.e. cascade-delete triggers after the last reference is deleted
			AND NOT EXISTS (
				SELECT
					1
				FROM
					Relationship rAlt
				WHERE
					rAlt.TypeId = r.TypeId AND
					rAlt.TenantId = @tenantId AND
					rAlt.FromId = r.FromId AND
					-- Except if they've also been deleted
					NOT EXISTS (
						SELECT
							1
						FROM
							#thisIter ti
						WHERE
							ti.EntityId = rAlt.ToId AND
							rAlt.TenantId = @tenantId
						) AND
					NOT EXISTS (
						SELECT
							1
						FROM
							EntityBatch eb
						WHERE
							eb.EntityId = rAlt.ToId AND
							eb.BatchId = @batchId AND
							rAlt.TenantId = @tenantId
						)
			) 
				
		-- Cascade from one entity to another through cascadeDeleteTo relationships
		INSERT INTO
			#nextIter
		SELECT
			r.ToId
		FROM
			Relationship r
		JOIN
			#thisIter cur ON
				r.TenantId = @tenantId AND
				r.FromId = cur.EntityId
		JOIN
			Data_Bit d ON
				d.TenantId = @tenantId
				AND d.EntityId = r.TypeId
				AND d.FieldId = @cascadeDeleteTo
				AND d.Data = 1
		WHERE
			r.TenantId = @tenantId AND
			-- Check that there aren't other many-to-many relationships holding onto it
			-- I.e. cascade-delete triggers after the last reference is deleted
			NOT EXISTS (
				SELECT
					1
				FROM
					Relationship rAlt
				WHERE
					rAlt.TypeId = r.TypeId AND
					rAlt.TenantId = @tenantId AND
					rAlt.ToId = r.ToId AND
					-- Except if they've also been deleted
					NOT EXISTS (
						SELECT
							1
						FROM
							#thisIter ti
						WHERE
							ti.EntityId = rAlt.FromId AND
							rAlt.TenantId = @tenantId
					) AND
					NOT EXISTS (
						SELECT
							1
						FROM
							EntityBatch eb
						WHERE
							eb.EntityId = rAlt.FromId AND
							eb.BatchId = @batchId AND
							rAlt.TenantId = @tenantId
					)
			)


		-- END identifying cascades
		
		-- Truffle shuffle:
		-- * Clear this iter
		-- * Add any new items from #nextIter to EntityBatch
		-- * Add any new items from #nextIter to #thisIter
		-- * Clear #nextIter
		--delete from #thisIter
		TRUNCATE TABLE #thisIter
		
		IF ( @excludeExternallyReferencedEntities = 1 )
		BEGIN
			INSERT INTO
				#thisIter
			OUTPUT
				@batchId,
				inserted.EntityId
			INTO
				EntityBatch
			SELECT DISTINCT
				EntityId
			FROM
				#nextIter ni
			LEFT JOIN
				#externallyReferencedEntities e ON
					ni.EntityId = e.Id
			WHERE
				NOT EXISTS (
					SELECT
						1
					FROM
						EntityBatch eb
					WHERE
						eb.BatchId = @batchId AND
						eb.EntityId = ni.EntityId
				) AND
				e.Id IS NULL
		END
		ELSE
		BEGIN
			INSERT INTO
				#thisIter
			OUTPUT
				@batchId,
				inserted.EntityId
			INTO
				EntityBatch
			SELECT DISTINCT
				EntityId
			FROM
				#nextIter ni
			WHERE
				NOT EXISTS (
					SELECT
						1
					FROM
						EntityBatch eb
					WHERE
						eb.BatchId = @batchId AND
						eb.EntityId = ni.EntityId
				)
		END
		
		TRUNCATE TABLE #nextIter
	END
	
	DROP TABLE #thisIter
	DROP TABLE #nextIter

	IF @managedTempTable = 1
	BEGIN
		DROP TABLE #externallyReferencedEntities
	END

	SET NOCOUNT OFF
END

