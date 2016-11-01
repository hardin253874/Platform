-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE PROCEDURE [dbo].[spAliasLookupReadMany]
	@data dbo.AliasLookupType READONLY
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
	JOIN
		@data d
	ON
		a.TenantId = d.TenantId AND
		a.Namespace = d.Namespace AND
		a.Data = d.Data
END