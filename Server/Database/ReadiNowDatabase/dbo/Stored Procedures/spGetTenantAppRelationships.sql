-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE PROCEDURE [dbo].[spGetTenantAppRelationships]
	@solutionId BIGINT,
	@tenant BIGINT,
	@selfContained BIT = 0
WITH RECOMPILE
AS
BEGIN
	-- Note* The #candidateList table is created externally on the connection prior to this call
	-- and is cleaned up externally unless @selfContained is set to 1.

	SET NOCOUNT ON
	
	DECLARE @solutionUpgradeId UNIQUEIDENTIFIER
	DECLARE @app BIGINT
	DECLARE @packageIds TABLE ( PackageId UNIQUEIDENTIFIER PRIMARY KEY )
	DECLARE @dependentCount INT

	DECLARE @inSolution BIGINT = dbo.fnAliasNsId( 'inSolution', 'core', @tenant )
	DECLARE @indirectInSolution BIGINT = dbo.fnAliasNsId( 'indirectInSolution', 'core', @tenant )
	DECLARE @excludeFromPublish BIGINT = dbo.fnAliasNsId( 'excludeFromPublish', 'core', @tenant )
	DECLARE @cardinality BIGINT = dbo.fnAliasNsId( 'cardinality', 'core', @tenant )
	DECLARE @applicationId BIGINT = dbo.fnAliasNsId( 'applicationId', 'core', 0 )
	DECLARE @packageForApplication BIGINT = dbo.fnAliasNsId( 'packageForApplication', 'core', 0 )
	DECLARE @appVerId BIGINT = dbo.fnAliasNsId( 'appVerId', 'core', 0 )

	IF ( @selfContained = 1)
	BEGIN
		CREATE TABLE #candidateList ( UpgradeId UNIQUEIDENTIFIER PRIMARY KEY, [Explicit] BIT )
		CREATE TABLE #dependents ( Id BIGINT PRIMARY KEY )

		EXEC spGetTenantAppEntities @solutionId, @tenant, 0
	END

	SELECT @dependentCount = COUNT( * ) FROM #dependents

	SELECT
		TypeId = etype.UpgradeId,
		FromId = efrom.UpgradeId,
		ToId = eto.UpgradeId,
		Cardinality = c.ToId
	FROM
	(
		SELECT
			r.TenantId,
			r.TypeId,
			r.FromId,
			r.ToId
		FROM
			Relationship r
		JOIN
			Entity e ON
				e.TenantId = @tenant AND
				e.Id = r.FromId
		JOIN
			#candidateList c ON
				c.UpgradeId = e.UpgradeId
		WHERE
			r.TenantId = @tenant

		UNION

		SELECT
			r.TenantId,
			r.TypeId,
			r.FromId,
			r.ToId
		FROM
			Relationship r
		JOIN
			Entity e ON
				e.TenantId = @tenant AND
				e.Id = r.TypeId
		JOIN
			#candidateList c ON
				c.UpgradeId = e.UpgradeId
		WHERE
			r.TenantId = @tenant

		UNION

		SELECT
			r.TenantId,
			r.TypeId,
			r.FromId,
			r.ToId
		FROM
			Relationship r
		JOIN
			Entity e ON
				e.TenantId = @tenant AND
				e.Id = r.ToId
		JOIN
			#candidateList c ON c.UpgradeId = e.UpgradeId
		WHERE
			r.TenantId = @tenant

		UNION

		SELECT
			@tenant,
			@indirectInSolution,
			e.Id,
			@solutionId
		FROM
			#candidateList c
		JOIN
			Entity e ON
				e.TenantId = @tenant AND
				e.UpgradeId = c.UpgradeId
		LEFT JOIN
			Relationship r ON
				r.TenantId = @tenant AND
				r.TypeId = @indirectInSolution AND
				r.FromId = e.Id AND
				r.ToId = @solutionId
		WHERE
			r.ToId IS NULL AND
			c.[Explicit] = 0
	) r
	JOIN
		Entity efrom ON
			efrom.TenantId = @tenant AND
			efrom.Id = r.FromId
	JOIN
		Entity eto ON
			eto.TenantId = @tenant AND
			eto.Id = r.ToId
	JOIN
		Entity etype ON
			etype.TenantId = @tenant AND
			etype.Id = r.TypeId
	LEFT JOIN
		Relationship c ON
			c.TenantId = @tenant AND
			c.FromId = r.TypeId AND
			c.TypeId = @cardinality
	LEFT JOIN
		Data_Bit exc ON
			exc.TenantId = @tenant AND
			exc.EntityId = r.TypeId AND
			exc.FieldId = @excludeFromPublish
	WHERE
		ISNULL( exc.Data, 0 ) = 0 AND
		EXISTS (
			SELECT
				1
			WHERE
				@dependentCount <= 0

			UNION ALL

			SELECT
				1
			FROM
				#candidateList
			WHERE
				UpgradeId = etype.UpgradeId

			UNION ALL

			SELECT
				1
			FROM
				Entity e1
			LEFT JOIN
				Relationship r1 ON
					r1.TenantId = e1.TenantId AND
					r1.FromId = e1.Id AND
					r1.TypeId = @inSolution
			LEFT JOIN
				#dependents d ON
					r1.ToId = d.Id
			WHERE
				e1.UpgradeId = etype.UpgradeId AND
				e1.TenantId = @tenant AND
				d.Id IS NULL
		) AND
		EXISTS (
			SELECT
				1
			WHERE
				@dependentCount <= 0

			UNION ALL

			SELECT
				1
			FROM
				#candidateList
			WHERE
				UpgradeId = efrom.UpgradeId

			UNION ALL

			SELECT
				1
			FROM
				Entity e1
			LEFT JOIN
				Relationship r1 ON
					r1.TenantId = e1.TenantId AND
					r1.FromId = e1.Id AND
					r1.TypeId = @inSolution
			LEFT JOIN
				#dependents d ON
					r1.ToId = d.Id
			WHERE
				e1.UpgradeId = efrom.UpgradeId AND
				e1.TenantId = @tenant AND
				d.Id IS NULL
		) AND
		EXISTS (
			SELECT
				1
			WHERE
				@dependentCount <= 0

			UNION ALL

			SELECT
				1
			FROM
				#candidateList
			WHERE
				UpgradeId = eto.UpgradeId

			UNION ALL

			SELECT
				1
			FROM
				Entity e1
			LEFT JOIN
				Relationship r1 ON
					r1.TenantId = e1.TenantId AND
					r1.FromId = e1.Id AND
					r1.TypeId = @inSolution
			LEFT JOIN
				#dependents d ON
					r1.ToId = d.Id
			WHERE
				e1.UpgradeId = eto.UpgradeId AND
				e1.TenantId = @tenant AND
				d.Id IS NULL
		)

	SELECT
		@solutionUpgradeId = UpgradeId
	FROM
		Entity
	WHERE
		TenantId = @tenant AND
	Id = @solutionId

	SELECT
		@app = EntityId
	FROM
		Data_Guid
	WHERE
		TenantId = 0 AND
		FieldId = @applicationId AND
		Data = @solutionUpgradeId

	INSERT INTO
		@packageIds
	SELECT DISTINCT
		g.Data
	FROM
		Relationship r
	JOIN
		Data_Guid g ON
			r.TenantId = g.TenantId AND
			r.FromId = g.EntityId AND
			g.FieldId = @appVerId
	WHERE
		r.TenantId = 0 AND
		r.ToId = @app AND
		r.TypeId = @packageForApplication

	SELECT DISTINCT
		TypeId = TypeUid,
		FromId = FromUid,
		ToId = ToUid,
		Cardinality = c.ToId
	FROM
		AppDeploy_Relationship r
	JOIN
		@packageIds p ON
			r.AppVerUid = p.PackageId
	LEFT JOIN
		Entity e ON
			r.TenantId = e.TenantId AND
			r.TypeUid = e.UpgradeId
	LEFT JOIN
		Relationship c ON
			c.TenantId = e.TenantId AND
			c.FromId = e.Id AND
			c.TypeId = @cardinality
	WHERE
	r.TenantId = @tenant AND EXISTS (
		SELECT
			1
		WHERE
			@dependentCount <= 0

		UNION ALL

		SELECT
			1
		FROM
			#candidateList
		WHERE
			UpgradeId = r.TypeUid

		UNION ALL

		SELECT
			1
		WHERE NOT EXISTS (
			SELECT
				Id
			FROM
				Entity
			WHERE
				TenantId = @tenant AND
				UpgradeId = r.TypeUid
		)

		UNION ALL

		SELECT
			1
		FROM
			Entity e1
		LEFT JOIN
			Relationship r1 ON
				r1.TenantId = e1.TenantId AND
				r1.FromId = e1.Id AND
				r1.TypeId = @inSolution
		LEFT JOIN
			#dependents d ON
				r1.ToId = d.Id
		WHERE
			e1.UpgradeId = r.TypeUid AND
			e1.TenantId = @tenant AND
			d.Id IS NULL
	) AND
	EXISTS (
		SELECT
			1
		WHERE
			@dependentCount <= 0

		UNION ALL

		SELECT
			1
		FROM
			#candidateList
		WHERE
			UpgradeId = r.FromUid

		UNION ALL

		SELECT
			1
		WHERE NOT EXISTS (
			SELECT
				Id
			FROM
				Entity
			WHERE
				TenantId = @tenant AND
				UpgradeId = r.FromUid
		)

		UNION ALL

		SELECT
			1
		FROM
			Entity e1
		LEFT JOIN
			Relationship r1 ON
				r1.TenantId = e1.TenantId AND
				r1.FromId = e1.Id AND
				r1.TypeId = @inSolution
		LEFT JOIN
			#dependents d ON
				r1.ToId = d.Id
		WHERE
			e1.UpgradeId = r.FromUid AND
			e1.TenantId = @tenant AND
			d.Id IS NULL
	) AND
	EXISTS (
		SELECT
			1
		WHERE
			@dependentCount <= 0

		UNION ALL

		SELECT
			1
		FROM
			#candidateList
		WHERE
			UpgradeId = r.ToUid

		UNION ALL

		SELECT
			1
		WHERE NOT EXISTS (
			SELECT
				Id
			FROM
				Entity
			WHERE
				TenantId = @tenant AND
				UpgradeId = r.ToUid
		)

		UNION ALL

		SELECT
			1
		FROM
			Entity e1
		LEFT JOIN
			Relationship r1 ON
				r1.TenantId = e1.TenantId AND
				r1.FromId = e1.Id AND
				r1.TypeId = @inSolution
		LEFT JOIN
			#dependents d ON
				r1.ToId = d.Id
		WHERE
			e1.UpgradeId = r.ToUid AND
			e1.TenantId = @tenant AND
			d.Id IS NULL
	)

	IF ( @selfContained = 1)
	BEGIN
		DROP TABLE #candidateList
		DROP TABLE #dependents
	END
END