-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE FUNCTION [dbo].[tblFnAppVerName]
(
	@appVerUid UNIQUEIDENTIFIER
)
RETURNS TABLE
AS
RETURN
(
	SELECT TOP 1
		n.Data AS Name
	FROM
		Data_Guid g
	INNER JOIN
		dbo.tblFnAliasNsId( 'appVerId', 'core', DEFAULT ) a ON
			g.FieldId = a.EntityId
	CROSS APPLY
		dbo.tblFnName( g.EntityId, 0 ) n
	WHERE
		g.Data = @appVerUid AND
		g.TenantId = 0
)