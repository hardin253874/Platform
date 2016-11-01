-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE FUNCTION [dbo].[tblFnFieldNVarCharA]
(
	@entityId BIGINT,
	@tenantId BIGINT,
	@alias NVARCHAR( 100 ),
	@namespace NVARCHAR( 100 )	
)
RETURNS TABLE 
AS
RETURN 
(
	SELECT TOP 1
		d.Data
	FROM
		Data_NVarChar d
	INNER JOIN
		Data_Alias a ON
			d.TenantId = a.TenantId AND
			d.FieldId = a.EntityId AND
			a.Data = @alias AND
			a.Namespace = @namespace AND
			a.AliasMarkerId = 0
	WHERE
		d.EntityId = @entityId AND
		d.TenantId = @tenantId
)