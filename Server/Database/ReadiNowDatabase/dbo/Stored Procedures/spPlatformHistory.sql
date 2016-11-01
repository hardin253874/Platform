-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE PROCEDURE [dbo].[spPlatformHistory]
	@tenantId bigint = 0,
	@packageId uniqueidentifier = null,
	@operation nvarchar(100) = 'Unknown',
	@machine nvarchar(200) = null,
	@user nvarchar(200) = null,
	@process nvarchar(200) = null,
	@arguments nvarchar(max) = null,
	@exception nvarchar(max) = null
AS
BEGIN
	DECLARE @packageName NVARCHAR(200) = [dbo].[fnAppVerName](@packageId);
	IF (@packageName IS NULL)
	BEGIN

		-- in some cases, package id is in fact an entity's upgrade id
		DECLARE @packageIdField BIGINT = [dbo].[fnAliasNsId]('packageId', 'core', @tenantId)
		
		SET @packageId = (SELECT 
			ISNULL([dbo].[fnFieldGuid](e.Id, @packageIdField), @packageId)
		FROM
			[dbo].[Entity] e
		WHERE
			e.[UpgradeId] = @packageId AND e.[TenantId] = @tenantId)

		SET @packageName = [dbo].[fnAppVerName](@packageId);

	END

	INSERT INTO [dbo].[PlatformHistory] 
	(
		[TenantName],
		[TenantId],
		[PackageName],
		[PackageId],
		[Operation],
		[Machine],
		[User],
		[Process],
		[Arguments],
		[Exception]
	)
	VALUES (
		[dbo].[fnName](@tenantId),
		@tenantId,
		@packageName,
		@packageId,
		@operation,
		@machine,
		@user,
		@process,
		@arguments,
		@exception
	)
END
