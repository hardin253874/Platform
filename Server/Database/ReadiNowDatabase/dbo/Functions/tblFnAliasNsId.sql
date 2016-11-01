-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE FUNCTION [dbo].[tblFnAliasNsId]
(
	@alias NVARCHAR( MAX ),
	@namespace NVARCHAR( MAX ),
	@tenantId BIGINT = 0
)
RETURNS TABLE
AS
RETURN
(
	SELECT TOP 1
		a.EntityId
	FROM
		Data_Alias a
	WHERE
		a.Data = @alias AND
		a.Namespace = @namespace AND
 		a.TenantId = @tenantId AND
 		a.AliasMarkerId = 0
)