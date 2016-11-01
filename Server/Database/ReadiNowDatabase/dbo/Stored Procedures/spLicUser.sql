-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE PROCEDURE [dbo].[spLicUser]
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
		DECLARE @isOfType BIGINT = [dbo].[fnAliasNsId]( 'isOfType', 'core', @tenantId )
		DECLARE @userAccount BIGINT = [dbo].[fnAliasNsId]( 'userAccount', 'core', @tenantId )
		DECLARE @accountStatus BIGINT = [dbo].[fnAliasNsId]( 'accountStatus', 'core', @tenantId )

		INSERT INTO
			[dbo].[Lic_User]	
		SELECT
			@idx,
			@tenantId,
			r.[FromId],
			ISNULL( [dbo].[fnName]( r.FromId ), '' ),
			ISNULL( [dbo].[fnName]( s.ToId ), '' )
		FROM
			[dbo].[fnDerivedTypes] ( @userAccount, @tenantId ) fn
		JOIN
			[dbo].[Relationship] r WITH ( NOLOCK ) ON
				r.TenantId = @tenantId
				AND r.TypeId = @isOfType
				AND r.ToId = fn.Id
		LEFT JOIN
			[dbo].[Relationship] s WITH ( NOLOCK ) ON
				s.TenantId = r.TenantId
				AND s.FromId = r.FromId AND
				s.TypeId = @accountStatus

		FETCH NEXT FROM Tenants INTO @tenantId
	END

	CLOSE Tenants
	DEALLOCATE Tenants

END
