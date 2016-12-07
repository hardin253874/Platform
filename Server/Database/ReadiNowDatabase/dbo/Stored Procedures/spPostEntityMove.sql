-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE PROCEDURE [dbo].[spPostEntityMove]
	@entityIds dbo.[UniqueIdListType] READONLY,
	@tenantId BIGINT,
	@solutionId BIGINT
AS
BEGIN
	SET NOCOUNT ON

	DECLARE @latestCandidates TABLE ( UpgradeId UNIQUEIDENTIFIER PRIMARY KEY )
	DECLARE @candidateList TABLE ( UpgradeId UNIQUEIDENTIFIER PRIMARY KEY )
	DECLARE @dependents TABLE ( Id BIGINT PRIMARY KEY )

	DECLARE @dependentApplication BIGINT = dbo.fnAliasNsId( 'dependentApplication', 'core', @tenantId )
	DECLARE @dependencyApplication BIGINT = dbo.fnAliasNsId( 'dependencyApplication', 'core', @tenantId )
	DECLARE @implicitInSolution BIGINT = dbo.fnAliasNsId( 'implicitInSolution', 'core', @tenantId )
	DECLARE @reverseImplicitInSolution BIGINT = dbo.fnAliasNsId( 'reverseImplicitInSolution', 'core', @tenantId )
	DECLARE @inSolution BIGINT = dbo.fnAliasNsId( 'inSolution', 'core', @tenantId )
	DECLARE @indirectInSolution BIGINT = dbo.fnAliasNsId( 'indirectInSolution', 'core', @tenantId )

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
			d.TenantId = @tenantId
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
			d.TenantId = @tenantId
			AND d.TypeId = @dependencyApplication
	)
	INSERT INTO
		@dependents
	SELECT
		DISTINCT Id
	FROM
		Dependents

	----------------------------------

	INSERT INTO
		@candidateList
	OUTPUT
		INSERTED.UpgradeId
	INTO
		@latestCandidates
	-- Select all the entities that have an explicit 'inSolution' forward relationship to the solution requested.
	SELECT DISTINCT
		e.UpgradeId
	FROM
		Entity e
	JOIN
		@entityIds i ON
			e.Id = i.Id
	WHERE
		e.TenantId = @tenantId

	DECLARE @continue BIT = 1

	-- Continue looping until there are no more new candidates discovered.
	WHILE ( @continue > 0 )
	BEGIN
		INSERT INTO
			@candidateList
		OUTPUT
			INSERTED.UpgradeId
		INTO
			@latestCandidates
		SELECT DISTINCT
			e2.UpgradeId
		FROM
			@latestCandidates c
		JOIN
			Entity e ON c.UpgradeId = e.UpgradeId AND e.TenantId = @tenantId
		JOIN
			Relationship r ON
				r.FromId = e.Id AND
				r.TenantId = e.TenantId
		JOIN
			Data_Bit db ON
				r.TypeId = db.EntityId AND
				r.TenantId = db.TenantId AND
				db.FieldId = @implicitInSolution AND
				db.TenantId = @tenantId
		JOIN
			Entity e2 ON
				r.ToId = e2.Id AND
				e2.TenantId = r.TenantId
		LEFT JOIN
			@latestCandidates cc ON
				e2.UpgradeId = cc.UpgradeId
		LEFT JOIN
			Relationship ris ON
				ris.TenantId = e2.TenantId AND (
					ris.TypeId = @inSolution OR
					ris.TypeId = @indirectInSolution
				) AND
				ris.FromId = e2.Id
		WHERE
			cc.UpgradeId IS NULL AND
			db.Data = 1 AND
			r.TenantId = @tenantId AND (
				ris.ToId IS NULL OR 
				ris.ToId = @solutionId
			) AND
			NOT EXISTS (
				SELECT
					1
				FROM
					Relationship eis
				JOIN
					@dependents d ON
						eis.ToId = d.Id
				WHERE
					eis.TenantId = @tenantId AND
					eis.FromId = e2.Id AND
					eis.TypeId = @inSolution
			)

		SET @continue = @@ROWCOUNT

		INSERT INTO
			@candidateList
		OUTPUT
			INSERTED.UpgradeId
		INTO
			@latestCandidates
		SELECT DISTINCT
			e2.UpgradeId
		FROM
			@latestCandidates c
		JOIN
			Entity e ON c.UpgradeId = e.UpgradeId AND e.TenantId = @tenantId
		JOIN
			Relationship r ON
				r.ToId = e.Id AND
				r.TenantId = e.TenantId
		JOIN
			Data_Bit db ON
				r.TypeId = db.EntityId AND
				r.TenantId = db.TenantId AND
				db.FieldId = @reverseImplicitInSolution AND
				db.TenantId = @tenantId
		JOIN
			Entity e2 ON
				r.FromId = e2.Id AND
				e2.TenantId = r.TenantId
		LEFT JOIN
			@latestCandidates cc ON
				e2.UpgradeId = cc.UpgradeId
		LEFT JOIN
			Relationship ris ON
				ris.TenantId = e2.TenantId AND (
					ris.TypeId = @inSolution OR
					ris.TypeId = @indirectInSolution
				) AND
				ris.FromId = e2.Id
		WHERE
			cc.UpgradeId IS NULL AND
			db.Data = 1 AND
			r.TenantId = @tenantId AND (
				ris.ToId IS NULL OR 
				ris.ToId = @solutionId
			) AND
			NOT EXISTS (
				SELECT
					1
				FROM
					Relationship eis
				JOIN
					@dependents d ON
						eis.ToId = d.Id
				WHERE
					eis.TenantId = @tenantId AND
					eis.FromId = e2.Id AND
					eis.TypeId = @inSolution
			)

		IF ( @continue <= 0 )
			SET @continue = @@ROWCOUNT
	END

	DELETE
		Relationship
	FROM
		Relationship r
	JOIN
		Entity e ON
			r.TenantId = e.TenantId
			AND r.FromId = e.Id
	JOIN
		@candidateList c ON
			e.UpgradeId = c.UpgradeId
	WHERE
		e.TenantId = @tenantId
		AND r.TypeId = @indirectInSolution
		AND r.ToId = @solutionId
END