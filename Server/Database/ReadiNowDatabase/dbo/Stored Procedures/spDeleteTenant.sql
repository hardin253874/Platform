-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE PROCEDURE [dbo].[spDeleteTenant]
	@tenantId BIGINT,
	@context VARCHAR( 128 ) = NULL
AS
BEGIN

	SET NOCOUNT ON

	IF ( @context IS NULL )
	BEGIN
		SET @context = OBJECT_NAME(@@PROCID)
	END

	IF ( @context IS NOT NULL )
	BEGIN
		DECLARE @contextInfo VARBINARY(128) = CONVERT( VARBINARY(128), @context )
		SET CONTEXT_INFO @contextInfo
	END

	-- Delete data fields
	DECLARE @sql NVARCHAR( MAX ) = '
	IF (
		SELECT
			COUNT( * )
		FROM
			?
		WHERE
			TenantId = ' + CAST( @tenantId AS NVARCHAR( MAX ) ) + '
		) > 4500
	BEGIN
		DELETE FROM
			? WITH (PAGLOCK)
		WHERE
			TenantId = ' +  CAST( @tenantId AS NVARCHAR( MAX ) ) + '
	END
	ELSE
	BEGIN
		DELETE FROM
			?
		WHERE
			TenantId = ' + CAST( @tenantId AS NVARCHAR( MAX ) ) + '
	END

	DELETE FROM
		?
	WHERE
		TenantId = 0 AND
		EntityId = ' +  CAST( @tenantId AS NVARCHAR( MAX ) )

	EXEC spExecForDataTables @sql
	
	-- Delete relationships
	IF (
		SELECT
			COUNT( * )
		FROM
			Relationship
		WHERE
			TenantId = @tenantId
		) > 4500
	BEGIN
		DELETE FROM
			Relationship WITH ( PAGLOCK )
		WHERE
			TenantId = @tenantId
	END
	ELSE
	BEGIN
		DELETE FROM
			Relationship
		WHERE
			TenantId = @tenantId
	END

	DELETE FROM
		Relationship
	WHERE
		TenantId = 0 AND
		TypeId = @tenantId

	DELETE FROM
		Relationship
	WHERE
		TenantId = 0 AND
		FromId = @tenantId

	DELETE FROM
		Relationship
	WHERE
		TenantId = 0 AND
		ToId = @tenantId

	-- Delete entities
	IF (
		SELECT
			COUNT( * )
		FROM
			Entity
		WHERE
			TenantId = @tenantId
		) > 4500
	BEGIN
		DELETE FROM
			Entity WITH (PAGLOCK)
		WHERE
			TenantId = @tenantId
	END
	ELSE
	BEGIN
		DELETE FROM
			Entity
		WHERE
			TenantId = @tenantId
	END

	DELETE FROM
		Entity
	WHERE
		TenantId = 0 AND
		Id = @tenantId

	-- Delete deploy tables
	DELETE FROM
		AppDeploy_Field
	WHERE
		TenantId = @tenantId

	DELETE FROM
		AppDeploy_Relationship
	WHERE
		TenantId = @tenantId
	
END