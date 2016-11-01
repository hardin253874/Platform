-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE VIEW [dbo].[_vReport]
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
		f6.Data canCreate,
		f7.Data canCreateDerivedTypes,
		f9.Data hideNewButton,
		f10.Data hideAddButton,
		f11.Data hideRemoveButton,
		f14.Data rollupSubTotals,
		f15.Data rollupGrandTotals,
		f16.Data rollupRowCounts,
		f17.Data rollupRowLabels,
		f18.Data rollupOptionLabels,
		f19.Data hideActionBar,
		f20.Data hideReportHeader,
		f26.Data defaultDataViewId,
		f27.Data isCalculatedFieldReport,
		f29.Data hideOnDesktop,
		f30.Data hideOnTablet,
		f31.Data hideOnMobile
	FROM (
		SELECT
			EntityId,
			TenantId
		FROM
			Data_Alias a1
		WHERE
			a1.Namespace = 'core' AND
			a1.Data = 'report'
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
	JOIN Entity e ON
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
		dbo.tblFnFieldBitA( e.Id, e.TenantId, 'canCreate', 'core' ) f6
	OUTER APPLY
		dbo.tblFnFieldBitA( e.Id, e.TenantId, 'canCreateDerivedTypes', 'core' ) f7
	OUTER APPLY
		dbo.tblFnFieldBitA( e.Id, e.TenantId, 'hideNewButton', 'core' ) f9
	OUTER APPLY
		dbo.tblFnFieldBitA( e.Id, e.TenantId, 'hideAddButton', 'core' ) f10
	OUTER APPLY
		dbo.tblFnFieldBitA( e.Id, e.TenantId, 'hideRemoveButton', 'core' ) f11
	OUTER APPLY
		dbo.tblFnFieldBitA( e.Id, e.TenantId, 'rollupSubTotals', 'core' ) f14
	OUTER APPLY
		dbo.tblFnFieldBitA( e.Id, e.TenantId, 'rollupGrandTotals', 'core' ) f15
	OUTER APPLY
		dbo.tblFnFieldBitA( e.Id, e.TenantId, 'rollupRowCounts', 'core' ) f16
	OUTER APPLY
		dbo.tblFnFieldBitA( e.Id, e.TenantId, 'rollupRowLabels', 'core' ) f17
	OUTER APPLY
		dbo.tblFnFieldBitA( e.Id, e.TenantId, 'rollupOptionLabels', 'core' ) f18
	OUTER APPLY
		dbo.tblFnFieldBitA( e.Id, e.TenantId, 'hideActionBar', 'core' ) f19
	OUTER APPLY
		dbo.tblFnFieldBitA( e.Id, e.TenantId, 'hideReportHeader', 'core' ) f20
	OUTER APPLY
		dbo.tblFnFieldGuidA( e.Id, e.TenantId, 'defaultDataViewId', 'core' ) f26
	OUTER APPLY
		dbo.tblFnFieldBitA( e.Id, e.TenantId, 'isCalculatedFieldReport', 'core' ) f27
	OUTER APPLY
		dbo.tblFnFieldBitA( e.Id, e.TenantId, 'hideOnDesktop', 'core' ) f29
	OUTER APPLY
		dbo.tblFnFieldBitA( e.Id, e.TenantId, 'hideOnTablet', 'core' ) f30
	OUTER APPLY
		dbo.tblFnFieldBitA( e.Id, e.TenantId, 'hideOnMobile', 'core' ) f31