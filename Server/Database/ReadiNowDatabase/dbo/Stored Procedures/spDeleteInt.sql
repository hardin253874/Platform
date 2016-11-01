-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE PROCEDURE [dbo].[spDeleteInt]
	@data dbo.FieldKeyType READONLY
AS
BEGIN

	SET NOCOUNT ON;

	DELETE
		dbo.Data_Int
	FROM
		dbo.Data_Int a
	INNER JOIN
		@data b
	ON
		a.EntityId = b.EntityId AND
		a.TenantId = b.TenantId AND
		a.FieldId = b.FieldId
END