-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE FUNCTION [dbo].[fnFieldAlias]
(
	-- Add the parameters for the function here
	@id bigint,
	@fieldId bigint
)
RETURNS NVarChar(100)
AS
BEGIN
	DECLARE @result NVarChar(100)

	-- Note: we need to both resolve the ID of alias, as well as the ID of the item we're looking for
	select @result = Data
		from Data_Alias result
		where EntityId = @id and FieldId = @fieldId

	RETURN @result

END
