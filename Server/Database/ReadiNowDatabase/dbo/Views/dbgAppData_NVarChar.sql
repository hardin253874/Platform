-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE VIEW [dbo].[dbgAppData_NVarChar]
AS
    SELECT
		d.AppVerUid,
		d.EntityUid,
		d.FieldUid,
		d.Data,
		[AppVer] = avn.Name,
		[Entity] = ISNULL( ea.Alias, en.Name ),
		[Field] = ISNULL( fa.Alias, fn.Name )
	FROM
		AppData_NVarChar d
	OUTER APPLY
		dbo.tblFnAppVerName( d.AppVerUid ) avn
	OUTER APPLY
		dbo.tblFnAppAlias( d.EntityUid, d.AppVerUid ) ea
	OUTER APPLY
		dbo.tblFnAppName( d.EntityUid, d.AppVerUid ) en
	OUTER APPLY
		dbo.tblFnAppAlias( d.FieldUid, d.AppVerUid ) fa
	OUTER APPLY
		dbo.tblFnAppName( d.FieldUid, d.AppVerUid ) fn
