-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE PROCEDURE [dbo].[spResolveAlias]
	@tenantId BIGINT,
	@alias NVARCHAR( 100 ),
	@namespace NVARCHAR( 100 )	
AS
BEGIN
	SET NOCOUNT ON

	SELECT a.EntityId, a.AliasMarkerId
	FROM Data_Alias a
	WHERE a.TenantId = @tenantId AND a.Namespace = @namespace AND a.Data = @alias
END