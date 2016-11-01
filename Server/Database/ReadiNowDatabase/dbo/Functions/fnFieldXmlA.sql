-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE FUNCTION [dbo].[fnFieldXmlA]
(
	-- Add the parameters for the function here
	@id bigint,
	@fieldAlias nvarchar(max)
)
RETURNS nvarchar(max)
AS
BEGIN
    declare @tenantId bigint = (select TenantId from Entity where Id=@id)
	RETURN dbo.fnFieldXml(@id, dbo.fnAliasId(@fieldAlias, @tenantId))
END

