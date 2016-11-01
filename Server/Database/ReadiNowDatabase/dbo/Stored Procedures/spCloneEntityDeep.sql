-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE PROCEDURE [dbo].[spCloneEntityDeep]
	@entities dbo.EntityMapType READONLY,
	@tenantId BIGINT,
	@debug BIT = 0
AS
BEGIN

	DECLARE @debugCount INT

	SET NOCOUNT ON

	-----
	-- Store the mapping from source entity to destination entity.
	-----
	CREATE TABLE #ClonedEntities
	(
		SourceId BIGINT PRIMARY KEY,
		DestinationId BIGINT
	)

	-----
	-- Setup the current iteration.
	-----
	CREATE TABLE #thisIter
	(
		EntityId BIGINT PRIMARY KEY CLUSTERED

	)

	-----
	-- Setup the next iteration.
	-----
	CREATE TABLE #nextIter
	(
		EntityId BIGINT PRIMARY KEY CLUSTERED
	)

	-----
	-- Stores the cloned entities and their relationship
	-- information due to the cloneEntities action.
	-----
	CREATE TABLE #clonedRelationshipEntities
	(
		SourceId BIGINT,
		DestinationId BIGINT,
		TypeId BIGINT,
		FromId BIGINT,
		ToId BIGINT,
		InsertedId BIGINT
	)

	CREATE NONCLUSTERED INDEX [IDX_clonedRelationshipEntities_From] ON #clonedRelationshipEntities ([FromId]) INCLUDE ([InsertedId])
	CREATE NONCLUSTERED INDEX [IDX_clonedRelationshipEntities_To] ON #clonedRelationshipEntities ([ToId]) INCLUDE ([InsertedId])

	-----
	-- Add the initial value into the current iteration
	-----
	INSERT INTO
		#thisIter
		(
			EntityId
		)
	SELECT
		SourceId
	FROM
		@entities
	
	SET @debugCount = @@ROWCOUNT
	
	IF @debug = 1
	BEGIN
		PRINT 'Initial @thisIter contains ' + CAST( @debugCount AS NVARCHAR( 10 ) )+ ' row.'
	END

	-----
	-- Precache the identifiers for repetitive operations.
	-----
	DECLARE @cloneActionId				BIGINT = dbo.fnAliasNsId( 'cloneAction', 'core', @tenantId )
	DECLARE @reverseCloneActionId		BIGINT = dbo.fnAliasNsId( 'reverseCloneAction', 'core', @tenantId )
	DECLARE @cloneReferencesId			BIGINT = dbo.fnAliasNsId( 'cloneReferences', 'core', @tenantId )
	DECLARE @cloneEntitiesId			BIGINT = dbo.fnAliasNsId( 'cloneEntities', 'core', @tenantId )
	DECLARE @cardinalityId				BIGINT = dbo.fnAliasNsId( 'cardinality', 'core', @tenantId )
	DECLARE @cardinalityOneToOneId		BIGINT = dbo.fnAliasNsId( 'oneToOne', 'core', @tenantId )
	DECLARE @cardinalityOneToManyId		BIGINT = dbo.fnAliasNsId( 'oneToMany', 'core', @tenantId )
	DECLARE @cardinalityManyToOneId		BIGINT = dbo.fnAliasNsId( 'manyToOne', 'core', @tenantId )
	DECLARE @cardinalityManyToManyId	BIGINT = dbo.fnAliasNsId( 'manyToMany', 'core', @tenantId )

	-----
	-- Create the new entities storing the resulting mapping in the temporary table.
	-----
	INSERT INTO
		#ClonedEntities
		(
			SourceId,
			DestinationId
		)
	SELECT
		SourceId,
		DestinationId
	FROM
		@entities

	-----
	-- Loop until there are no more entities that are required to be  cloned.
	-----
	WHILE EXISTS
	(
		SELECT
			1
		FROM
			#thisIter
	)
	BEGIN

		IF @debug = 1
		BEGIN
			PRINT '------------------------------------------------'
		END
	
		-----
		-- Create the new forward relationship entities
		-----
		MERGE Entity AS t
		USING
		(
			SELECT
				c.SourceId,
				c.DestinationId,
				r.TenantId,
				r.TypeId,
				r.FromId,
				r.ToId
			FROM
				#thisIter i
			INNER JOIN
				Relationship r ON r.TenantId = @tenantId AND
				i.EntityId = r.FromId
			INNER JOIN
				#ClonedEntities c ON r.FromId = c.SourceId
			INNER JOIN
				Relationship r2 ON r.TenantId = r2.TenantId AND
				r.TypeId = r2.FromId AND
				r2.TypeId = @cloneActionId
			LEFT JOIN
				Relationship r3 ON r.TenantId = r3.TenantId AND
				r.TypeId = r3.FromId AND
				r3.TypeId = @cardinalityId
			LEFT JOIN
				#ClonedEntities ce ON ce.SourceId = r.ToId
			WHERE
				r2.ToId = @cloneEntitiesId
				AND
					ce.SourceId IS NULL
		) AS s
		(
			SourceId,
			DestinationId,
			TenantId,
			TypeId,
			FromId,
			ToId
		)
		ON 1 = 0
		WHEN NOT MATCHED BY TARGET THEN
			INSERT
			(
				TenantId,
				UpgradeId
			)
			VALUES
			(
				@tenantId,
				NEWID( )
			)
		OUTPUT
			s.SourceId,
			s.DestinationId,
			s.TypeId,
			s.FromId,
			s.ToId,
			inserted.Id
		INTO
			#clonedRelationshipEntities;
			
		SET @debugCount = @@ROWCOUNT
		
		IF @debug = 1
		BEGIN
			PRINT 'Added ' + CAST( @debugCount AS NVARCHAR( 10 ) )+ ' new Entities for Forward Relationship Entity rows.'
		END
		
		-----
		-- Place the newly cloned entities into the clone map.
		-----
		INSERT INTO
			#ClonedEntities
			(
				SourceId,
				DestinationId
			)
		SELECT
			rc.ToId,
			rc.InsertedId
		FROM
			#clonedRelationshipEntities rc
		LEFT JOIN
			#ClonedEntities c ON rc.ToId = c.SourceId
		WHERE
			c.DestinationId IS NULL
		
		SET @debugCount = @@ROWCOUNT
		
		IF @debug = 1
		BEGIN
			PRINT 'Added ' + CAST( @debugCount AS NVARCHAR( 10 ) )+ ' new rows into the #ClonedEntities map.'
		END
				
		-----
		-- Update the next iteration with the newly created entities.
		-----
		INSERT INTO
			#nextIter
			(
				EntityId
			)
		SELECT DISTINCT
			ToId
		FROM
			#clonedRelationshipEntities t
		LEFT JOIN
			#nextIter n ON t.ToId = n.EntityId
		WHERE
			n.EntityId IS NULL
		
		SET @debugCount = @@ROWCOUNT
		
		IF @debug = 1
		BEGIN
			PRINT 'Added ' + CAST( @debugCount AS NVARCHAR( 10 ) )+ ' values into the @nextIter.'
		END
		
		-----
		-- Flush the collection of cloned relationship entities.
		-----
		TRUNCATE TABLE #clonedRelationshipEntities
		
		-----
		-- Create the new reverse relationship entities
		-----
		MERGE Entity AS t
		USING (
			SELECT
				c.SourceId,
				c.DestinationId,
				r.TenantId,
				r.TypeId,
				r.FromId,
				r.ToId
			FROM
				#thisIter i
			INNER JOIN
				Relationship r ON r.TenantId = @tenantId AND
				i.EntityId = r.ToId
			INNER JOIN
				#ClonedEntities c ON r.ToId = c.SourceId
			INNER JOIN
				Relationship r2 ON r.TenantId = r2.TenantId AND
				r.TypeId = r2.FromId AND
				r2.TypeId = @reverseCloneActionId
			LEFT JOIN
				Relationship r3 ON r.TenantId = r3.TenantId AND
				r.TypeId = r3.FromId AND
				r3.TypeId = @cardinalityId
			LEFT JOIN
				#ClonedEntities ce ON ce.SourceId = r.FromId
			WHERE
				r2.ToId = @cloneEntitiesId
				AND
					ce.SourceId IS NULL
		) AS s
		(
			SourceId,
			DestinationId,
			TenantId,
			TypeId,
			FromId,
			ToId
		)
		ON 1 = 0
		WHEN NOT MATCHED BY TARGET THEN
			INSERT
			(
				TenantId,
				UpgradeId
			)
			VALUES
			(
				@tenantId,
				NEWID( )
			)
		OUTPUT
			s.SourceId,
			s.DestinationId,
			s.TypeId,
			s.FromId,
			s.ToId,
			inserted.Id
		INTO
			#clonedRelationshipEntities;
			
		SET @debugCount = @@ROWCOUNT
		
		IF @debug = 1
		BEGIN
			PRINT 'Added ' + CAST( @debugCount AS NVARCHAR( 10 ) )+ ' new Entities for Reverse Relationship Entity rows.'
		END
		
		-----
		-- Place the newly cloned entities into the clone map.
		-----
		INSERT INTO
			#ClonedEntities
			(
				SourceId,
				DestinationId
			)
		SELECT
			rc.FromId,
			rc.InsertedId
		FROM
			#clonedRelationshipEntities rc
		LEFT JOIN
			#ClonedEntities c ON rc.FromId = c.SourceId
		WHERE
			c.DestinationId IS NULL
		
		SET @debugCount = @@ROWCOUNT
		
		IF @debug = 1
		BEGIN
			PRINT 'Added ' + CAST( @debugCount AS NVARCHAR( 10 ) )+ ' new rows into the #ClonedEntities map.'
		END

		-----
		-- Update the next iteration with the newly created entities.
		-----
		INSERT INTO
			#nextIter
			(
				EntityId
			)
		SELECT DISTINCT
			FromId
		FROM
			#clonedRelationshipEntities t
		LEFT JOIN
			#nextIter n ON t.FromId = n.EntityId
		WHERE
			n.EntityId IS NULL
		
		SET @debugCount = @@ROWCOUNT
		
		IF @debug = 1
		BEGIN
			PRINT 'Added ' + CAST( @debugCount AS NVARCHAR( 10 ) )+ ' values into the @nextIter.'
		END
			
		-----
		-- Flush the collection of cloned relationship entities.
		-----
		TRUNCATE TABLE #clonedRelationshipEntities
		
		-----
		-- Flush the current iteration.
		-----
		TRUNCATE TABLE #thisIter
		
		-----
		-- Set the current iteration to be the next iteration.
		-----
		INSERT INTO #thisIter
		SELECT EntityId
		FROM #nextIter

		-----
		-- Flush the next iteration.
		-----
		TRUNCATE TABLE #nextIter
	END

	INSERT INTO
		#thisIter
		(
			EntityId
		)
	SELECT DISTINCT
		SourceId
	FROM
		#ClonedEntities

	IF @debug = 1
	BEGIN
		PRINT '------------------------------------------------'
	END

	
	-----
	-- Create the new forward relationship references
	-----
	MERGE Relationship AS t
	USING
	(
		SELECT
			fm.SourceId,
			fm.DestinationId,
			tm.DestinationId,
			r.TenantId,
			r.TypeId,
			r.FromId,
			r.ToId
		FROM
			#thisIter i
		INNER JOIN
			Relationship r ON r.TenantId = @tenantId AND
			i.EntityId = r.FromId
		LEFT JOIN
			#ClonedEntities fm ON r.FromId = fm.SourceId
		LEFT JOIN
			#ClonedEntities tm ON r.ToId = tm.SourceId
		LEFT JOIN
			Relationship c ON r.TenantId = c.TenantId AND
			r.TypeId = c.FromId AND
			c.TypeId = @cardinalityId
		LEFT JOIN (
			SELECT
				r.TenantId,
				r.TypeId,
				r.FromId,
				Count = COUNT ( r.ToId )
			FROM
				Relationship r
			WHERE
				r.TenantId = @tenantId
			GROUP BY
				r.TenantId,
				r.TypeId,
				r.FromId ) fwd ON r.TenantId = fwd.TenantId AND
					r.TypeId = fwd.TypeId AND
					ISNULL( fm.DestinationId, r.FromId ) = fwd.FromId
		LEFT JOIN (
			SELECT
				r.TenantId,
				r.TypeId,
				r.ToId,
				Count = COUNT ( r.FromId )
			FROM
				Relationship r
			WHERE
				r.TenantId = @tenantId
			GROUP BY
				r.TenantId,
				r.TypeId,
				r.ToId ) rev ON r.TenantId = rev.TenantId AND
					r.TypeId = rev.TypeId AND
					ISNULL( tm.DestinationId, r.ToId ) = rev.ToId
		LEFT JOIN
			Relationship e ON e.TenantId = r.TenantId
			AND
				e.TypeId = r.TypeId
			AND
				e.FromId = ISNULL( fm.DestinationId, r.FromId )
			AND
				e.ToId = ISNULL( tm.DestinationId, r.ToId )
		WHERE
		(
			c.ToId IS NULL
			OR
			(
				c.ToId = @cardinalityManyToManyId OR
				(
					c.ToId = @cardinalityOneToOneId AND
					(
						( fwd.Count = 0 OR fwd.Count IS NULL ) AND
						( rev.Count = 0 OR rev.Count IS NULL )
					)
				) OR (
					c.ToId = @cardinalityManyToOneId AND
					( fwd.Count = 0 OR fwd.Count IS NULL )
				) OR (
					c.ToId = @cardinalityOneToManyId AND
					( rev.Count = 0 OR rev.Count IS NULL )
				)
			)
		)
		AND
			e.TypeId IS NULL
	) AS s
	(
		SourceId,
		DestinationId,
		DestinationId2,
		TenantId,
		TypeId,
		FromId,
		ToId
	)
	ON 1 = 0
	WHEN NOT MATCHED BY TARGET THEN
		INSERT
		(
			TenantId,
			TypeId,
			FromId,
			ToId
		)
		VALUES
		(
			s.TenantId,
			s.TypeId,
			s.DestinationId,
			ISNULL( DestinationId2, s.ToId )
		);
			
	SET @debugCount = @@ROWCOUNT
			
	IF @debug = 1
	BEGIN
		PRINT 'Added ' + CAST( @debugCount AS NVARCHAR( 10 ) )+ ' new Forward Relationship Reference rows.'
	END
	

	-----
	-- Create the new reverse relationship references
	-----
	MERGE Relationship AS t
	USING
	(
		SELECT
			tm.SourceId,
			tm.DestinationId,
			fm.DestinationId,
			r.TenantId,
			r.TypeId,
			r.FromId,
			r.ToId
		FROM
			#thisIter i
		INNER JOIN
			Relationship r ON r.TenantId = @tenantId AND
			i.EntityId = r.ToId
		LEFT JOIN
			#ClonedEntities fm ON r.FromId = fm.SourceId
		LEFT JOIN
			#ClonedEntities tm ON r.ToId = tm.SourceId
		LEFT JOIN
			Relationship c ON r.TenantId = c.TenantId AND
			r.TypeId = c.FromId AND
			c.TypeId = @cardinalityId
		LEFT JOIN (
			SELECT
				r.TenantId,
				r.TypeId,
				r.FromId,
				Count = COUNT ( r.ToId )
			FROM
				Relationship r
			WHERE
				r.TenantId = @tenantId
			GROUP BY
				r.TenantId,
				r.TypeId,
				r.FromId ) fwd ON r.TenantId = fwd.TenantId AND
					r.TypeId = fwd.TypeId AND
					ISNULL( fm.DestinationId, r.FromId ) = fwd.FromId
		LEFT JOIN (
			SELECT
				r.TenantId,
				r.TypeId,
				r.ToId,
				Count = COUNT ( r.FromId )
			FROM
				Relationship r
			WHERE
				r.TenantId = @tenantId
			GROUP BY
				r.TenantId,
				r.TypeId,
				r.ToId ) rev ON r.TenantId = rev.TenantId AND
					r.TypeId = rev.TypeId AND
					ISNULL( tm.DestinationId, r.ToId ) = rev.ToId
		LEFT JOIN
			Relationship e ON e.TenantId = r.TenantId
			AND
				e.TypeId = r.TypeId
			AND
				e.FromId = ISNULL( fm.DestinationId, r.FromId )
			AND
				e.ToId = ISNULL( tm.DestinationId, r.ToId )
		WHERE
		(
			c.ToId IS NULL
			OR
			(
				c.ToId = @cardinalityManyToManyId OR
				(
					c.ToId = @cardinalityOneToOneId AND
					(
						( fwd.Count = 0 OR fwd.Count IS NULL ) AND
						( rev.Count = 0 OR rev.Count IS NULL )
					)
				) OR (
					c.ToId = @cardinalityManyToOneId AND
					( fwd.Count = 0 OR fwd.Count IS NULL )
				) OR (
					c.ToId = @cardinalityOneToManyId AND
					( rev.Count = 0 OR rev.Count IS NULL )
				)
			)
		)
		AND
			e.TypeId IS NULL
	) AS s
	(
		SourceId,
		DestinationId,
		DestinationId2,
		TenantId,
		TypeId,
		FromId,
		ToId
	)
	ON 1 = 0
	WHEN NOT MATCHED BY TARGET THEN
		INSERT
		(
			TenantId,
			TypeId,
			FromId,
			ToId
		)
		VALUES
		(
			s.TenantId,
			s.TypeId,
			ISNULL( s.DestinationId2, s.FromId ),
			s.DestinationId
		);
		
	SET @debugCount = @@ROWCOUNT
		
	IF @debug = 1
	BEGIN
		PRINT 'Added ' + CAST( @debugCount AS NVARCHAR( 10 ) )+ ' new Reverse Relationship Reference rows.'
	END

	-----
	-- Clone data
	-----
	DECLARE @sql NVARCHAR(MAX) =
	'INSERT INTO ?
	(
		EntityId,
		TenantId,
		FieldId,
		Data
		{{,ExtraFields}}
	)
	SELECT
		c.DestinationId,
		d.TenantId,
		d.FieldId,
		d.Data
		{{,d.ExtraFields}}
	FROM
		? d
	INNER JOIN
		#ClonedEntities c ON d.EntityId = c.SourceId
	WHERE
		d.TenantId = ' + CAST( @tenantId AS NVARCHAR( MAX ) )

	-----					
	-- Run for every data table
	-----
	EXEC spExecForDataTables @sql, 'Data_Alias'

	SELECT *
	FROM #ClonedEntities 

	DROP TABLE #ClonedEntities
	DROP TABLE #thisIter
	DROP TABLE #nextIter
	DROP TABLE #clonedRelationshipEntities
						
END