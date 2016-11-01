
-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE VIEW [dbo].[dbgAppEntity]
AS
	SELECT
		e.Id,
		e.AppVerUid,
		e.EntityUid,
		[AppVer] = avn.Name,
		[Entity] = ISNULL( ea.Alias, en.Name )
	FROM
		AppEntity e
	OUTER APPLY
		dbo.tblFnAppVerName( e.AppVerUid ) avn
	OUTER APPLY
		dbo.tblFnAppAlias( e.EntityUid, e.AppVerUid ) ea
	OUTER APPLY
		dbo.tblFnAppName( e.EntityUid, e.AppVerUid ) en