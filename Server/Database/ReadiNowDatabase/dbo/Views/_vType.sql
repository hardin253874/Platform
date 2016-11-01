-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE VIEW [dbo].[_vType]
AS
	SELECT
		e.Id,
		r.ToId TypeId,
		e.TenantId TenantId,
		f1.Data name,
		f2.Data description,
		f3.Data createdDate,
		f4.Data modifiedDate,
		f5.Data alias,
		f6.Data isAbstract,
		f7.Data isSealed,
		f8.Data supportMultiTypes,
		f9.Data className,
		f10.Data classType,
		f11.Data generateCode,
		f12.Data systemTenantOnly
	FROM (
		SELECT
			EntityId,
			TenantId
		FROM
			Data_Alias a1
		WHERE
			a1.Namespace = 'core' AND
			a1.Data = 'type'
	) t
	CROSS APPLY
		dbo.fnDerivedTypes( t.EntityId, t.TenantId ) dt
	JOIN
		Relationship r ON
			dt.Id = r.ToId AND
			t.TenantId = r.TenantId
	JOIN
		Data_Alias aa ON
			aa.TenantId = r.TenantId AND
			r.TypeId = aa.EntityId AND
			aa.Namespace = 'core' AND
			aa.Data = 'isOfType'
	JOIN
		Entity e ON
			e.Id = r.FromId AND
			e.TenantId = r.TenantId
    OUTER APPLY
		dbo.tblFnFieldNVarCharA( e.Id, e.TenantId, 'name', 'core' ) f1
	OUTER APPLY
		dbo.tblFnFieldNVarCharA( e.Id, e.TenantId, 'description', 'core' ) f2
	OUTER APPLY
		dbo.tblFnFieldDateTimeA( e.Id, e.TenantId, 'createdDate', 'core' ) f3
	OUTER APPLY
		dbo.tblFnFieldDateTimeA( e.Id, e.TenantId, 'modifiedDate', 'core' ) f4
	OUTER APPLY
		dbo.tblFnFieldAliasA( e.Id, e.TenantId, 'alias', 'core' ) f5
	OUTER APPLY
		dbo.tblFnFieldBitA( e.Id, e.TenantId, 'isAbstract', 'core' ) f6
	OUTER APPLY
		dbo.tblFnFieldBitA( e.Id, e.TenantId, 'isSealed', 'core' ) f7
	OUTER APPLY
		dbo.tblFnFieldBitA( e.Id, e.TenantId, 'supportMultiTypes', 'core' ) f8
	OUTER APPLY
		dbo.tblFnFieldNVarCharA( e.Id, e.TenantId, 'className', 'core' ) f9
	OUTER APPLY
		dbo.tblFnFieldNVarCharA( e.Id, e.TenantId, 'classType', 'core' ) f10
	OUTER APPLY
		dbo.tblFnFieldBitA( e.Id, e.TenantId, 'generateCode', 'core' ) f11
	OUTER APPLY
		dbo.tblFnFieldBitA( e.Id, e.TenantId, 'systemTenantOnly', 'core' ) f12