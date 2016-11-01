-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE FUNCTION [dbo].[fnFieldBitA]
(
	-- Add the parameters for the function here
	@id bigint,
	@fieldAlias nvarchar(max)
)
RETURNS Bit
AS
BEGIN
    declare @tenantId bigint = (select TenantId from Entity where Id=@id)
	RETURN dbo.fnFieldBit(@id, dbo.fnAliasId(@fieldAlias, @tenantId))
END
