-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE PROCEDURE [dbo].[spUpgradeAutoNumberInstance]
	@tenantId BIGINT,
	@context VARCHAR(128) = NULL
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
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

	DECLARE @autoNumber BIGINT = dbo.fnAliasNsId( 'autoNumberField', 'core', @tenantId )
	DECLARE @autoNumberValue BIGINT = dbo.fnAliasNsId( 'autoNumberValue', 'core', @tenantId )
	DECLARE @isOfType BIGINT = dbo.fnAliasNsId( 'isOfType', 'core', @tenantId )
	DECLARE @definition BIGINT = dbo.fnAliasNsId( 'definition', 'core', @tenantId )

	INSERT INTO
		Data_Int (
			EntityId,
			TenantId,
			FieldId,
			Data )
	SELECT
		i.FieldId,
		@tenantId,
		@autoNumberValue,
		i.Data
	FROM
		Data_Int i
	JOIN
		Relationship re ON
			i.EntityId = re.FromId
			AND i.TenantId = re.TenantId
			AND re.TypeId = @isOfType
			AND re.ToId = @definition
	JOIN
		Relationship rf ON
			i.FieldId = rf.FromId
			AND i.TenantId = rf.TenantId
			AND rf.TypeId = @isOfType
			AND rf.ToId = @autoNumber
	LEFT JOIN
		Data_Int ii ON
			i.FieldId = ii.EntityId
			AND i.TenantId = ii.TenantId
			AND @autoNumberValue = ii.FieldId
	WHERE
		i.TenantId = @tenantId
		AND ii.EntityId IS NULL

	DELETE
		Data_Int
	FROM
		Data_Int i
	JOIN
		Relationship re ON
			i.EntityId = re.FromId
			AND i.TenantId = re.TenantId
			AND re.TypeId = @isOfType
			AND re.ToId = @definition
	JOIN
		Relationship rf ON
			i.FieldId = rf.FromId
			AND i.TenantId = rf.TenantId
			AND rf.TypeId = @isOfType
			AND rf.ToId = @autoNumber
	WHERE
		i.TenantId = @tenantId
END