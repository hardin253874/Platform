-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE PROCEDURE [dbo].[spInSolution]
	@action NVARCHAR( 50 ) = 'view',
	@tenant NVARCHAR( MAX ) = 'EDC',
	@solution NVARCHAR( MAX ) = 'Test Solution',
	@start DATETIME, -- local
	@end DATETIME    -- local

AS

BEGIN

	DECLARE @tz AS NVARCHAR(100) = 'AUS Eastern Standard Time'
	DECLARE @startUtc AS DATETIME = dbo.fnConvertToUtc(@start, @tz)
	DECLARE @endUtc AS DATETIME = dbo.fnConvertToUtc(@end, @tz)

	DECLARE @tenantId AS BIGINT = ( SELECT Id FROM _vTenant WHERE name = @tenant )
	DECLARE @solutionId AS BIGINT = ( SELECT Id FROM _vSolution WHERE TenantId = @tenantId AND name = @solution )
	DECLARE @inSolution AS BIGINT = dbo.fnAliasNsId( 'inSolution', 'core', @tenantId )
	DECLARE @createdDate AS BIGINT = dbo.fnAliasNsId( 'createdDate', 'core', @tenantId )
	DECLARE @isOfType AS BIGINT = dbo.fnAliasNsId( 'isOfType', 'core', @tenantId )

	DECLARE @excludedTypes TABLE ( TypeId BIGINT PRIMARY KEY )

	-- Exclude 'openIdLogin'
	INSERT INTO
		@excludedTypes
	SELECT
		dbo.fnAliasNsId( 'openIdLogin', 'core', @tenantId )

	IF ( @action = 'view' )
	BEGIN
		-- Preview
		SELECT
			dt.EntityId,
			r.name Name,
			n.Data [Type],
			sn.Data [Current Solution],
			t.Time 'Created'
		FROM
			Data_DateTime dt
		JOIN
			_vResource r ON
				r.TenantId = dt.TenantId AND
				r.Id = dt.EntityId
		LEFT JOIN
			Relationship inSol ON
				inSol.TenantId = dt.TenantId AND
				inSol.FromId = dt.EntityId AND
				inSol.TypeId = @inSolution
		OUTER APPLY
			dbo.tblFnName( r.TypeId, @tenantId ) n
		OUTER APPLY
			dbo.tblFnName( inSol.ToId, @tenantId ) sn
		CROSS APPLY
			dbo.tblFnConvertToLocalTime( dt.Data, @tz ) t
		LEFT JOIN
			@excludedTypes et ON
				r.TypeId = et.TypeId
		WHERE
			dt.TenantId = @tenantId AND
			dt.FieldId = @createdDate AND
			dt.Data BETWEEN @startUtc AND @endUtc AND
			et.TypeId IS NULL
		ORDER BY
			n.Data,
			Name
	END

	IF ( @action = 'previewadd' )
	BEGIN
		-- Preview
		SELECT
			dt.EntityId,
			r.name Name,
			n.Data [Type],
			sn.Data [Current Solution]
		FROM
			Data_DateTime dt
		JOIN
			_vResource r ON
				r.TenantId = dt.TenantId AND
				r.Id = dt.EntityId
		LEFT JOIN
			Relationship inSol ON
				inSol.TenantId = dt.TenantId AND
				inSol.FromId = dt.EntityId AND
				inSol.TypeId = @inSolution
		OUTER APPLY
			dbo.tblFnName( r.TypeId, @tenantId ) n
		OUTER APPLY
			dbo.tblFnName( inSol.ToId, @tenantId ) sn
		LEFT JOIN
			@excludedTypes et ON
				r.TypeId = et.TypeId
		WHERE
			dt.TenantId = @tenantId AND
			dt.FieldId = @createdDate AND
			dt.Data BETWEEN @startUtc AND @endUtc AND
			inSol.ToId IS NULL AND
			et.TypeId IS NULL
		ORDER BY
			n.Data,
			Name
	END

	IF ( @action = 'previewclear' )
	BEGIN
		-- Preview
		SELECT
			dt.EntityId,
			r.name Name,
			n.Data [Type],
			sn.Data [Current Solution]
		FROM
			Data_DateTime dt
		JOIN
			_vResource r ON
				r.TenantId = dt.TenantId AND
				r.Id = dt.EntityId
		JOIN
			Relationship inSol ON
				inSol.TenantId = dt.TenantId AND
				inSol.FromId = dt.EntityId AND
				inSol.TypeId = @inSolution AND
				inSol.ToId = @solutionId
		OUTER APPLY
			dbo.tblFnName( r.TypeId, @tenantId ) n
		OUTER APPLY
			dbo.tblFnName( inSol.ToId, @tenantId ) sn
		LEFT JOIN
			@excludedTypes et ON
				r.TypeId = et.TypeId
		WHERE
			dt.TenantId = @tenantId AND
			dt.FieldId = @createdDate AND
			dt.Data BETWEEN @startUtc AND @endUtc AND
			et.TypeId IS NULL
		ORDER BY
			n.Data,
			Name
	END

	IF ( @action = 'add' )
	BEGIN
	BEGIN TRANSACTION
		-- Assign
		INSERT INTO
			Relationship ( TenantId, TypeId, FromId, ToId )
		SELECT
			@tenantId,
			@inSolution,
			dt.EntityId,
			@solutionId
		FROM
			Data_DateTime dt
		LEFT JOIN
			Relationship t ON
				dt.TenantId = t.TenantId AND
				dt.EntityId = t.FromId AND
				t.TypeId = @isOfType
		LEFT JOIN
			Relationship inSol ON
				dt.EntityId = inSol.FromId AND
				inSol.TypeId = @inSolution AND
				inSol.TenantId = @tenantId
		LEFT JOIN
			@excludedTypes et ON
				t.ToId = et.TypeId
		WHERE
			dt.TenantId = @tenantId AND
			dt.FieldId = @createdDate AND
			dt.Data BETWEEN @startUtc AND @endUtc AND
			inSol.ToId IS NULL AND
			et.TypeId IS NULL
	ROLLBACK TRANSACTION
	END

	IF ( @action = 'clear' )
	BEGIN
		-- Assign
		DELETE FROM
			Relationship
		WHERE
			TenantId = @tenantId AND
			TypeId = @inSolution AND
			ToId = @solutionId AND
			FromId IN (
				SELECT
					dt.EntityId
				FROM
					Data_DateTime dt
				LEFT JOIN
					Relationship t ON
					dt.TenantId = t.TenantId AND
					dt.EntityId = t.FromId AND
					t.TypeId = @isOfType
				LEFT JOIN
					@excludedTypes et ON
						t.ToId = et.TypeId
				WHERE
					dt.TenantId = @tenantId AND
					dt.FieldId = @createdDate AND
					dt.Data BETWEEN @startUtc AND @endUtc AND
					et.TypeId IS NULL
			)
	END

END
