-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE FUNCTION [dbo].[tblFnAliasName]
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
		ISNULL( a.Namespace + ':' + a.Data, n.Data ) AS Data
	FROM
		Entity e
	LEFT JOIN
		Data_Alias a ON
			e.TenantId = a.TenantId AND
			e.Id = a.EntityId AND
			a.AliasMarkerId = 0
	LEFT JOIN (
		SELECT
			n.*
		FROM
			Data_NVarChar n
		JOIN
			Data_Alias a ON
				n.TenantId = a.TenantId AND
				n.FieldId = a.EntityId AND
				a.AliasMarkerId = 0
		) n ON
			e.TenantId = n.TenantId AND
			e.Id = n.EntityId
	WHERE
		e.Id = @id AND 
		e.TenantId = @tenantId
)