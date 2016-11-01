-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE FUNCTION [dbo].[fnNameId]
(
	-- Add the parameters for the function here
	@name nvarchar(max),
	@TenantId bigint
)
RETURNS bigint
AS
BEGIN
	DECLARE @result bigint

	select @result = result.EntityId
		from Data_NVarChar result
		join Data_Alias nameAlias on result.FieldId = nameAlias.EntityId
		where
			nameAlias.Data = 'name'
			and nameAlias.Namespace = 'core'
			and nameAlias.TenantId = @TenantId
			and result.Data = @name
			and result.TenantId = @TenantId

	RETURN @result

END
