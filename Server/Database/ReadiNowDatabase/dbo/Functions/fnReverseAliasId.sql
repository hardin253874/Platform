-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE FUNCTION [dbo].[fnReverseAliasId]
(
	-- Add the parameters for the function here
	@alias nvarchar(max),
	@TenantId bigint = 0
)
RETURNS bigint
AS
BEGIN
	DECLARE @result bigint

	-- Note: we need to both resolve the ID of alias, as well as the ID of the item we're looking for
	select @result = a.EntityId
		from Data_Alias a
		where a.Data = @alias
 			and a.TenantId = @TenantId
 			and a.AliasMarkerId = 1		-- 'reverseAlias', rather than 'alias'

	RETURN @result

END
