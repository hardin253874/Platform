
-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE VIEW [dbo].[dbgEntity] AS

	SELECT
		e.*,
		ISNULL( CAST( ea.Data AS NVARCHAR( MAX ) ), en.Data ) [Entity]
	FROM
		Entity e
	OUTER APPLY
		dbo.tblFnAlias( e.Id, e.TenantId ) ea
	OUTER APPLY
		dbo.tblFnName( e.Id, e.TenantId ) en