-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE PROCEDURE [dbo].[spLicRecord]
AS
BEGIN

	SET NOCOUNT ON

	DECLARE @idx BIGINT
	DECLARE @entityTable BIGINT = 0
	DECLARE @relationshipTable BIGINT = 1
	DECLARE @data_Alias BIGINT = 2
	DECLARE @data_Bit BIGINT = 3
	DECLARE @data_DateTime BIGINT = 4
	DECLARE @data_Decimal BIGINT = 5
	DECLARE @data_Guid BIGINT = 6
	DECLARE @data_Int BIGINT = 7
	DECLARE @data_NVarChar BIGINT = 8
	DECLARE @data_Xml BIGINT = 9

	SELECT TOP 1
		@idx = [Id]
	FROM
		[dbo].[Lic_Index]
	ORDER BY
		[Timestamp] DESC

	DECLARE @tenantId BIGINT

	DECLARE @isOfTypeGlobal BIGINT = [dbo].[fnAliasNsId]( 'isOfType', 'core', 0 )
	DECLARE @tenant BIGINT = [dbo].[fnAliasNsId]( 'tenant', 'core', 0 )

	DECLARE Tenants CURSOR FAST_FORWARD FOR
		SELECT
			FromId
		FROM
			[dbo].[Relationship]
		WHERE
			TenantId = 0
			AND TypeId = @isOfTypeGlobal
			AND ToId = @tenant
	    
	OPEN Tenants

	FETCH NEXT FROM Tenants into @tenantId
	WHILE @@fetch_status = 0
	BEGIN

		DECLARE @inSolution BIGINT = [dbo].[fnAliasNsId]( 'inSolution', 'core', @tenantId )

		INSERT INTO [dbo].[Lic_Record]
		SELECT
			@idx,
			@tenantId,
			@entityTable,
			COUNT ( 1 ) AS [RowCount],
			0
		FROM
			[dbo].[Entity] WITH ( NOLOCK )
		WHERE
			[TenantId] = @tenantId

		INSERT INTO [dbo].[Lic_Record]
		SELECT
			@idx,
			@tenantId,
			@relationshipTable,
			COUNT ( 1 ) AS [RowCount],
			0
		FROM
			[dbo].[Relationship] WITH ( NOLOCK )
		WHERE
			[TenantId] = @tenantId

		INSERT INTO [dbo].[Lic_Record]
		SELECT
			@idx,
			@tenantId,
			@data_Alias,
			COUNT ( 1 ) AS [RowCount],
			0
		FROM
			[dbo].[Data_Alias] WITH ( NOLOCK )
		WHERE
			[TenantId] = @tenantId

		INSERT INTO [dbo].[Lic_Record]
		SELECT
			@idx,
			@tenantId,
			@data_Bit,
			COUNT ( 1 ) AS [RowCount],
			0
		FROM
			[dbo].[Data_Bit] WITH ( NOLOCK )
		WHERE
			[TenantId] = @tenantId

		INSERT INTO [dbo].[Lic_Record]
		SELECT
			@idx,
			@tenantId,
			@data_DateTime,
			COUNT ( 1 ) AS [RowCount],
			0
		FROM
			[dbo].[Data_DateTime] WITH ( NOLOCK )
		WHERE
			[TenantId] = @tenantId

		INSERT INTO [dbo].[Lic_Record]
		SELECT
			@idx,
			@tenantId,
			@data_Decimal,
			COUNT ( 1 ) AS [RowCount],
			0
		FROM
			[dbo].[Data_Decimal] WITH ( NOLOCK )
		WHERE
			[TenantId] = @tenantId

		INSERT INTO [dbo].[Lic_Record]
		SELECT
			@idx,
			@tenantId,
			@data_Guid,
			COUNT ( 1 ) AS [RowCount],
			0
		FROM
			[dbo].[Data_Guid] WITH ( NOLOCK )
		WHERE
			[TenantId] = @tenantId

		INSERT INTO [dbo].[Lic_Record]
		SELECT
			@idx,
			@tenantId,
			@data_Int,
			COUNT ( 1 ) AS [RowCount],
			0
		FROM
			[dbo].[Data_Int] WITH ( NOLOCK )
		WHERE
			[TenantId] = @tenantId

		INSERT INTO [dbo].[Lic_Record]
		SELECT
			@idx,
			@tenantId,
			@data_NVarChar,
			COUNT ( 1 ) AS [RowCount],
			0
		FROM
			[dbo].[Data_NVarChar] WITH ( NOLOCK )
		WHERE
			[TenantId] = @tenantId

		INSERT INTO [dbo].[Lic_Record]
		SELECT
			@idx,
			@tenantId,
			@data_Xml,
			COUNT ( 1 ) AS [RowCount],
			0
		FROM
			[dbo].[Data_Xml] WITH ( NOLOCK )
		WHERE
			[TenantId] = @tenantId
		

		-- entity records per app
		INSERT INTO
			[dbo].[Lic_Record]
		SELECT
			@idx,
			@tenantId,
			@entityTable,
			COUNT ( 1 ),
			r.[ToId]
		FROM
			[dbo].[Entity] e WITH ( NOLOCK )
		JOIN
			[dbo].[Relationship] r WITH ( NOLOCK ) ON
				r.TenantId = e.TenantId AND
				r.FromId = e.Id AND
				r.TypeId = @inSolution
		WHERE
			e.TenantId = @tenantId
		GROUP BY
			r.[ToId]

		-- Data_Alias records per app
		INSERT INTO
			[dbo].[Lic_Record]
		SELECT
			@idx,
			@tenantId,
			@data_Alias,
			COUNT ( 1 ),
			r.[ToId]
		FROM
			[dbo].[Data_Alias] d WITH ( NOLOCK )
		JOIN
			[dbo].[Relationship] r WITH ( NOLOCK ) ON
				r.TenantId = d.TenantId AND
				r.[FromId] = d.[EntityId] AND
				r.TypeId = @inSolution
		WHERE
			d.TenantId = @tenantId
		GROUP BY
			r.[ToId]

		-- Data_Bit records per app
		INSERT INTO
			[dbo].[Lic_Record]
		SELECT
			@idx,
			@tenantId,
			@data_Bit,
			COUNT ( 1 ),
			r.[ToId]
		FROM
			[dbo].[Data_Bit] d WITH ( NOLOCK )
		JOIN
			[dbo].[Relationship] r WITH ( NOLOCK ) ON
				r.TenantId = d.TenantId AND
				r.[FromId] = d.[EntityId] AND
				r.TypeId = @inSolution
		WHERE
			d.TenantId = @tenantId
		GROUP BY
			r.[ToId]

		-- Data_DateTime records per app
		INSERT INTO
			[dbo].[Lic_Record]
		SELECT
			@idx,
			@tenantId,
			@data_DateTime,
			COUNT ( 1 ),
			r.[ToId]
		FROM
			[dbo].[Data_DateTime] d WITH ( NOLOCK )
		JOIN
			[dbo].[Relationship] r WITH ( NOLOCK ) ON
				r.TenantId = d.TenantId AND
				r.[FromId] = d.[EntityId] AND
				r.TypeId = @inSolution
		WHERE
			d.TenantId = @tenantId
		GROUP BY
			r.[ToId]

		-- Data_Decimal records per app
		INSERT INTO
			[dbo].[Lic_Record]
		SELECT
			@idx,
			@tenantId,
			@data_Decimal,
			COUNT ( 1 ),
			r.[ToId]
		FROM
			[dbo].[Data_Decimal] d WITH ( NOLOCK )
		JOIN
			[dbo].[Relationship] r WITH ( NOLOCK ) ON
				r.TenantId = d.TenantId AND
				r.[FromId] = d.[EntityId] AND
				r.TypeId = @inSolution
		WHERE
			d.TenantId = @tenantId
		GROUP BY
			r.[ToId]

		-- Data_Guid records per app
		INSERT INTO
			[dbo].[Lic_Record]
		SELECT
			@idx,
			@tenantId,
			@data_Guid,
			COUNT ( 1 ),
			r.[ToId]
		FROM
			[dbo].[Data_Guid] d WITH ( NOLOCK )
		JOIN
			[dbo].[Relationship] r WITH ( NOLOCK ) ON
				r.TenantId = d.TenantId AND
				r.[FromId] = d.[EntityId] AND
				r.TypeId = @inSolution
		WHERE
			d.TenantId = @tenantId
		GROUP BY
			r.[ToId]

		-- Data_Int records per app
		INSERT INTO
			[dbo].[Lic_Record]
		SELECT
			@idx,
			@tenantId,
			@data_Int,
			COUNT ( 1 ),
			r.[ToId]
		FROM
			[dbo].[Data_Int] d WITH ( NOLOCK )
		JOIN
			[dbo].[Relationship] r WITH ( NOLOCK ) ON
				r.TenantId = d.TenantId AND
				r.[FromId] = d.[EntityId] AND
				r.TypeId = @inSolution
		WHERE
			d.TenantId = @tenantId
		GROUP BY
			r.[ToId]

		-- Data_NVarChar records per app
		INSERT INTO
			[dbo].[Lic_Record]
		SELECT
			@idx,
			@tenantId,
			@data_NVarChar,
			COUNT ( 1 ),
			r.[ToId]
		FROM
			[dbo].[Data_NVarChar] d WITH ( NOLOCK )
		JOIN
			[dbo].[Relationship] r WITH ( NOLOCK ) ON
				r.TenantId = d.TenantId AND
				r.[FromId] = d.[EntityId] AND
				r.TypeId = @inSolution
		WHERE
			d.TenantId = @tenantId
		GROUP BY
			r.[ToId]

		-- Data_Xml records per app
		INSERT INTO
			[dbo].[Lic_Record]
		SELECT
			@idx,
			@tenantId,
			@data_Xml,
			COUNT ( 1 ),
			r.[ToId]
		FROM
			[dbo].[Data_Xml] d WITH ( NOLOCK )
		JOIN
			[dbo].[Relationship] r WITH ( NOLOCK ) ON
				r.TenantId = d.TenantId AND
				r.[FromId] = d.[EntityId] AND
				r.TypeId = @inSolution
		WHERE
			d.TenantId = @tenantId
		GROUP BY
			r.[ToId]
	
		FETCH NEXT FROM Tenants INTO @tenantId
	END

	CLOSE Tenants
	DEALLOCATE Tenants
END