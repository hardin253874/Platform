-- Copyright 2011-2016 Global Software Innovation Pty Ltd


CREATE view [dbo].[vRelationshipDefn] as
(
	SELECT
		e.Id,
		e.TenantId,
		f2.Data cascadeDeleteFrom,
		f3.Data cascadeDeleteTo
	FROM
		Entity e  
	JOIN
		Relationship r ON
			r.TenantId = e.TenantId AND
			r.FromId = e.Id
	JOIN
		Data_Alias iot ON
			iot.TenantId = r.TenantId AND
			iot.EntityId = r.TypeId AND
			iot.Data = 'isOfType' AND
			iot.Namespace = 'core' AND
			iot.AliasMarkerId = 0
    JOIN
		Data_Alias da2 ON
			da2.EntityId = r.ToId AND
			da2.TenantId = r.TenantId AND
			da2.Data = 'relationship' AND
			da2.Namespace = 'core'
	OUTER APPLY
		dbo.tblFnFieldBitA( e.Id, e.TenantId, 'cascadeDelete', 'core' ) f2
	OUTER APPLY
		dbo.tblFnFieldBitA( e.Id, e.TenantId, 'cascadeDeleteTo', 'core' ) f3
	WHERE
		r.TypeId = iot.EntityId
)