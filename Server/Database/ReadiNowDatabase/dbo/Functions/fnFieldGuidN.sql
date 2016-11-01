-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE FUNCTION [dbo].[fnFieldGuidN]
(
	-- Add the parameters for the function here
	@id bigint,
	@fieldName nvarchar(max)
)
RETURNS uniqueidentifier
AS
BEGIN
    declare @tenantId bigint = (select TenantId from Entity where Id=@id)
	RETURN dbo.fnFieldGuid(@id, dbo.fnNameId(@fieldName, @tenantId))
END;

