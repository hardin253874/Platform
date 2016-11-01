-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE FUNCTION [dbo].[tblFnAliasId]
(
	-- Add the parameters for the function here
	@alias NVARCHAR( MAX ),
	@tenantId BIGINT
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
		a.TenantId = @tenantId AND
 		a.AliasMarkerId = 0
)