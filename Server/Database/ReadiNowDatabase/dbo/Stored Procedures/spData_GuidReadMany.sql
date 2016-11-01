-- Copyright 2011-2016 Global Software Innovation Pty Ltd


CREATE PROCEDURE [dbo].[spData_GuidReadMany]
	@data dbo.FieldKeyType READONLY
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		a.EntityId,
		a.TenantId,
		a.FieldId,
		a.Data
	FROM
		Data_Guid a
	JOIN
		@data d
	ON
		a.EntityId = d.EntityId AND
		a.TenantId = d.TenantId AND
		a.FieldId = d.FieldId
END