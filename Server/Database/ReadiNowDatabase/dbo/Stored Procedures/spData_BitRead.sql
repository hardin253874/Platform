-- Copyright 2011-2016 Global Software Innovation Pty Ltd


CREATE PROCEDURE [dbo].[spData_BitRead]
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
		Data_Bit a
	WHERE
		TenantId = @tenantId AND
		EntityId = @entityId AND
		FieldId = @fieldId
END