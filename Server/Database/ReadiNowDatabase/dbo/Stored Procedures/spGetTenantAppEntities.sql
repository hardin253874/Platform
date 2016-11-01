-- Copyright 2011-2016 Global Software Innovation Pty Ltd


CREATE PROCEDURE [dbo].[spGetTenantAppEntities] 
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
	
	CREATE TABLE #latestCandidates ( UpgradeId UNIQUEIDENTIFIER PRIMARY KEY )

	IF ( @selfContained = 1)
	BEGIN
		CREATE TABLE #candidateList ( UpgradeId UNIQUEIDENTIFIER PRIMARY KEY, [Explicit] BIT )
		CREATE TABLE #dependents ( Id BIGINT PRIMARY KEY )
	END

	DECLARE @dependentApplication BIGINT = dbo.fnAliasNsId( 'dependentApplication', 'core', @tenant )
	DECLARE @dependencyApplication BIGINT = dbo.fnAliasNsId( 'dependencyApplication', 'core', @tenant )
	DECLARE @implicitInSolution BIGINT = dbo.fnAliasNsId( 'implicitInSolution', 'core', @tenant )
	DECLARE @reverseImplicitInSolution BIGINT = dbo.fnAliasNsId( 'reverseImplicitInSolution', 'core', @tenant )
	DECLARE @inSolution BIGINT = dbo.fnAliasNsId( 'inSolution', 'core', @tenant )
	DECLARE @indirectInSolution BIGINT = dbo.fnAliasNsId( 'indirectInSolution', 'core', @tenant )

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
		#candidateList
	OUTPUT
		INSERTED.UpgradeId
	INTO
		#latestCandidates
	-- Select all the entities that have an explicit 'inSolution' forward relationship to the solution requested.
	SELECT DISTINCT
		e.UpgradeId, 1
	FROM
		Entity e
	JOIN
		Relationship r ON
			r.TenantId = e.TenantId AND
			r.FromId = e.Id
	CROSS APPLY
		dbo.tblFnAliasNsId( 'inSolution', 'core', @tenant ) ins
	WHERE
		e.TenantId = @tenant AND
		r.TypeId = ins.EntityId AND
		r.ToId = @solutionId

	UNION

	-- Add the solution entity itself.
	SELECT
		UpgradeId, 1
	FROM
		Entity
	WHERE
		Id = @solutionId
	AND
		TenantId = @tenant

	DECLARE @continue BIT = 1

	-- Continue looping until there are no more new candidates discovered.
	WHILE ( @continue > 0 )
	BEGIN
		INSERT INTO
			#candidateList
		OUTPUT
			INSERTED.UpgradeId
		INTO
			#latestCandidates
		SELECT DISTINCT
			e2.UpgradeId, 0
		FROM
			#latestCandidates c
		JOIN
			Entity e ON c.UpgradeId = e.UpgradeId AND e.TenantId = @tenant
		JOIN
			Relationship r ON
				r.FromId = e.Id AND
				r.TenantId = e.TenantId
		JOIN
			Data_Bit db ON
				r.TypeId = db.EntityId AND
				r.TenantId = db.TenantId AND
				db.FieldId = @implicitInSolution AND
				db.TenantId = @tenant
		JOIN
			Entity e2 ON
				r.ToId = e2.Id AND
				e2.TenantId = r.TenantId
		LEFT JOIN
			#latestCandidates cc ON
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
			r.TenantId = @tenant AND (
				ris.ToId IS NULL OR 
				ris.ToId = @solutionId
			) AND
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

		SET @continue = @@ROWCOUNT

		INSERT INTO
			#candidateList
		OUTPUT
			INSERTED.UpgradeId
		INTO
			#latestCandidates
		SELECT DISTINCT
			e2.UpgradeId, 0
		FROM
			#latestCandidates c
		JOIN
			Entity e ON c.UpgradeId = e.UpgradeId AND e.TenantId = @tenant
		JOIN
			Relationship r ON
				r.ToId = e.Id AND
				r.TenantId = e.TenantId
		JOIN
			Data_Bit db ON
				r.TypeId = db.EntityId AND
				r.TenantId = db.TenantId AND
				db.FieldId = @reverseImplicitInSolution AND
				db.TenantId = @tenant
		JOIN
			Entity e2 ON
				r.FromId = e2.Id AND
				e2.TenantId = r.TenantId
		LEFT JOIN
			#latestCandidates cc ON
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
			r.TenantId = @tenant AND (
				ris.ToId IS NULL OR 
				ris.ToId = @solutionId
			) AND
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

		IF ( @continue <= 0 )
			SET @continue = @@ROWCOUNT
	END

	IF ( @outputResults = 1 )
	BEGIN
		SELECT
			c.UpgradeId, [Explicit]
		FROM
			#candidateList c
	END
		
	-- Cleanup
	DROP TABLE #latestCandidates

	IF ( @selfContained = 1)
	BEGIN
		DROP TABLE #candidateList
		DROP TABLE #dependents
	END
END