
-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE PROCEDURE [dbo].[spLicTenant]
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
		DECLARE @isTenantDisabled BIGINT = [dbo].[fnAliasNsId]( 'isTenantDisabled', 'core', @tenantId )

		INSERT INTO
			[dbo].[Lic_Tenant]
		SELECT
			@idx,
			@tenantId,
			ISNULL( [dbo].[fnName]( @tenantId ), '' ),
			[dbo].[fnFieldBit]( @tenantId, @isTenantDisabled )

		FETCH NEXT FROM Tenants INTO @tenantId
	END

	CLOSE Tenants
	DEALLOCATE Tenants

END
