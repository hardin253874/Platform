-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE FUNCTION [dbo].[fnFieldXml]
(
	-- Add the parameters for the function here
	@id bigint,
	@fieldId bigint
)
RETURNS nvarchar(max)
AS
BEGIN
	DECLARE @result nvarchar(max)

	-- Note: we need to both resolve the ID of alias, as well as the ID of the item we're looking for
	select @result = Data
		from Data_Xml result
		JOIN Entity e ON result.TenantId = e.TenantId AND e.Id = result.EntityId
		where EntityId = @id and FieldId = @fieldId

	RETURN @result

END

