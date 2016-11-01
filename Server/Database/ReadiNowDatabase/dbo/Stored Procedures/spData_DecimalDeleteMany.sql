-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE PROCEDURE [dbo].[spData_DecimalDeleteMany]
	@data dbo.FieldKeyType READONLY,
	@context VARCHAR(128) = NULL
AS
BEGIN
	SET NOCOUNT ON;

	IF ( @context IS NULL )
	BEGIN
		SET @context = OBJECT_NAME(@@PROCID)
	END

	IF ( @context IS NOT NULL )
	BEGIN
		DECLARE @contextInfo VARBINARY(128) = CONVERT( VARBINARY(128), @context )
		SET CONTEXT_INFO @contextInfo
	END

	DECLARE @output TABLE
	(
		EntityId BIGINT,
		TenantId BIGINT,
		FieldId BIGINT,
		Data DECIMAL(38,10)
	)

	DELETE
		Data_Decimal
	OUTPUT
		DELETED.EntityId,
		DELETED.TenantId,
		DELETED.FieldId,
		DELETED.Data
	INTO
		@output
	FROM
		Data_Decimal a
	JOIN
		@data d
	ON
		a.EntityId = d.EntityId AND
		a.TenantId = d.TenantId AND
		a.FieldId = d.FieldId

	SELECT
		EntityId,
		TenantId,
		FieldId,
		Data
	FROM
		@output
END