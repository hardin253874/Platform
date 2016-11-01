-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE VIEW [dbo].[dbgAppDeploy_Field]
AS
	SELECT
		d.AppVerUid,
		d.TenantId,
		d.EntityUid,
		d.FieldUid,
		[AppVer] = avn.Name,
		[Tenant] = t.Data,
		[Entity] = ISNULL( ea.Alias, en.Name ),
		[Field] = ISNULL( fa.Alias, fn.Name ),
		[Data] = d.Data,
		[Type] = d.[Type]
	FROM
		AppDeploy_Field d
	OUTER APPLY
		dbo.tblFnAppVerName( d.AppVerUid ) avn
	OUTER APPLY
		dbo.tblFnAliasName( d.TenantId, 0 ) t
	OUTER APPLY
		dbo.tblFnAppAlias( d.EntityUid, d.AppVerUid ) ea
	OUTER APPLY
		dbo.tblFnAppName( d.EntityUid, d.AppVerUid ) en
	OUTER APPLY
		dbo.tblFnAppAlias( d.FieldUid, d.AppVerUid ) fa
	OUTER APPLY
		dbo.tblFnAppName( d.FieldUid, d.AppVerUid ) fn