-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE PROCEDURE [dbo].[spDeleteBatchDetails]
(
	@batchId BIGINT,
	@tenantId BIGINT
)
AS
BEGIN
	
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	CREATE TABLE #original ( EntityId BIGINT PRIMARY KEY )

	-- Store the original entities in a temporary table so that
	-- we can exclude them from the results later
	INSERT INTO
		#original
	SELECT
		EntityId
	FROM
		EntityBatch
	WHERE
		BatchId = @batchId

	-- Extends the batch to include cascaded entries
	EXEC spDetermineCascadeBatch @batchId, @tenantId, 0

	-- Store the ids for the commonly used aliases
	DECLARE @isOfType BIGINT = dbo.fnAliasNsId( 'isOfType', 'core', @tenantId )
	DECLARE @inherits BIGINT = dbo.fnAliasNsId( 'inherits', 'core', @tenantId )
	DECLARE @definition BIGINT = dbo.fnAliasNsId( 'definition', 'core', @tenantId )
	DECLARE @name BIGINT = dbo.fnAliasNsId( 'name', 'core', @tenantId )
	DECLARE @description BIGINT = dbo.fnAliasNsId( 'description', 'core', @tenantId )
	DECLARE @excludeFromDeleteDetails BIGINT = dbo.fnAliasNsId( 'excludeFromDeleteDetails', 'core', @tenantId )

	-- The top-level types that are of interest are cached here so that they
	-- do not have to be resolved more than once.
	-- Add new values as appropriate.
	CREATE TABLE #types ( Id BIGINT PRIMARY KEY, TypeName NVARCHAR( 100 ) )
	INSERT INTO #types SELECT Id, 'Definition' FROM dbo.fnDerivedTypes( dbo.fnAliasNsId( 'definition', 'core', @tenantId ), @tenantId )
	INSERT INTO #types SELECT Id, 'Report' FROM dbo.fnDerivedTypes( dbo.fnAliasNsId( 'report', 'core', @tenantId ), @tenantId )
	INSERT INTO #types SELECT Id, 'Chart' FROM dbo.fnDerivedTypes( dbo.fnAliasNsId( 'chart', 'core', @tenantId ), @tenantId )
	INSERT INTO #types SELECT Id, 'Edit Form' FROM dbo.fnDerivedTypes( dbo.fnAliasNsId( 'customEditForm', 'console', @tenantId ), @tenantId )
	INSERT INTO #types SELECT Id, 'Field' FROM dbo.fnDerivedTypes( dbo.fnAliasNsId( 'field', 'core', @tenantId ), @tenantId )
	INSERT INTO #types SELECT Id, 'Workflow' FROM dbo.fnDerivedTypes( dbo.fnAliasNsId( 'workflow', 'core', @tenantId ), @tenantId )
	INSERT INTO #types SELECT Id, 'Screen' FROM dbo.fnDerivedTypes( dbo.fnAliasNsId( 'screen', 'console', @tenantId ), @tenantId )

	INSERT INTO
		#types
	SELECT
		r.FromId, n.Data_StartsWith
	FROM
		fnDescendantsAndSelf( @inherits, @definition, @tenantId ) a
	JOIN
		Relationship r ON
			a.Id = r.ToId
			AND r.TenantId = @tenantId
			AND r.TypeId = @isOfType
	JOIN
		Data_NVarChar n ON
			r.FromId = n.EntityId
			AND r.TenantId = n.TenantId
			AND n.FieldId = @name
	
	-- The relationship types that are to be excluded when walking the graph.
	-- Add new values as appropriate
	CREATE TABLE #excludedRelationships ( Id BIGINT PRIMARY KEY )
	INSERT INTO #excludedRelationships SELECT dbo.fnAliasNsId( 'inSolution', 'core', @tenantId )
	INSERT INTO #excludedRelationships SELECT dbo.fnAliasNsId( 'securityOwner', 'core', @tenantId )
	INSERT INTO #excludedRelationships SELECT dbo.fnAliasNsId( 'isOfType', 'core', @tenantId )
	INSERT INTO #excludedRelationships SELECT dbo.fnAliasNsId( 'createdBy', 'core', @tenantId )
	INSERT INTO #excludedRelationships SELECT dbo.fnAliasNsId( 'lastModifiedBy', 'core', @tenantId )
	INSERT INTO #excludedRelationships SELECT dbo.fnAliasNsId( 'resourceReportNodeType', 'core', @tenantId )
	INSERT INTO #excludedRelationships SELECT dbo.fnAliasNsId( 'reportExpressionResultType', 'core', @tenantId )
	INSERT INTO #excludedRelationships SELECT dbo.fnAliasNsId( 'fromType', 'core', @tenantId )
	INSERT INTO #excludedRelationships SELECT dbo.fnAliasNsId( 'toType', 'core', @tenantId )
	INSERT INTO #excludedRelationships SELECT dbo.fnAliasNsId( 'inherits', 'core', @tenantId )
	INSERT INTO #excludedRelationships SELECT dbo.fnAliasNsId( 'fieldGroupBelongsToType', 'core', @tenantId )
	INSERT INTO #excludedRelationships SELECT dbo.fnAliasNsId( 'resourceHasResourceKeyDataHashes', 'core', @tenantId )
	INSERT INTO #excludedRelationships SELECT dbo.fnAliasNsId( 'fieldInKey', 'core', @tenantId )
	INSERT INTO #excludedRelationships SELECT dbo.fnAliasNsId( 'fieldIsOnType', 'core', @tenantId )
	INSERT INTO #excludedRelationships SELECT dbo.fnAliasNsId( 'reportUsesDefinition', 'core', @tenantId )

	-- Return cascade deleted top-level entities
	SELECT
		b.EntityId,
		[Name] = n.Data_StartsWith,
		[Description] = d.Data_StartsWith,
		t.TypeName
	FROM
		EntityBatch b
	JOIN
		Relationship r ON
			r.TenantId = @tenantId AND
			r.FromId = b.EntityId AND
			r.TypeId = @isOfType
	JOIN
		#types t ON
			r.ToId = t.Id
	LEFT JOIN
		Data_NVarChar n ON
			n.TenantId = @tenantId AND
			n.EntityId = b.EntityId AND
			n.FieldId = @name
	LEFT JOIN
		Data_NVarChar d ON
			d.TenantId = @tenantId AND
			d.EntityId = b.EntityId AND
			d.FieldId = @description
	LEFT JOIN
		#original o ON
			o.EntityId = b.EntityId
	WHERE
		b.BatchId = @batchId AND
		o.EntityId IS NULL
	ORDER BY
		[Name]

	CREATE TABLE #visited ( Id BIGINT PRIMARY KEY, Depth INT, Direction NVARCHAR( 10 ) )

	-- Start with the original values.
	INSERT INTO
		#visited
	SELECT
		EntityId,
		0,
		'Reverse'
	FROM
		#original

	CREATE TABLE #candidates ( Id BIGINT PRIMARY KEY )

	-- Candidates begin with the original set or entities.
	INSERT INTO
		#candidates
	SELECT
		EntityId
	FROM
		#original

	CREATE TABLE #pass ( Id BIGINT PRIMARY KEY )

	DECLARE @continue BIT = 1

	DECLARE @depth INT = 1

	WHILE ( @continue = 1 )
	BEGIN

		-- Determine the entities in the reverse direction that may be affected by the
		-- removal of the original entities.
		INSERT INTO
			#pass
		SELECT DISTINCT 
			r.FromId
		FROM
			Relationship r
		JOIN
			#candidates c ON
				r.ToId = c.Id
		LEFT JOIN
			#excludedRelationships e ON
				e.Id = r.TypeId
		LEFT JOIN
			#visited v ON
				r.FromId = v.Id
		WHERE
			r.TenantId = @tenantId AND
			v.Id IS NULL AND
			e.Id IS NULL

		IF ( @@ROWCOUNT = 0 )
		BEGIN
			SET @continue = 0
		END

		-- Store all entities that have been visited to ensure cycles in the graph are handled
		INSERT INTO
			#visited
		SELECT
			Id,
			@depth,
			'Reverse'
		FROM
			#pass

		-- Empty out the candidate table for the next pass.
		TRUNCATE TABLE #candidates

		-- Populate the candidates with the entities from the current pass.
		INSERT INTO
			#candidates
		SELECT
			Id
		FROM
			#pass

		-- Empty out the current pass entities.
		TRUNCATE TABLE #pass

		SET @depth = @depth + 1
	END

	-- Empty out the candidate table for the next pass.
	TRUNCATE TABLE #candidates

	-- Set depth back to one for the forward relationships.
	SET @depth = 1

	-- Initialize the candidate list to the original entity list.
	INSERT INTO
		#candidates
	SELECT
		EntityId
	FROM
		#original

	SET @continue = 1

	WHILE ( @continue = 1 )
	BEGIN

		-- Determine the entities in the forward direction that may be affected by the
		-- removal of the original entities.
		INSERT INTO
			#pass
		SELECT DISTINCT 
			r.ToId
		FROM
			Relationship r
		JOIN
			#candidates c ON
				r.FromId = c.Id
		LEFT JOIN
			#excludedRelationships e ON
				e.Id = r.TypeId
		LEFT JOIN
			#visited v ON
				r.ToId = v.Id
		WHERE
			r.TenantId = @tenantId AND
			v.Id IS NULL AND
			e.Id IS NULL

		IF ( @@ROWCOUNT = 0 )
		BEGIN
			SET @continue = 0
		END

		-- Store all entities that have been visited to ensure cycles in the graph are handled
		INSERT INTO
			#visited
		SELECT
			Id,
			@depth,
			'Forward'
		FROM
			#pass

		-- Empty out the candidate table for the next pass.
		TRUNCATE TABLE #candidates

		-- Populate the candidates with the entities from the current pass.
		INSERT INTO
			#candidates
		SELECT
			Id
		FROM
			#pass

		-- Empty out the current pass entities.
		TRUNCATE TABLE #pass

		--SELECT *, @depth FROM #candidates

		SET @depth = @depth + 1
	END

	-- Return the list of entities that may be affected by the removal of the original entities.
	SELECT
		v.Id,
		[Name] = n.Data_StartsWith,
		[Description] = d.Data_StartsWith,
		t.TypeName,
		v.Depth,
		v.Direction
	FROM
		#visited v
	JOIN
		Relationship r ON
			v.Id = r.FromId AND
			r.TenantId = @tenantId AND
			r.TypeId = @isOfType
	JOIN
		#types t ON
			r.ToId = t.Id
	LEFT JOIN
		EntityBatch b ON
			v.Id = b.EntityId AND
			b.BatchId = @batchId
	LEFT JOIN
		Data_NVarChar n ON
			n.TenantId = r.TenantId AND
			n.EntityId = v.Id AND
			n.FieldId = @name
	LEFT JOIN
		Data_NVarChar d ON
			d.TenantId = @tenantId AND
			d.EntityId = b.EntityId AND
			d.FieldId = @description
	LEFT JOIN
		#original o ON
			o.EntityId = b.EntityId
	WHERE
		b.EntityId IS NULL AND
		o.EntityId IS NULL
	ORDER BY
		Depth,
		[Name]

	-- Cleanup
	DROP TABLE #visited
	DROP TABLE #types
	DROP TABLE #excludedRelationships
	DROP TABLE #candidates
	DROP TABLE #pass
	DROP TABLE #original
END