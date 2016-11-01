-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE FUNCTION [dbo].[fnAlias]
(
	-- Add the parameters for the function here
	@id bigint,
	@TenantId bigint = 0
)
RETURNS nvarchar(max)
AS
BEGIN
	DECLARE @result nvarchar(max)

	-- Note: we need to both resolve the ID of alias, as well as the ID of the item we're looking for
	select @result = a.Data
		from Data_Alias a
		where a.EntityId = @id
 			and a.TenantId = @TenantId
 			and a.AliasMarkerId = 0		-- 'alias', rather than 'reverseAlias'

	RETURN @result

END
