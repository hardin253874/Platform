-- Copyright 2011-2016 Global Software Innovation Pty Ltd
CREATE FUNCTION [dbo].[fnFromNameAlias]
(
	-- Add the parameters for the function here
	@id bigint,
	@TenantId bigint = 0
)
RETURNS nvarchar(max)
AS
BEGIN
	DECLARE @result nvarchar(max)
	DECLARE @fromName BIGINT = dbo.fnAliasNsId('fromName', 'core', @TenantId )
	DECLARE @name BIGINT = dbo.fnAliasNsId('name', 'core', @TenantId )

	-- Note: we need to both resolve the ID of alias, as well as the ID of the item we're looking for
SELECT
	@result = ISNULL(n.Data, ISNULL( n2.Data, a.Namespace + ':' + a.Data))
FROM
	Entity e
LEFT JOIN
	Data_Alias a ON
		e.Id = a.EntityId AND
		a.TenantId = @TenantId AND
		a.AliasMarkerId = 0
LEFT JOIN
	Data_NVarChar n ON
		e.Id = n.EntityId AND
		n.TenantId = @TenantId AND
		n.FieldId = @fromName
LEFT JOIN
	Data_NVarChar n2 ON
		e.Id = n2.EntityId AND
		n2.TenantId = @TenantId AND
		n2.FieldId = @name
WHERE
	e.Id = @id AND
	e.TenantId = @TenantId

	RETURN @result

END