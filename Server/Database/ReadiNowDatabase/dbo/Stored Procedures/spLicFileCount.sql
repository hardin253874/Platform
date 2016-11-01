-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE PROCEDURE [dbo].[spLicFileCount]
AS
BEGIN
	
		SET NOCOUNT ON

	DECLARE @idx BIGINT

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
			[dbo].[Relationship] WITH ( NOLOCK )
		WHERE
			TenantId = 0
			AND TypeId = @isOfTypeGlobal
			AND ToId = @tenant
	    
	OPEN Tenants

	FETCH NEXT FROM Tenants into @tenantId
	WHILE @@fetch_status = 0
	BEGIN

		DECLARE @document BIGINT = [dbo].[fnAliasNsId]( 'document', 'core', @tenantId )
		DECLARE @file BIGINT = [dbo].[fnAliasNsId]( 'fileType', 'core', @tenantId )
		DECLARE @isOfType BIGINT = [dbo].[fnAliasNsId]( 'isOfType', 'core', @tenantId )
		DECLARE @inSolution BIGINT = [dbo].[fnAliasNsId]( 'inSolution', 'core', @tenantId )
		DECLARE @inFolder BIGINT = [dbo].[fnAliasNsId]( 'inFolder', 'core', @tenantId )
		DECLARE @size BIGINT = [dbo].[fnAliasNsId]( 'size', 'core', @tenantId )
		DECLARE @generatedDocumentFolder BIGINT = [dbo].[fnAliasNsId]( 'generatedDocumentFolder', 'core', @tenantId )
		DECLARE @fileDataHash BIGINT = [dbo].[fnAliasNsId]( 'fileDataHash', 'core', @tenantId )

		-- Total per tenant
		INSERT INTO
			[Lic_File_Count]
		SELECT
			@idx,
			@tenantId,
			COUNT( 1 ),
			ISNULL( SUM( f1.[Data] ), 0 ),
			'generatedDocument',
			0
		FROM
			[dbo].[Entity] e WITH ( NOLOCK )
		JOIN
			[dbo].[Relationship] r WITH ( NOLOCK ) ON
				r.TenantId = e.TenantId AND
				r.[FromId] = e.[Id] AND
				r.TypeId = @isOfType AND
				r.ToId = @document
		JOIN
			[dbo].[Relationship] rf WITH ( NOLOCK ) ON
				rf.TenantId = e.TenantId AND
				rf.[FromId] = e.[Id] AND
				rf.TypeId = @inFolder AND
				rf.ToId = @generatedDocumentFolder
		JOIN
			[dbo].[Data_Int] f1 WITH ( NOLOCK ) ON
				f1.TenantId = e.TenantId AND
				f1.EntityId = e.Id AND
				f1.FieldId = @size
		WHERE
			e.TenantId = @tenantId
		HAVING
			COUNT( 1 ) > 0

		-- Totals per tenant, per app
		INSERT INTO
			[Lic_File_Count]
		SELECT
			@idx,
			@tenantId,
			COUNT ( 1 ),
			ISNULL( SUM ( f1.[Data] ), 0 ),
			'generatedDocument',
			rs.[ToId]
		FROM
			[dbo].[Entity] e WITH ( NOLOCK )
		JOIN
			[dbo].[Relationship] r WITH ( NOLOCK ) ON
				r.TenantId = e.TenantId AND
				r.[FromId] = e.[Id] AND
				r.TypeId = @isOfType AND
				r.ToId = @document
		JOIN
			[dbo].[Relationship] rf WITH (NOLOCK) ON
				rf.TenantId = e.TenantId AND
				rf.[FromId] = e.[Id] AND
				rf.TypeId = @inFolder AND
				rf.ToId = @generatedDocumentFolder
		JOIN
			[dbo].[Data_Int] f1 WITH ( NOLOCK ) ON
				f1.TenantId = e.TenantId AND
				f1.EntityId = e.Id AND
				f1.FieldId = @size
		JOIN
			[dbo].[Relationship] rs WITH ( NOLOCK ) ON
				rs.TenantId = e.TenantId AND
				rs.[FromId] = e.[Id] AND
				rs.TypeId = @inSolution
		WHERE
			e.TenantId = @tenantId
		GROUP BY
			rs.[ToId]

		-- Total files per tenant
		INSERT INTO
			[Lic_File_Count]
		SELECT
			@idx,
			@tenantId,
			COUNT ( 1 ),
			ISNULL( SUM ( fsize.Data ), 0),
			'file',
			0
		FROM
			[dbo].[fnDerivedTypes] ( @file, @tenantId ) fn
		JOIN
			[dbo].[Relationship] r WITH ( NOLOCK ) ON
				r.TypeId = @isOfType AND
				r.ToId = fn.Id
		LEFT JOIN
			[dbo].[Data_NVarChar] f1 WITH ( NOLOCK ) ON
				f1.TenantId = r.TenantId AND
				f1.EntityId = r.FromId AND
				f1.FieldId = @fileDataHash
		LEFT JOIN
			[dbo].[Data_Int] fsize WITH ( NOLOCK ) ON
				fsize.TenantId = r.TenantId AND
				fsize.EntityId = r.FromId AND
				fsize.FieldId = @size
		WHERE
			r.TenantId = @tenantId
		HAVING
			COUNT( 1 ) > 0

		-- Total files per tenant, per app
		INSERT INTO
			[Lic_File_Count]
		SELECT
			@idx,
			@tenantId,
			COUNT ( 1 ),
			ISNULL( SUM ( fsize.Data ), 0),
			'file',
			rs.[ToId]
		FROM
			[dbo].[fnDerivedTypes] ( @file, @tenantId ) fn
		JOIN
			[dbo].[Relationship] r WITH ( NOLOCK ) ON
				r.TypeId = @isOfType AND
				r.ToId = fn.Id
		LEFT JOIN
			[dbo].[Data_NVarChar] f1 WITH ( NOLOCK ) ON
				f1.TenantId = r.TenantId AND
				f1.EntityId = r.FromId AND
				f1.FieldId = @fileDataHash
		LEFT JOIN
			[dbo].[Data_Int] fsize WITH ( NOLOCK ) ON
				fsize.TenantId = r.TenantId AND
				fsize.EntityId = r.FromId AND
				fsize.FieldId = @size
		JOIN
			[dbo].[Relationship] rs WITH ( NOLOCK ) ON
				rs.TenantId = r.TenantId AND
				rs.[FromId] = r.[FromId] AND
				rs.TypeId = @inSolution
		WHERE
			r.TenantId = @tenantId
		GROUP BY
			rs.[ToId]
	
		FETCH NEXT FROM Tenants INTO @tenantId
	END

	CLOSE Tenants
	DEALLOCATE Tenants

END
