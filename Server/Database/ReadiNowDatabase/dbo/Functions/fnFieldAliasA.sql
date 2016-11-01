-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE FUNCTION [dbo].[fnFieldAliasA]
(
	-- Add the parameters for the function here
	@id bigint,
	@fieldAlias nvarchar(max)
)
RETURNS NVarChar(100)
AS
BEGIN
    declare @tenantId bigint = (select TenantId from Entity where Id=@id)
	RETURN dbo.fnFieldAlias(@id, dbo.fnAliasId(@fieldAlias, @tenantId))
END
