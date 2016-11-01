-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE VIEW [dbo].[dbgRelationship] AS

	SELECT
		r.*,
		ISNULL( CAST( tya.Data AS NVARCHAR( MAX ) ), tyn.Data ) [Type],
		ISNULL( CAST( fra.Data AS NVARCHAR( MAX ) ), frn.Data ) [From],
		ISNULL( CAST( toa.Data AS NVARCHAR( MAX ) ), ton.Data ) [To]
	FROM
		Relationship r
	OUTER APPLY
		dbo.tblFnAlias( r.TypeId, r.TenantId ) tya
	OUTER APPLY
		dbo.tblFnName( r.TypeId, r.TenantId ) tyn
	OUTER APPLY
		dbo.tblFnAlias( r.FromId, r.TenantId ) fra
	OUTER APPLY
		dbo.tblFnName( r.FromId, r.TenantId ) frn
	OUTER APPLY
		dbo.tblFnAlias( r.ToId, r.TenantId ) toa
	OUTER APPLY
		dbo.tblFnName( r.ToId, r.TenantId ) ton
