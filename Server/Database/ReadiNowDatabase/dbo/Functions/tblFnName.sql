-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE FUNCTION [dbo].[tblFnName]
(
	-- Add the parameters for the function here
	@id BIGINT,
	@tenantId BIGINT
)
RETURNS TABLE
AS
RETURN
(
	SELECT TOP 1
		n.Data
	FROM
		Data_NVarChar n
	JOIN
		Data_Alias a ON
			n.TenantId = a.TenantId AND
			n.FieldId = a.EntityId AND
			a.Namespace = 'core' AND
			a.Data = 'name' AND
			a.AliasMarkerId = 0
	WHERE
		n.EntityId = @id AND
		n.TenantId = @tenantId
)