-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE PROCEDURE [dbo].[spData_AliasMergeMany]
	@data dbo.Data_AliasType READONLY,
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
		Namespace NVARCHAR(100),
		Data NVARCHAR(100)
	)

	MERGE
		Data_Alias d
	USING (
		SELECT
			v.EntityId,
			v.TenantId,
			v.FieldId,
			v.Data,
			v.Namespace,
			CASE WHEN
				a.Data = 'reverseAlias' AND
				a.Namespace = 'core'
			THEN
				1
			ELSE
				0
			END
		FROM
			@data v
		JOIN
			Data_Alias a
		ON
			a.EntityId = v.FieldId AND a.TenantId = v.TenantId )
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
			Namespace = s.Namespace,
			Data = s.Data
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
			s.EntityId,
			s.TenantId,
			s.FieldId,
			s.Data,
			s.Namespace,
			s.AliasMarkerId )
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