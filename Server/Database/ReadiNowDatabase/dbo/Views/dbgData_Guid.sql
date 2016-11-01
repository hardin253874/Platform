-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE view [dbo].[dbgData_Guid]
as
    SELECT
		d.EntityId,
		d.TenantId,
		d.FieldId,
		d.Data,
		[Entity] = ISNULL( ea.Data, en.Data ),
		[Field] = ISNULL( fa.Data, fn.Data )
	FROM
		Data_Guid d
	OUTER APPLY
		dbo.tblFnAlias( d.EntityId, d.TenantId ) ea
	OUTER APPLY
		dbo.tblFnName( d.EntityId, d.TenantId ) en
	OUTER APPLY
		dbo.tblFnAlias( d.FieldId, d.TenantId ) fa
	OUTER APPLY
		dbo.tblFnName( d.FieldId, d.TenantId ) fn
