CREATE PROCEDURE [dbo].[spUpdateAutoNumberInstance]
	@entityId BIGINT,
	@fieldId BIGINT,
	@tenantId BIGINT,
	@context VARCHAR(128) = NULL
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    DECLARE @newId INT
	DECLARE @autoNumberValue BIGINT = dbo.fnAliasNsId( 'autoNumberValue', 'core', @tenantId )
	DECLARE @autoNumberSeed BIGINT = dbo.fnAliasNsId( 'autoNumberSeed', 'core', @tenantId )

	SELECT @newId = Data FROM Data_Int WITH (UPDLOCK, SERIALIZABLE) WHERE EntityId = @entityId AND FieldId = @fieldId AND TenantId = @tenantId

	IF ( @newId IS NULL )
	BEGIN
		SELECT @newId = Data FROM Data_Int WITH (UPDLOCK, SERIALIZABLE) WHERE EntityId = @fieldId AND FieldId = @autoNumberValue AND TenantId = @tenantId
	
		IF ( @newId IS NULL )
		BEGIN
			SET @newId = 1
		
			DECLARE @seed INT
		
			SELECT @seed = Data FROM Data_Int WHERE EntityId = @fieldId AND FieldId = @autoNumberSeed AND TenantId = @tenantId

			SET @newId = ISNULL( @seed, @newId )	

			INSERT INTO Data_Int ( EntityId, TenantId, FieldId, Data )
			SELECT @fieldId, @tenantId, @autoNumberValue, @newId
		END
		ELSE
		BEGIN
			SET @newId = @newId + 1
		
			UPDATE Data_Int SET Data = @newId WHERE EntityId = @fieldId AND FieldId = @autoNumberValue AND TenantId = @tenantId
		END
	
		INSERT INTO Data_Int ( EntityId, TenantId, FieldId, Data )
		SELECT @entityId, @tenantId, @fieldId, @newId
	END

	SELECT @newId
END