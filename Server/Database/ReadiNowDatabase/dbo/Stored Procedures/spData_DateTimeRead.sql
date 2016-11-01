-- Copyright 2011-2016 Global Software Innovation Pty Ltd




CREATE PROCEDURE [dbo].[spData_DateTimeRead]
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
		a.Data
	FROM
		Data_DateTime a
	WHERE
		TenantId = @tenantId AND
		EntityId = @entityId AND
		FieldId = @fieldId
END