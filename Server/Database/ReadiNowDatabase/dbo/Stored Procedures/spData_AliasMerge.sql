-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE PROCEDURE [dbo].[spData_AliasMerge]
	@entityId BIGINT,
	@tenantId BIGINT,
	@fieldId BIGINT,
	@namespace NVARCHAR( 100 ),
	@data NVARCHAR( 100 ),
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

	DECLARE @aliasMarkerId INT

	SELECT
		@aliasMarkerId = (
			CASE WHEN
				Data = 'reverseAlias' AND
				Namespace = 'core'
			THEN
				1
			ELSE
				0
			END )
	FROM
		Data_Alias
	WHERE
		EntityId = @fieldId AND
		TenantId = @tenantId

	DECLARE @output TABLE
	(
		EntityId BIGINT,
		TenantId BIGINT,
		FieldId BIGINT,
		Namespace NVARCHAR(100),
		Data NVARCHAR(100)
	)

	MERGE
		Data_Alias d
	USING (
		SELECT
			@entityId,
			@tenantId,
			@fieldId,
			@data,
			@namespace,
			@aliasMarkerId )
		AS s (
			EntityId,
			TenantId,
			FieldId,
			Data,
			Namespace,
			AliasMarkerId )
	ON (
		d.EntityId = s.EntityId AND
		d.TenantId = s.TenantId AND
		d.FieldId = s.FieldId )
	WHEN MATCHED AND (
		d.Namespace <> s.Namespace OR
		d.Data <> s.Data )
	THEN
		UPDATE SET
			Namespace = @namespace,
			Data = @data
	WHEN NOT MATCHED
	THEN
		INSERT (
			EntityId,
			TenantId,
			FieldId,
			Data,
			Namespace,
			AliasMarkerId )
		VALUES (
			@entityId,
			@tenantId,
			@fieldId,
			@data,
			@namespace,
			@aliasMarkerId )
	OUTPUT
		DELETED.EntityId,
		DELETED.TenantId,
		DELETED.FieldId,
		DELETED.Namespace,
		DELETED.Data
	INTO
		@output;

	SELECT
		EntityId,
		TenantId,
		FieldId,
		Namespace,
		Data
	FROM
		@output
END