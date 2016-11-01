-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE PROCEDURE [dbo].[spLicWorkflow]
AS
BEGIN

	SET NOCOUNT ON

	DECLARE @idx BIGINT
	DECLARE @ts DATETIME

	SELECT TOP 1
		@idx = [Id]
	FROM
		[dbo].[Lic_Index]
	ORDER BY
		[Timestamp] DESC

	SELECT
		@ts = CASE WHEN
			MAX ( [Timestamp] ) IS NULL
		THEN
			'1980-01-01'
		ELSE
			MAX ( [Timestamp] )
		END
	FROM
		[dbo].[Lic_Index]
	WHERE
		[Id] <> @idx

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

		DECLARE @workflow BIGINT = [dbo].[fnAliasNsId]( 'workflow', 'core', @tenantId )
		DECLARE @isOfType BIGINT = [dbo].[fnAliasNsId]( 'isOfType', 'core', @tenantId )
		DECLARE @inSolution BIGINT = [dbo].[fnAliasNsId]( 'inSolution', 'core', @tenantId )
		DECLARE @workflowBeingRun BIGINT = [dbo].[fnAliasNsId]( 'workflowBeginRun', 'core', @tenantId )
		DECLARE @createdDate BIGINT = [dbo].[fnAliasNsId]( 'createdDate', 'core', @tenantId )

		---- Workflows (with total runs since last count was taken)
		INSERT INTO
			[Lic_Workflow]
		SELECT
			@idx,
			@tenantId,
			r.FromId,
			ISNULL(MAX ( [dbo].[fnName]( r.FromId ) ), ''),
			COUNT ( rw.[Id] ),
			ISNULL(MAX ( rs.[ToId] ), 0)
		FROM
			[dbo].[fnDerivedTypes] ( @workflow, @tenantId ) fn
		JOIN
			[dbo].[Relationship] r WITH ( NOLOCK ) ON
				r.TenantId = @tenantId
				AND r.TypeId = @isOfType
				AND r.ToId = fn.Id
		LEFT JOIN 
			[dbo].[Relationship] rs WITH ( NOLOCK ) ON
				rs.TenantId = r.TenantId AND
				rs.FromId = r.FromId AND
				rs.TypeId = @inSolution
		LEFT JOIN
			[dbo].[Relationship] rw WITH (NOLOCK) ON
				rw.TenantId = r.TenantId AND
				rw.ToId = r.FromId AND
				rw.TypeId = @workflowBeingRun
		LEFT JOIN
			[dbo].[Data_DateTime] f2 WITH ( NOLOCK ) ON
				f2.TenantId = rw.TenantId AND
				f2.EntityId = rw.FromId AND
				f2.FieldId = @createdDate AND
				f2.Data > @ts
		WHERE
			r.TenantId = @tenantId
		GROUP BY
			r.[FromId]

	FETCH NEXT FROM Tenants INTO @tenantId
	END

	CLOSE Tenants
	DEALLOCATE Tenants
END