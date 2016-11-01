-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE VIEW [dbo].[vSystemTenantEntityTypeOnly]
AS
	SELECT
		t.Id
	FROM
		_vType t
	WHERE
		t.systemTenantOnly = 1 AND
		t.TenantId = 0

	UNION 

	SELECT
		b.EntityId AS Id
	FROM
		Data_Bit b
	CROSS APPLY
		dbo.tblFnAliasNsId( 'systemTenantOnly', 'core', 0 ) a
	WHERE
		b.FieldId = a.EntityId AND
		b.Data = 1 AND
		TenantId = 0