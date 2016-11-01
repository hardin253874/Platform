-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE FUNCTION [dbo].[fnFieldIntN]
(
	-- Add the parameters for the function here
	@id bigint,
	@fieldName nvarchar(max)
)
RETURNS Int
AS
BEGIN
    declare @tenantId bigint = (select TenantId from Entity where Id=@id)
	RETURN dbo.fnFieldInt(@id, dbo.fnNameId(@fieldName, @tenantId))
END;
