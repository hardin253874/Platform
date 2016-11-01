-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE FUNCTION [dbo].[fnAppVerName]
(
	-- Add the parameters for the function here
	@appVerUid UNIQUEIDENTIFIER
)
RETURNS nvarchar(max)
AS
BEGIN
	DECLARE @name nvarchar(max)
	DECLARE @appVerId BIGINT = dbo.fnAliasNsId('appVerId', 'core', DEFAULT)

	SELECT
		@name = dbo.fnName( g.EntityId )
	FROM
		Data_Guid g WHERE g.Data = @appVerUid AND g.TenantId = 0 and g.FieldId = @appVerId

	RETURN @name

END


