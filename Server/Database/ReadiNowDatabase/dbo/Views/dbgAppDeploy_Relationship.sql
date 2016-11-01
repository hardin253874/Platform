-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE VIEW [dbo].[dbgAppDeploy_Relationship]
AS
	SELECT
		d.Id,
		d.AppVerUid,
		d.TenantId,
		d.TypeUid,
		d.FromUid,
		d.ToUid,
		[AppVer] = avn.Name,
		[Tenant] = t.Data,
		[Type] = ISNULL( tya.Alias, tyn.Name ),
		[From] = ISNULL( fra.Alias, frn.Name ),
		[To] = ISNULL( toa.Alias, ton.Name )
	FROM
		AppDeploy_Relationship d
	OUTER APPLY
		dbo.tblFnAppVerName( d.AppVerUid ) avn
	OUTER APPLY
		dbo.tblFnAliasName( d.TenantId, 0 ) t
	OUTER APPLY
		dbo.tblFnAppAlias( d.TypeUid, d.AppVerUid ) tya
	OUTER APPLY
		dbo.tblFnAppName( d.TypeUid, d.AppVerUid ) tyn
	OUTER APPLY
		dbo.tblFnAppAlias( d.FromUid, d.AppVerUid ) fra
	OUTER APPLY
		dbo.tblFnAppName( d.FromUid, d.AppVerUid ) frn
	OUTER APPLY
		dbo.tblFnAppAlias( d.ToUid, d.AppVerUid ) toa
	OUTER APPLY
		dbo.tblFnAppName( d.ToUid, d.AppVerUid ) ton