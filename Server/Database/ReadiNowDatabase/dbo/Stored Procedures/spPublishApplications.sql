-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE PROCEDURE [dbo].[spPublishApplications]
AS
BEGIN
	SET NOCOUNT ON

	-----
	-- Forward declare the well-known alias ids.
	-----
	DECLARE @isOfType				BIGINT = dbo.fnAliasNsId( 'isOfType', 'core', DEFAULT )
	DECLARE @inSolution				BIGINT = dbo.fnAliasNsId( 'inSolution', 'core', DEFAULT )
	DECLARE @solution				BIGINT = dbo.fnAliasNsId( 'solution', 'core', DEFAULT )
	DECLARE @applicationId			BIGINT = dbo.fnAliasNsId( 'applicationId', 'core', DEFAULT )
	DECLARE @packageForApplication	BIGINT = dbo.fnAliasNsId( 'packageForApplication', 'core', DEFAULT )
	DECLARE @appVerId				BIGINT = dbo.fnAliasNsId( 'appVerId', 'core', DEFAULT )

	DECLARE @solutionId				BIGINT
	
	-----
	-- Obtain the well-known alias values.
	-----
	DECLARE @packageUpgradeId UNIQUEIDENTIFIER

	-----
	-- Loop through the installed solutions.
	-----
	DECLARE Solutions CURSOR FAST_FORWARD FOR
		SELECT
			FromId
		FROM
			Relationship
		WHERE
			TenantId = 0 AND
			TypeId = @isOfType AND
			ToId = @solution
	    
	OPEN Solutions

	FETCH NEXT FROM Solutions into @solutionId
	WHILE @@fetch_status = 0
	BEGIN
		
		-----
		-- Determine the application package upgrade id for the current solution.
		-----
		SELECT
			@packageUpgradeId = pa.Data
		FROM
			UpgradeMap um
		INNER JOIN
			Data_Alias aa ON
				aa.TenantId = 0 AND
				aa.Namespace = um.Namespace AND
				aa.Data = um.Alias AND
				aa.EntityId = @solutionId AND
				aa.AliasMarkerId = 0
		INNER JOIN
			Data_Guid ag ON
				ag.TenantId = aa.TenantId AND
				um.UpgradeId = ag.Data AND
				ag.FieldId = @applicationId
		INNER JOIN
			Relationship ap ON
				ap.TenantId = ag.TenantId AND
				ap.ToId = ag.EntityId AND
				ap.TypeId = @packageForApplication
		INNER JOIN
			Data_Guid pa ON
				pa.TenantId = ap.TenantId AND
				pa.EntityId = ap.FromId AND
				pa.FieldId = @appVerId
		
		
		-----
		-- Publish Entities for the current solution.
		-----
		INSERT INTO
			AppEntity ( AppVerUid, EntityUid )
		SELECT
			PackageUpgradeId = @packageUpgradeId,
			EntityUpgradeId = e.UpgradeId
		FROM
			Entity e
		INNER JOIN
			Relationship r ON
				r.TenantId = e.TenantId AND
				r.FromId = e.Id AND
				r.TypeId = @inSolution AND
				r.ToId = @solutionId AND
				r.TenantId = 0
		LEFT JOIN
			AppEntity ae ON
				ae.AppVerUid = @packageUpgradeId AND
				ae.EntityUid = e.UpgradeId
		WHERE
			ae.EntityUid IS NULL

		-----
		-- Publish Relationships for the current solution.
		-----
		INSERT INTO
			AppRelationship ( AppVerUid, TypeUid, FromUid, ToUid )
		SELECT
			PackageUpgradeId = @packageUpgradeId,
			TypeId = typ.UpgradeId,
			FromId = fro.UpgradeId,
			ToId = toe.UpgradeId
		FROM
		(
			SELECT
				r.TenantId,
				r.TypeId,
				r.FromId,
				r.ToId
			FROM
				Relationship r
			JOIN (
				SELECT
					rr.TenantId,
					rr.FromId
				FROM
					Relationship rr
				WHERE
					rr.TenantId = 0 AND
					rr.TypeId = @inSolution AND
					rr.ToId = @solutionId
				) rr ON 
					rr.TenantId = r.TenantId AND
					rr.FromId = r.FromId

			UNION

			SELECT
				r.TenantId,
				r.TypeId,
				r.FromId,
				r.ToId
			FROM
				Relationship r
			WHERE
				r.TenantId = 0 AND
				r.TypeId = @inSolution AND
				r.ToId = @solutionId
		) a
		LEFT JOIN
			Entity typ ON
				typ.TenantId = a.TenantId AND
				typ.Id = a.TypeId
		LEFT JOIN
			Entity fro ON
				fro.TenantId = a.TenantId AND
				fro.Id = a.FromId
		LEFT JOIN
			Entity toe ON
				toe.TenantId = a.TenantId AND
				toe.Id = a.ToId
		LEFT JOIN
			AppRelationship ap ON
				ap.AppVerUid = @packageUpgradeId AND
				ap.TypeUid = typ.UpgradeId AND
				ap.FromUid = fro.UpgradeId AND
				ap.ToUid = toe.UpgradeId
		WHERE
			ap.AppVerUid IS NULL
		
		-----
		-- Construct the query that is to be executed against each data table for the current solution.
		-----
		DECLARE @sql NVARCHAR( MAX )
		SET @sql = '
		INSERT INTO
			App? ( AppVerUid, EntityUid, FieldUid, Data{{,ExtraFields}} )
		SELECT
			AppVerUid = ''' + CAST( @packageUpgradeId AS NVARCHAR( 50 ) ) + ''', EntityUid = u.UpgradeId, FieldUid = uf.UpgradeId, d.Data{{,d.ExtraFields}}
		FROM
			? d
		JOIN
			Data_Alias a ON a.TenantId = d.TenantId AND d.EntityId = a.EntityId
		JOIN
			UpgradeMap u ON a.Namespace = u.Namespace AND a.Data = u.Alias AND a.AliasMarkerId = 0
		JOIN
			Relationship r ON r.TenantId = a.TenantId AND a.EntityId = r.FromId AND r.TypeId = ' + CAST( @inSolution AS NVARCHAR( 10 ) ) + ' AND r.ToId = ' + CAST( @solutionId AS NVARCHAR( 10 ) ) + '
		JOIN
			Data_Alias f ON f.TenantId = d.TenantId AND d.FieldId = f.EntityId
		JOIN
			UpgradeMap uf ON f.Namespace = uf.Namespace AND f.Data = uf.Alias AND f.AliasMarkerId = 0
		LEFT JOIN
			App? dd ON
				dd.AppVerUid = ''' + CAST( @packageUpgradeId AS NVARCHAR( 50 ) ) + ''' AND
				dd.EntityUid = u.UpgradeId AND
				dd.FieldUid = uf.UpgradeId
		WHERE
			d.TenantId = 0 AND
			dd.AppVerUid IS NULL'
		
		-----
		-- Publish the data for each solution.
		-----
		EXEC spExecForDataTables @sql
		
		-----
		-- Move onto the next solution.
		-----
		FETCH NEXT FROM Solutions INTO @solutionId
	END

	-----
	-- Cleanup.
	-----
	CLOSE Solutions
	DEALLOCATE Solutions
END



