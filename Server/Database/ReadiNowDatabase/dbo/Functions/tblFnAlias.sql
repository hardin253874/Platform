-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE FUNCTION [dbo].[tblFnAlias]
(
	@id BIGINT,
	@tenantId BIGINT
)
RETURNS TABLE
AS
RETURN
(
	SELECT TOP 1
		a.Data
	FROM
		Data_Alias a
	WHERE
		a.EntityId = @id AND 
		a.TenantId = @tenantId AND
 		a.AliasMarkerId = 0
)