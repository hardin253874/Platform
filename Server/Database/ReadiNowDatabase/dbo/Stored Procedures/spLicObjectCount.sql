-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE PROCEDURE [dbo].[spLicObjectCount]
AS
BEGIN

	SET NOCOUNT ON

	DECLARE @idx BIGINT

	SELECT TOP 1 @idx = [Id] FROM [dbo].[Lic_Index] ORDER BY [Timestamp] DESC

	DECLARE @tenantId BIGINT

	DECLARE @isOfTypeGlobal BIGINT = [dbo].[fnAliasNsId]( 'isOfType', 'core', 0 )
	DECLARE @tenant BIGINT = [dbo].[fnAliasNsId]( 'tenant', 'core', 0 )

	DECLARE Tenants CURSOR FAST_FORWARD FOR
		SELECT
			FromId
		FROM
			[dbo].[Relationship] WITH ( NOLOCK )
		WHERE
			TenantId = 0
			AND TypeId = @isOfTypeGlobal
			AND ToId = @tenant
	    
	OPEN Tenants

	FETCH NEXT FROM Tenants into @tenantId
	WHILE @@fetch_status = 0
	BEGIN
		DECLARE @isOfType BIGINT = [dbo].[fnAliasNsId]( 'isOfType', 'core', @tenantId )
		DECLARE @inSolution BIGINT = [dbo].[fnAliasNsId]( 'inSolution', 'core', @tenantId )
		DECLARE @typeId BIGINT = [dbo].[fnAliasNsId]( 'type', 'core', @tenantId )
		DECLARE @customEditForm BIGINT = [dbo].[fnAliasNsId]( 'customEditForm', 'console', @tenantId )
		DECLARE @screen BIGINT = [dbo].[fnAliasNsId]( 'screen', 'console', @tenantId )
		DECLARE @report BIGINT = [dbo].[fnAliasNsId]( 'report', 'core', @tenantId )
		DECLARE @chart BIGINT = [dbo].[fnAliasNsId]( 'chart', 'core', @tenantId )
		DECLARE @reportTemplate BIGINT = [dbo].[fnAliasNsId]( 'reportTemplate', 'core', @tenantId )

		-- total object counts
		INSERT INTO
			[dbo].[Lic_Object_Count]
		SELECT
			@idx,
			@tenantId,
			COUNT ( 1 ),
			'object',
			0
		FROM
			[dbo].[Entity] e WITH ( NOLOCK )
		WHERE
			e.[TenantId] = @tenantId
		HAVING
			COUNT( 1 ) > 0

		-- In Solution Relationship (cross tenant)
		INSERT INTO
			[dbo].[Lic_Object_Count]
		SELECT
			@idx,
			@tenantId,
			COUNT ( 1 ),
			'object',
			r.[ToId]
		FROM
			[dbo].[Relationship] r WITH ( NOLOCK )
		WHERE
			r.TenantId = @tenantId AND
			r.TypeId = @inSolution
		GROUP BY
			r.[ToId]

		-- total type counts
		INSERT INTO
			[dbo].[Lic_Object_Count]
		SELECT
			@idx,
			@tenantId,
			COUNT( 1 ),
			'type',
			0
		FROM
			[dbo].[fnDerivedTypes]( @typeId, @tenantId ) f
		JOIN
			[dbo].[Relationship] r WITH ( NOLOCK ) ON
				r.TenantId = @tenantId
				AND r.TypeId = @isOfType
				AND r.ToId = f.Id
		HAVING
			COUNT( 1 ) > 0

		UNION ALL

		SELECT
			@idx,
			@tenantId,
			COUNT( 1 ),
			'type',
			s.ToId
		FROM
			[dbo].[fnDerivedTypes]( @typeId, @tenantId ) f
		JOIN
			[dbo].[Relationship] r WITH ( NOLOCK ) ON
				r.TenantId = @tenantId
				AND r.TypeId = @isOfType
				AND r.ToId = f.Id
		JOIN
			Relationship s WITH ( NOLOCK ) ON
				s.TenantId = r.TenantId
				AND s.TypeId = @inSolution
				AND s.FromId = r.FromId
		GROUP BY
			s.ToId
				
				

		-- total form counts
		INSERT INTO
			[dbo].[Lic_Object_Count]
		SELECT
			@idx,
			@tenantId,
			COUNT( 1 ),
			'customEditForm',
			0
		FROM
			[dbo].[fnDerivedTypes]( @customEditForm, @tenantId ) f
		JOIN
			[dbo].[Relationship] r WITH ( NOLOCK ) ON
				r.TenantId = @tenantId
				AND r.TypeId = @isOfType
				AND r.ToId = f.Id
		HAVING
			COUNT( 1 ) > 0

		UNION ALL

		SELECT
			@idx,
			@tenantId,
			COUNT( 1 ),
			'customEditForm',
			s.ToId
		FROM
			[dbo].[fnDerivedTypes]( @customEditForm, @tenantId ) f
		JOIN
			[dbo].[Relationship] r WITH ( NOLOCK ) ON
				r.TenantId = @tenantId
				AND r.TypeId = @isOfType
				AND r.ToId = f.Id
		JOIN
			[dbo].[Relationship] s WITH ( NOLOCK ) ON
				s.TenantId = r.TenantId
				AND s.TypeId = @inSolution
				AND s.FromId = r.FromId
		GROUP BY
			s.ToId

		-- total screen counts
		INSERT INTO
			[dbo].[Lic_Object_Count]
		SELECT
			@idx,
			@tenantId,
			COUNT( 1 ),
			'screen',
			0
		FROM
			[dbo].[fnDerivedTypes]( @screen, @tenantId ) f
		JOIN
			[dbo].[Relationship] r WITH ( NOLOCK ) ON
				r.TenantId = @tenantId
				AND r.TypeId = @isOfType
				AND r.ToId = f.Id
		HAVING
			COUNT( 1 ) > 0

		UNION ALL

		SELECT
			@idx,
			@tenantId,
			COUNT( 1 ),
			'screen',
			s.ToId
		FROM
			[dbo].[fnDerivedTypes]( @screen, @tenantId ) f
		JOIN
			[dbo].[Relationship] r WITH ( NOLOCK ) ON
				r.TenantId = @tenantId
				AND r.TypeId = @isOfType
				AND r.ToId = f.Id
		JOIN
			[dbo].[Relationship] s WITH ( NOLOCK ) ON
				s.TenantId = r.TenantId
				AND s.TypeId = @inSolution
				AND s.FromId = r.FromId
		GROUP BY
			s.ToId

		-- total report counts
		INSERT INTO
			[dbo].[Lic_Object_Count]
		SELECT
			@idx,
			@tenantId,
			COUNT( 1 ),
			'report',
			0
		FROM
			[dbo].[fnDerivedTypes]( @report, @tenantId ) f
		JOIN
			[dbo].[Relationship] r WITH ( NOLOCK ) ON
				r.TenantId = @tenantId
				AND r.TypeId = @isOfType
				AND r.ToId = f.Id
		HAVING
			COUNT( 1 ) > 0

		UNION ALL

		SELECT
			@idx,
			@tenantId,
			COUNT( 1 ),
			'report',
			s.ToId
		FROM
			[dbo].[fnDerivedTypes]( @report, @tenantId ) f
		JOIN
			[dbo].[Relationship] r WITH ( NOLOCK ) ON
				r.TenantId = @tenantId
				AND r.TypeId = @isOfType
				AND r.ToId = f.Id
		JOIN
			[dbo].[Relationship] s WITH ( NOLOCK ) ON
				s.TenantId = r.TenantId
				AND s.TypeId = @inSolution
				AND s.FromId = r.FromId
		GROUP BY
			s.ToId

		-- total chart counts
		INSERT INTO
			[dbo].[Lic_Object_Count]
		SELECT
			@idx,
			@tenantId,
			COUNT( 1 ),
			'chart',
			0
		FROM
			[dbo].[fnDerivedTypes]( @chart, @tenantId ) f
		JOIN
			[dbo].[Relationship] r ON
				r.TenantId = @tenantId
				AND r.TypeId = @isOfType
				AND r.ToId = f.Id
		HAVING
			COUNT( 1 ) > 0

		UNION ALL

		SELECT
			@idx,
			@tenantId,
			COUNT( 1 ),
			'chart',
			s.ToId
		FROM
			[dbo].[fnDerivedTypes]( @chart, @tenantId ) f
		JOIN
			[dbo].[Relationship] r WITH ( NOLOCK ) ON
				r.TenantId = @tenantId
				AND r.TypeId = @isOfType
				AND r.ToId = f.Id
		JOIN
			[dbo].[Relationship] s WITH ( NOLOCK ) ON
				s.TenantId = r.TenantId
				AND s.TypeId = @inSolution
				AND s.FromId = r.FromId
		GROUP BY
			s.ToId

		-- total report template counts
		INSERT INTO
			[dbo].[Lic_Object_Count]
		SELECT
			@idx,
			@tenantId,
			COUNT( 1 ),
			'reportTemplate',
			0
		FROM
			[dbo].[fnDerivedTypes]( @reportTemplate, @tenantId ) f
		JOIN
			[dbo].[Relationship] r WITH ( NOLOCK ) ON
				r.TenantId = @tenantId
				AND r.TypeId = @isOfType
				AND r.ToId = f.Id
		HAVING
			COUNT( 1 ) > 0

		UNION ALL

		SELECT
			@idx,
			@tenantId,
			COUNT( 1 ),
			'reportTemplate',
			s.ToId
		FROM
			[dbo].[fnDerivedTypes]( @reportTemplate, @tenantId ) f
		JOIN
			[dbo].[Relationship] r WITH ( NOLOCK ) ON
				r.TenantId = @tenantId
				AND r.TypeId = @isOfType
				AND r.ToId = f.Id
		JOIN
			[dbo].[Relationship] s WITH ( NOLOCK ) ON
				s.TenantId = r.TenantId
				AND s.TypeId = @inSolution
				AND s.FromId = r.FromId
		GROUP BY
			s.ToId

		FETCH NEXT FROM Tenants INTO @tenantId
	END

	CLOSE Tenants
	DEALLOCATE Tenants

END