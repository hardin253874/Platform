-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE VIEW [dbo].[_vSolution]
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
		f6.Data baseName,
		f7.Data solutionVersionString,
		f8.Data solutionVersionDetails,
		f9.Data legacyGuid,
		f10.Data packageId,
		f11.Data solutionReleaseDate,
		f12.Data solutionPublisher,
		f13.Data solutionPublisherUrl,
		f14.Data hideOnDesktop,
		f15.Data hideOnTablet,
		f16.Data hideOnMobile
	FROM (
		SELECT
			EntityId,
			TenantId
		FROM
			Data_Alias a1
		WHERE
			a1.Namespace = 'core' AND
			a1.Data = 'solution'
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
		dbo.tblFnFieldNVarCharA( e.Id, e.TenantId, 'baseName', 'core' ) f6
	OUTER APPLY
		dbo.tblFnFieldNVarCharA( e.Id, e.TenantId, 'solutionVersionString', 'core' ) f7
	OUTER APPLY
		dbo.tblFnFieldNVarCharA( e.Id, e.TenantId, 'solutionVersionDetails', 'core' ) f8
	OUTER APPLY
		dbo.tblFnFieldGuidA( e.Id, e.TenantId, 'legacyGuid', 'core' ) f9
	OUTER APPLY
		dbo.tblFnFieldGuidA( e.Id, e.TenantId, 'packageId', 'core' ) f10
	OUTER APPLY
		dbo.tblFnFieldDateTimeA( e.Id, e.TenantId, 'solutionReleaseDate', 'core' ) f11
	OUTER APPLY
		dbo.tblFnFieldNVarCharA( e.Id, e.TenantId, 'solutionPublisher', 'core' ) f12
	OUTER APPLY
		dbo.tblFnFieldNVarCharA( e.Id, e.TenantId, 'solutionPublisherUrl', 'core' ) f13
	OUTER APPLY
		dbo.tblFnFieldBitA( e.Id, e.TenantId, 'hideOnDesktop', 'core' ) f14
	OUTER APPLY
		dbo.tblFnFieldBitA( e.Id, e.TenantId, 'hideOnTablet', 'core' ) f15
	OUTER APPLY
		dbo.tblFnFieldBitA( e.Id, e.TenantId, 'hideOnMobile', 'core' ) f16