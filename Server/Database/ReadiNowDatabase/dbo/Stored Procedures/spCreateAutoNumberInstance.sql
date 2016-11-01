CREATE PROCEDURE [dbo].[spCreateAutoNumberInstance]
	@entityId BIGINT,
	@fieldId BIGINT,
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

    DECLARE @newId INT
	DECLARE @autoNumberValue BIGINT = dbo.fnAliasNsId( 'autoNumberValue', 'core', @tenantId )
	DECLARE @autoNumberSeed BIGINT = dbo.fnAliasNsId( 'autoNumberSeed', 'core', @tenantId )

	IF EXISTS ( SELECT * FROM Data_Int WITH (SERIALIZABLE) WHERE EntityId = @fieldId AND FieldId = @autoNumberValue AND TenantId = @tenantId )
	BEGIN
		SELECT @newId = Data + 1 FROM Data_Int WHERE EntityId = @fieldId AND FieldId = @autoNumberValue AND TenantId = @tenantId

		UPDATE Data_Int SET Data = @newId WHERE EntityId = @fieldId AND FieldId = @autoNumberValue AND TenantId = @tenantId
	END
	ELSE
	BEGIN
		SET @newId = 1
	
		DECLARE @seed INT
	
		SELECT @seed = Data FROM Data_Int WHERE EntityId = @fieldId AND FieldId = @autoNumberSeed AND TenantId = @tenantId

		SET @newId = ISNULL( @seed, @newId )	

		INSERT INTO Data_Int ( EntityId, TenantId, FieldId, Data )
		SELECT @fieldId, @tenantId, @autoNumberValue, @newId
	END

	IF EXISTS( SELECT * FROM Data_Int WITH (SERIALIZABLE) WHERE EntityId = @entityId AND FieldId = @fieldId AND TenantId = @tenantId )
	BEGIN
		UPDATE Data_Int SET Data = @newId WHERE EntityId = @entityId AND FieldId = @fieldId AND TenantId = @tenantId
	END
	ELSE
	BEGIN
		INSERT INTO Data_Int ( EntityId, TenantId, FieldId, Data )
		SELECT @entityId, @tenantId, @fieldId, @newId
	END

	SELECT @newId
END