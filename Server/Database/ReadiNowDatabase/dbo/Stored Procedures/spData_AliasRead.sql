-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE PROCEDURE [dbo].[spData_AliasRead]
	@entityId BIGINT,
	@tenantId BIGINT,
	@fieldId BIGINT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		a.EntityId,
		a.TenantId,
		a.FieldId,
		a.Namespace,
		a.Data
	FROM
		Data_Alias a
	WHERE
		TenantId = @tenantId AND
		EntityId = @entityId AND
		FieldId = @fieldId
END