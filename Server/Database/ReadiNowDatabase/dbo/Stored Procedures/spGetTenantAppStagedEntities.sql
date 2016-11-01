-- Copyright 2011-2016 Global Software Innovation Pty Ltd


CREATE PROCEDURE [dbo].[spGetTenantAppStagedEntities] 
	@solutionId BIGINT,
	@tenant BIGINT,
	@outputResults BIT = 1,
	@selfContained BIT = 0
WITH RECOMPILE
AS
BEGIN
	-- Note* The #candidateList table is created externally on the connection prior to this call
	-- and is cleaned up externally also.

	SET NOCOUNT ON
	
	DECLARE @dependentApplication BIGINT = dbo.fnAliasNsId( 'dependentApplication', 'core', @tenant )
	DECLARE @dependencyApplication BIGINT = dbo.fnAliasNsId( 'dependencyApplication', 'core', @tenant )
	DECLARE @inSolution BIGINT = dbo.fnAliasNsId( 'inSolution', 'core', @tenant )
	DECLARE @implicitInSolution BIGINT = dbo.fnAliasNsId( 'implicitInSolution', 'core', @tenant )
	DECLARE @reverseImplicitInSolution BIGINT = dbo.fnAliasNsId( 'reverseImplicitInSolution', 'core', @tenant )
	DECLARE @isOfType BIGINT = dbo.fnAliasNsId( 'isOfType', 'core', @tenant )
	DECLARE @name BIGINT = dbo.fnAliasNsId( 'name', 'core', @tenant )
	DECLARE @depth INT = 0

	DECLARE @latestCandidates TABLE ( UpgradeId UNIQUEIDENTIFIER )

	IF ( @selfContained = 1)
	BEGIN
		CREATE TABLE #candidateList ( UpgradeId UNIQUEIDENTIFIER PRIMARY KEY, [Explicit] BIT )
		CREATE TABLE #dependents ( Id BIGINT PRIMARY KEY )
	END

	CREATE TABLE #candidateListParents ( UpgradeId UNIQUEIDENTIFIER, ParentUpgradeId UNIQUEIDENTIFIER, RelationshipTypeUpgradeId UNIQUEIDENTIFIER, [Info] NVARCHAR(24), Depth INT, [Explicit] BIT,
		PRIMARY KEY CLUSTERED 
		(
			[UpgradeId] ASC,
			[ParentUpgradeId] ASC,
			[RelationshipTypeUpgradeId] ASC,
			[Info] ASC,
			[Depth] ASC
		))

	;WITH Dependents(Id)
	AS
	(
		SELECT
			da.ToId
		FROM
			Relationship d
		LEFT JOIN
			Relationship da ON
				d.TenantId = da.TenantId
				AND d.FromId = da.FromId
				AND da.TypeId = @dependentApplication
		WHERE
			d.TenantId = @tenant
			AND d.TypeId = @dependencyApplication
			AND d.ToId = @solutionId
			AND da.ToId IS NOT NULL

		UNION ALL

		SELECT
			da.ToId
		FROM
			Relationship d
		JOIN
			Relationship da
				ON d.TenantId = da.TenantId
				AND d.FromId = da.FromId
				AND da.TypeId = @dependentApplication
		JOIN
			Dependents dd ON
				d.ToId = dd.Id
		WHERE
			d.TenantId = @tenant
			AND d.TypeId = @dependencyApplication
	)
	INSERT INTO
		#dependents
	SELECT
		DISTINCT Id
	FROM
		Dependents

	INSERT INTO
		#candidateListParents
	OUTPUT
		INSERTED.UpgradeId
	INTO
		@latestCandidates
	-- Select all the entities that have an explicit 'inSolution' forward relationship to the solution requested.
	SELECT DISTINCT
		e.UpgradeId,
		CAST( 0x0 AS UNIQUEIDENTIFIER ),
		CAST( 0x0 AS UNIQUEIDENTIFIER ),
		'explicit',
		@depth,
		1
	FROM
		Entity e
	JOIN
		Relationship r ON
			r.TenantId = @tenant AND
			r.FromId = e.Id
	WHERE
		e.TenantId = @tenant AND
		r.TypeId = @inSolution AND
		r.ToId = @solutionId

	UNION

	-- Add the solution entity itself.
	SELECT
		UpgradeId,
		CAST( 0x0 AS UNIQUEIDENTIFIER ),
		CAST( 0x0 AS UNIQUEIDENTIFIER ),
		'explicit',
		@depth,
		1
	FROM
		Entity
	WHERE
		Id = @solutionId AND
		TenantId = @tenant

	-- Do the bulk of the loop work once outside of the loop
	CREATE TABLE #forwardCandidates ( FromUpgradeId UNIQUEIDENTIFIER, ToUpgradeId UNIQUEIDENTIFIER, TypeUpgradeId UNIQUEIDENTIFIER )

	CREATE NONCLUSTERED INDEX [IDX_forwardCandidates] ON #forwardCandidates
	(
		[FromUpgradeId] ASC
	)

	-- Store all possible entities that don't have an explicit 'inSolution' relationship
	-- but are marked with 'implicitInSolution'.
	INSERT INTO
		#forwardCandidates
	SELECT
		e.UpgradeId,
		e2.UpgradeId,
		e3.UpgradeId
	FROM
		Relationship r
	JOIN
		Entity e ON
			e.TenantId = @tenant AND
			e.Id = r.FromId
	LEFT JOIN
		Data_Bit db ON
			db.TenantId = @tenant AND
			db.EntityId = r.TypeId AND
			db.FieldId = @implicitInSolution
	JOIN
		Entity e2 ON
			e2.TenantId = @tenant AND
			e2.Id = r.ToId
	JOIN
		Entity e3 ON
			e3.TenantId = @tenant AND
			e3.Id = r.TypeId
	WHERE
		ISNULL( db.Data, 0 ) = 1 AND
		r.TenantId = @tenant AND
		NOT EXISTS (
				SELECT
					1
				FROM
					Relationship eis
				JOIN
					#dependents d ON
						eis.ToId = d.Id
				WHERE
					eis.TenantId = @tenant AND
					eis.FromId = e2.Id AND
					eis.TypeId = @inSolution
			)

	CREATE TABLE #reverseCandidates ( ToUpgradeId UNIQUEIDENTIFIER, FromUpgradeId UNIQUEIDENTIFIER, TypeUpgradeId UNIQUEIDENTIFIER )

	CREATE NONCLUSTERED INDEX [IDX_reverseCandidates] ON #reverseCandidates
	(
		[ToUpgradeId] ASC
	)

	-- Store all possible entities that don't have an explicit 'inSolution' relationship
	-- but are marked with 'reverseImplicitInSolution' and are referenced in the reverse direction.
	INSERT INTO
		#reverseCandidates
	SELECT
		e.UpgradeId,
		e2.UpgradeId,
		e3.UpgradeId
	FROM
		Relationship r
	JOIN
		Entity e ON
			e.TenantId = @tenant AND
			e.Id = r.ToId
	LEFT JOIN
		Data_Bit db ON
			db.TenantId = @tenant AND
			db.EntityId = r.TypeId AND
			db.FieldId = @reverseImplicitInSolution
	JOIN
		Entity e2 ON
			e2.TenantId = @tenant AND
			e2.Id = r.FromId
	JOIN
		Entity e3 ON
			e3.TenantId = @tenant AND
			e3.Id = r.TypeId
	WHERE
		ISNULL( db.Data, 0 ) = 1 AND
		r.TenantId = @tenant AND
		NOT EXISTS (
				SELECT
					1
				FROM
					Relationship eis
				JOIN
					#dependents d ON
						eis.ToId = d.Id
				WHERE
					eis.TenantId = @tenant AND
					eis.FromId = e2.Id AND
					eis.TypeId = @inSolution
			)

	DECLARE @continue BIT = 1

	-- Continue looping until there are no more new candidates discovered.
	WHILE ( @continue > 0 )
	BEGIN
		SET @depth = @depth + 1

		INSERT INTO
			#candidateListParents
		OUTPUT
			INSERTED.UpgradeId
		INTO
			@latestCandidates
		SELECT
			a.UpgradeId,
			a.ParentUpgradeId,
			a.TypeUpgradeId,
			a.Info,
			@depth,
			0
		FROM
		(
			-- Get entities and instances in the forward direction that
			-- have not been discovered yet.
			SELECT	
				[UpgradeId] = f.ToUpgradeId,
				[ParentUpgradeId] = f.FromUpgradeId,
				f.TypeUpgradeId,
				[Info] = 'forward'
			FROM
				#forwardCandidates f
			JOIN
				@latestCandidates c ON
					c.UpgradeId = f.FromUpgradeId

			UNION

			-- Get entities and instances in the reverse direction that
			-- have not been discovered yet.
			SELECT
				[UpgradeId] = r.FromUpgradeId,
				[ParentUpgradeId] = r.ToUpgradeId,
				r.TypeUpgradeId,
				[Info] = 'reverse'
			FROM
				#reverseCandidates r
			JOIN
				@latestCandidates c ON
					c.UpgradeId = r.ToUpgradeId
		) a
		LEFT JOIN
			@latestCandidates c ON
				a.UpgradeId = c.UpgradeId
		WHERE
			c.UpgradeId IS NULL

		SET @continue = @@ROWCOUNT
	END

	INSERT INTO
		#candidateList
	SELECT DISTINCT
		UpgradeId, [Explicit]
	FROM
		#candidateListParents

	IF ( @outputResults = 1 )
	BEGIN
		SELECT
			c.Depth,
			[EntityUpgradeId] = e.UpgradeId,
			[EntityId] = e.Id,
			[EntityName] = en.Data,
			[EntityTypeId] = et.TypeId,
			[EntityTypeName] = etn.Data,
			[ParentEntityUpgradeId] = c.ParentUpgradeId,
			c.RelationshipTypeUpgradeId,
			[RelationshipTypeId] = re.Id,
			[RelationshipTypeName] = ren.Data,
			[Reason] = c.Info
		FROM
			#candidateListParents c
		JOIN
			Entity e ON
				e.UpgradeId = c.UpgradeId
		LEFT JOIN
			Data_NVarChar en ON
				e.TenantId = en.TenantId AND
				e.Id = en.EntityId AND
				en.FieldId = @name
		LEFT JOIN
			Relationship et ON
				e.TenantId = et.TenantId AND
				e.Id = et.FromId AND
				et.TypeId = @isOfType
		LEFT JOIN
			Data_NVarChar etn ON
				e.TenantId = etn.TenantId AND
				et.ToId = etn.EntityId AND
				etn.FieldId = @name
		LEFT JOIN
			Entity re ON
				e.TenantId = re.TenantId AND
				re.UpgradeId = c.RelationshipTypeUpgradeId
		LEFT JOIN
			Data_NVarChar ren ON
				e.TenantId = ren.TenantId AND
				re.Id = ren.EntityId AND
				ren.FieldId = @name
		WHERE
			e.TenantId = @tenant
		ORDER BY
			c.Depth,
			en.Data
	END
		
	-- Cleanup
	DROP TABLE #forwardCandidates
	DROP TABLE #reverseCandidates

	DROP TABLE #candidateListParents

	IF ( @selfContained = 1)
	BEGIN
		DROP TABLE #candidateList
		DROP TABLE #dependents
	END
END
