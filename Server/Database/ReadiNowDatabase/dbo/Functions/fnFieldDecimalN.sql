-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE FUNCTION [dbo].[fnFieldDecimalN]
(
	-- Add the parameters for the function here
	@id bigint,
	@fieldName nvarchar(max)
)
RETURNS decimal(38,10)
AS
BEGIN
    declare @tenantId bigint = (select TenantId from Entity where Id=@id)
	RETURN dbo.fnFieldDecimal(@id, dbo.fnNameId(@fieldName, @tenantId))
END;

