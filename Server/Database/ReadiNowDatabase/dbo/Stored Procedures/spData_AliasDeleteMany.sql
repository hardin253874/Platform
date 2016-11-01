-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE PROCEDURE [dbo].[spData_AliasDeleteMany]
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
		Namespace NVARCHAR(100),
		Data NVARCHAR(100),
		AliasMarkerId INT
	)

	DELETE
		Data_Alias
	OUTPUT
		DELETED.EntityId,
		DELETED.TenantId,
		DELETED.FieldId,
		DELETED.Namespace,
		DELETED.Data,
		DELETED.AliasMarkerId
	INTO
		@output
	FROM
		Data_Alias a
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
		Namespace,
		Data,
		AliasMarkerId
	FROM
		@output
END