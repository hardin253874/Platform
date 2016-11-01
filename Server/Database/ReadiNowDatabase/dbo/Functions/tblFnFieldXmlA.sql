-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE FUNCTION [dbo].[tblFnFieldXmlA]
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
		Data_Xml d
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