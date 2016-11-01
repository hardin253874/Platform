-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE PROCEDURE [dbo].[spDeleteDecimal]
	@data dbo.FieldKeyType READONLY
AS
BEGIN

	SET NOCOUNT ON;

	DELETE
		dbo.Data_Decimal
	FROM
		dbo.Data_Decimal a
	INNER JOIN
		@data b
	ON
		a.EntityId = b.EntityId AND
		a.TenantId = b.TenantId AND
		a.FieldId = b.FieldId
END