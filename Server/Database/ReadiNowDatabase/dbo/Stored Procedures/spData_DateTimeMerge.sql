-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE PROCEDURE [dbo].[spData_DateTimeMerge]
	@entityId BIGINT,
	@tenantId BIGINT,
	@fieldId BIGINT,
	@data DATETIME,
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
		Data DATETIME
	)
	
	MERGE
		Data_DateTime d
	USING (
		SELECT
			@entityId,
			@tenantId,
			@fieldId,
			@data )
		AS s (
			EntityId,
			TenantId,
			FieldId,
			Data )
	ON (
		d.EntityId = s.EntityId AND
		d.TenantId = s.TenantId AND
		d.FieldId = s.FieldId )
	WHEN MATCHED AND (
		d.Data <> s.Data )
	THEN
		UPDATE SET
			Data = @data
	WHEN NOT MATCHED
	THEN
		INSERT (
			EntityId,
			TenantId,
			FieldId,
			Data )
		VALUES (
			@entityId,
			@tenantId,
			@fieldId,
			@data )
	OUTPUT
		DELETED.EntityId,
		DELETED.TenantId,
		DELETED.FieldId,
		DELETED.Data
	INTO
		@output;

	SELECT
		EntityId,
		TenantId,
		FieldId,
		Data
	FROM
		@output
END