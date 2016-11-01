
-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE PROCEDURE [dbo].[spLicApplication]
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
		DECLARE @solutionVersionString BIGINT = [dbo].[fnAliasNsId]( 'solutionVersionString', 'core', @tenantId )
		DECLARE @solutionPublisher BIGINT = [dbo].[fnAliasNsId]( 'solutionPublisher', 'core', @tenantId )
		DECLARE @solutionReleaseDate BIGINT = [dbo].[fnAliasNsId]( 'solutionReleaseDate', 'core', @tenantId )
		DECLARE @packageId BIGINT = [dbo].[fnAliasNsId]( 'packageId', 'core', @tenantId )
		DECLARE @solution BIGINT = [dbo].[fnAliasNsId]( 'solution', 'core', @tenantId )

		INSERT INTO
			[dbo].[Lic_Application]
		SELECT
			@idx,
			r.FromId,
			@tenantId,
			ISNULL( [dbo].[fnName]( r.FromId ), '' ),
			[dbo].[fnFieldNVarChar]( r.FromId, @solutionVersionString ),
			[dbo].[fnFieldNVarChar]( r.FromId, @solutionPublisher ),
			[dbo].[fnFieldDateTime]( r.FromId, @solutionReleaseDate ),
			[dbo].[fnFieldGuid]( r.FromId, @packageId )
		FROM
			[dbo].[fnDerivedTypes] ( @solution, @tenantId ) fn
		JOIN
			[dbo].[Relationship] r WITH ( NOLOCK ) ON
				r.TenantId = @tenantId
				AND r.TypeId = @isOfType
				AND r.ToId = fn.Id

		FETCH NEXT FROM Tenants INTO @tenantId
	END

	CLOSE Tenants
	DEALLOCATE Tenants
END