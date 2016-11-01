-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE PROCEDURE [dbo].[spAliasLookupRead]
	@tenantId BIGINT,
	@data NVARCHAR( 100 ),
	@namespace NVARCHAR( 100 )
	
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		a.EntityId,
		a.TenantId,
		a.Namespace,
		a.Data
	FROM
		Data_Alias a
	WHERE
		TenantId = @tenantId AND
		Namespace = @namespace AND
		Data = @data
END