-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE PROCEDURE [dbo].[spData_NVarCharDelete]
	@entityId BIGINT,
	@tenantId BIGINT,
	@fieldId BIGINT,
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
		Data NVARCHAR(MAX)
	)

	DELETE FROM
		Data_NVarChar
	OUTPUT
		DELETED.EntityId,
		DELETED.TenantId,
		DELETED.FieldId,
		DELETED.Data
	INTO
		@output
	WHERE
		EntityId = @entityId AND
		TenantId = @tenantId AND
		FieldId = @fieldId

	SELECT
		EntityId,
		TenantId,
		FieldId,
		Data
	FROM
		@output
END