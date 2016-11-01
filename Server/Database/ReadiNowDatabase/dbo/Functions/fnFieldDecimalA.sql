-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE FUNCTION [dbo].[fnFieldDecimalA]
(
	-- Add the parameters for the function here
	@id bigint,
	@fieldAlias nvarchar(max)
)
RETURNS decimal(38,10)
AS
BEGIN
    declare @tenantId bigint = (select TenantId from Entity where Id=@id)
	RETURN dbo.fnFieldDecimal(@id, dbo.fnAliasId(@fieldAlias, @tenantId))
END

