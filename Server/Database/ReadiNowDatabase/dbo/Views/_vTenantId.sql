-- Copyright 2011-2016 Global Software Innovation Pty Ltd


CREATE VIEW [dbo].[_vTenantId]
AS
	SELECT
		Id = r.FromId
	FROM
		Relationship r
	JOIN
		Data_Alias iot ON
			r.TypeId = iot.EntityId
			AND r.TenantId = iot.TenantId
			AND iot.Data = 'isOfType'
			AND iot.Namespace = 'core'
			AND iot.AliasMarkerId = 0
	JOIN
		Data_Alias t ON
			t.EntityId = r.ToId
			AND r.TenantId = t.TenantId
			AND t.Data = 'tenant'
			AND t.Namespace = 'core'
			AND t.AliasMarkerId = 0