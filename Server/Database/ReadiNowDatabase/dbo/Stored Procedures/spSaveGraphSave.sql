-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE PROCEDURE [dbo].[spSaveGraphSave]
	@tenantId BIGINT,	
	@inputEntities dbo.[InputEntityType] READONLY,
	@mergeAlias dbo.[Data_AliasType] READONLY,
	@mergeBit dbo.[Data_BitType] READONLY,				
	@mergeDateTime dbo.[Data_DateTimeType] READONLY,
	@mergeDecimal dbo.[Data_DecimalType] READONLY,
	@mergeGuid dbo.[Data_GuidType] READONLY,
	@mergeInt dbo.[Data_IntType] READONLY,
	@mergeNVarChar dbo.[Data_NVarCharType] READONLY,
	@mergeXml dbo.[Data_XmlType] READONLY,		
	@mergeRelationship dbo.[RelationshipType] READONLY,		
	@deleteAlias dbo.[FieldKeyType] READONLY,	
	@deleteBit dbo.[FieldKeyType] READONLY,						
	@deleteDateTime dbo.[FieldKeyType] READONLY,	
	@deleteDecimal dbo.[FieldKeyType] READONLY,	
	@deleteGuid dbo.[FieldKeyType] READONLY,	
	@deleteInt dbo.[FieldKeyType] READONLY,	
	@deleteNVarChar dbo.[FieldKeyType] READONLY,	
	@deleteXml dbo.[FieldKeyType] READONLY,			
	@deleteRelationship dbo.[RelationshipType] READONLY,			
	@deleteRelationshipForward dbo.[ForwardRelationshipType] READONLY,	
	@deleteRelationshipReverse dbo.[ReverseRelationshipType] READONLY,
	@shallow dbo.[EntityMapType] READONLY,
	@deep dbo.[EntityMapType] READONLY,
	@miscSql NVARCHAR(MAX) = NULL,
	@context VARCHAR(128) = NULL,
	@transactionId BIGINT OUTPUT
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

	DECLARE @entityIdMap TABLE (oldId BIGINT, [newId] BIGINT PRIMARY KEY(oldId, newId))
	DECLARE @mergeAliasMapped dbo.[Data_AliasType]
	DECLARE @mergeBitMapped dbo.[Data_BitType]
	DECLARE @mergeDateTimeMapped dbo.[Data_DateTimeType]
	DECLARE @mergeDecimalMapped dbo.[Data_DecimalType]
	DECLARE @mergeGuidMapped dbo.[Data_GuidType]
	DECLARE @mergeIntMapped dbo.[Data_IntType]
	DECLARE @mergeNVarCharMapped dbo.[Data_NVarCharType]
	DECLARE @mergeXmlMapped dbo.[Data_XmlType]
	DECLARE @mergeRelationshipMapped dbo.[RelationshipType]
	DECLARE @deleteAliasMapped dbo.[FieldKeyType]	
	DECLARE @deleteBitMapped dbo.[FieldKeyType]	
	DECLARE @deleteDateTimeMapped dbo.[FieldKeyType]	
	DECLARE @deleteDecimalMapped dbo.[FieldKeyType]	
	DECLARE @deleteGuidMapped dbo.[FieldKeyType]	
	DECLARE @deleteIntMapped dbo.[FieldKeyType]	
	DECLARE @deleteNVarCharMapped dbo.[FieldKeyType]	
	DECLARE @deleteXmlMapped dbo.[FieldKeyType]	
	DECLARE @deleteRelationshipMapped dbo.[RelationshipType]	
	DECLARE @deleteRelationshipForwardMapped dbo.[ForwardRelationshipType]	
	DECLARE @deleteRelationshipReverseMapped dbo.[ReverseRelationshipType]
	DECLARE @shallowMapped dbo.[EntityMapType]
	DECLARE @deepMapped dbo.[EntityMapType]

	DECLARE @haveMergeAlias BIT
	DECLARE @haveMergeBit BIT
	DECLARE @haveMergeDateTime BIT
	DECLARE @haveMergeDecimal BIT
	DECLARE @haveMergeGuid BIT
	DECLARE @haveMergeInt BIT
	DECLARE @haveMergeNVarChar BIT
	DECLARE @haveMergeXml BIT
	DECLARE @haveMergeRelationship BIT
	DECLARE @haveDeleteAlias BIT
	DECLARE @haveDeleteBit BIT
	DECLARE @haveDeleteDateTime BIT
	DECLARE @haveDeleteDecimal BIT
	DECLARE @haveDeleteGuid BIT
	DECLARE @haveDeleteInt BIT
	DECLARE @haveDeleteNVarChar BIT
	DECLARE @haveDeleteXml BIT
	DECLARE @haveDeleteRelationship BIT
	DECLARE @haveDeleteRelationshipForward BIT
	DECLARE @haveDeleteRelationshipReverse BIT
	DECLARE @haveShallow BIT
	DECLARE @haveDeep BIT	

	BEGIN TRY
		BEGIN TRANSACTION

		EXEC spCreateRestorePoint @tenantId, DEFAULT, DEFAULT, DEFAULT, DEFAULT, @transactionId OUTPUT

		-- Save query start					
			
		IF EXISTS (SELECT 1 FROM @inputEntities)
		BEGIN
			-- Create new entities and get new entity ids into the @entityIdMap table
			DECLARE @isOfType BIGINT = dbo.fnAliasNsId('isOfType', 'core', @tenantId)

			MERGE dbo.Entity AS e
			USING (SELECT DISTINCT Id FROM @inputEntities) AS s
			ON 0 = 1
			WHEN NOT MATCHED BY TARGET THEN
				INSERT(TenantId) VALUES(@tenantId)
			OUTPUT s.Id, INSERTED.Id INTO @entityIdMap;

			INSERT INTO	dbo.Relationship (TenantId, TypeId,	FromId,	ToId )
			SELECT @tenantId, @isOfType, o.[newId],	i.TypeId
			FROM @inputEntities i
			JOIN @entityIdMap o ON i.Id = o.oldId
			WHERE i.IsClone = 0 AND	i.TypeId <> -1
		END		
    
		-- Always returned mapped ids
		SELECT oldId, [newId] FROM @entityIdMap
    
		-- Id parameter mapping start    		

		IF EXISTS (SELECT 1 FROM @mergeAlias)
		BEGIN
			INSERT INTO @mergeAliasMapped
			SELECT ISNULL(m.newId, um.EntityId), um.TenantId, um.FieldId, um.Data,	um.Namespace, um.AliasMarkerId
			FROM @mergeAlias um	
			LEFT JOIN @entityIdMap m ON m.oldId = um.EntityId

			SET @haveMergeAlias = 1
		END

		IF EXISTS (SELECT 1 FROM @mergeBit)
		BEGIN
			INSERT INTO @mergeBitMapped
			SELECT ISNULL(m.newId, um.EntityId), um.TenantId, um.FieldId, um.Data 
			FROM @mergeBit um 
			LEFT JOIN @entityIdMap m ON m.oldId = um.EntityId

			SET @haveMergeBit = 1
		END

		IF EXISTS (SELECT 1 FROM @mergeDateTime)
		BEGIN
			INSERT INTO @mergeDateTimeMapped
			SELECT ISNULL(m.newId, um.EntityId), um.TenantId, um.FieldId, um.Data
			FROM @mergeDateTime um 
			LEFT JOIN @entityIdMap m ON m.oldId = um.EntityId

			SET @haveMergeDateTime = 1
		END

		IF EXISTS (SELECT 1 FROM @mergeDecimal)
		BEGIN
			INSERT INTO @mergeDecimalMapped
			SELECT ISNULL(m.newId, um.EntityId), um.TenantId, um.FieldId, um.Data
			FROM @mergeDecimal um 
			LEFT JOIN @entityIdMap m ON m.oldId = um.EntityId

			SET @haveMergeDecimal = 1
		END

		IF EXISTS (SELECT 1 FROM @mergeGuid)
		BEGIN
			INSERT INTO @mergeGuidMapped
			SELECT ISNULL(m.newId, um.EntityId), um.TenantId, um.FieldId, um.Data
			FROM @mergeGuid um 
			LEFT JOIN @entityIdMap m ON m.oldId = um.EntityId

			SET @haveMergeGuid = 1
		END

		IF EXISTS (SELECT 1 FROM @mergeInt)
		BEGIN
			INSERT INTO @mergeIntMapped
			SELECT ISNULL(m.newId, um.EntityId), um.TenantId, um.FieldId, um.Data
			FROM @mergeInt um 
			LEFT JOIN @entityIdMap m ON m.oldId = um.EntityId

			SET @haveMergeInt = 1
		END

		IF EXISTS (SELECT 1 FROM @mergeNVarChar)
		BEGIN
			INSERT INTO @mergeNVarCharMapped
			SELECT ISNULL(m.newId, um.EntityId), um.TenantId, um.FieldId, um.Data
			FROM @mergeNVarChar um 
			LEFT JOIN @entityIdMap m ON m.oldId = um.EntityId

			SET @haveMergeNVarChar = 1
		END

		IF EXISTS (SELECT 1 FROM @mergeXml)
		BEGIN
			INSERT INTO @mergeXmlMapped
			SELECT ISNULL(m.newId, um.EntityId), um.TenantId, um.FieldId, um.Data
			FROM @mergeXml um 
			LEFT JOIN @entityIdMap m ON m.oldId = um.EntityId

			SET @haveMergeXml = 1
		END

		IF EXISTS (SELECT 1 FROM @mergeRelationship)
		BEGIN
			INSERT INTO @mergeRelationshipMapped
			SELECT um.TenantId, um.TypeId, ISNULL(m1.newId, um.FromId), ISNULL(m2.newId, um.ToId)
			FROM @mergeRelationship um
			LEFT JOIN @entityIdMap m1 ON m1.oldId = um.FromId
			LEFT JOIN @entityIdMap m2 ON m2.oldId = um.ToId

			SET @haveMergeRelationship = 1
		END

		IF EXISTS (SELECT 1 FROM @deleteAlias)
		BEGIN
			INSERT INTO @deleteAliasMapped
			SELECT ISNULL(m.newId, um.EntityId), um.TenantId, um.FieldId            
			FROM @deleteAlias um 
			LEFT JOIN @entityIdMap m ON m.oldId = um.EntityId		

			SET @haveDeleteAlias = 1
		END

		IF EXISTS (SELECT 1 FROM @deleteBit)
		BEGIN
			INSERT INTO @deleteBitMapped
			SELECT ISNULL(m.newId, um.EntityId), um.TenantId, um.FieldId            
			FROM @deleteBit um
			LEFT JOIN @entityIdMap m ON m.oldId = um.EntityId

			SET @haveDeleteBit = 1
		END
		
		IF EXISTS (SELECT 1 FROM @deleteDateTime)
		BEGIN
			INSERT INTO @deleteDateTimeMapped
			SELECT ISNULL(m.newId, um.EntityId), um.TenantId, um.FieldId            
			FROM @deleteDateTime um
			LEFT JOIN @entityIdMap m ON m.oldId = um.EntityId	
			
			SET @haveDeleteDateTime = 1	
		END

		IF EXISTS (SELECT 1 FROM @deleteDecimal)
		BEGIN
			INSERT INTO @deleteDecimalMapped
			SELECT ISNULL(m.newId, um.EntityId), um.TenantId, um.FieldId            
			FROM @deleteDecimal um
			LEFT JOIN @entityIdMap m ON m.oldId = um.EntityId	
			
			SET @haveDeleteDecimal = 1	
		END

		IF EXISTS (SELECT 1 FROM @deleteGuid)
		BEGIN
			INSERT INTO @deleteGuidMapped
			SELECT ISNULL(m.newId, um.EntityId), um.TenantId, um.FieldId            
			FROM @deleteGuid um
			LEFT JOIN @entityIdMap m ON m.oldId = um.EntityId		

			SET @haveDeleteGuid = 1
		END

		IF EXISTS (SELECT 1 FROM @deleteInt)
		BEGIN
			INSERT INTO @deleteIntMapped
			SELECT ISNULL(m.newId, um.EntityId), um.TenantId, um.FieldId            
			FROM @deleteInt um
			LEFT JOIN @entityIdMap m ON m.oldId = um.EntityId		

			SET @haveDeleteInt = 1
		END

		IF EXISTS (SELECT 1 FROM @deleteNVarChar)
		BEGIN
			INSERT INTO @deleteNVarCharMapped
			SELECT ISNULL(m.newId, um.EntityId), um.TenantId, um.FieldId            
			FROM @deleteNVarChar um
			LEFT JOIN @entityIdMap m ON m.oldId = um.EntityId

			SET @haveDeleteNVarChar = 1
		END
		
		IF EXISTS (SELECT 1 FROM @deleteXml)
		BEGIN
			INSERT INTO @deleteXmlMapped
			SELECT ISNULL(m.newId, um.EntityId), um.TenantId, um.FieldId            
			FROM @deleteXml um
			LEFT JOIN @entityIdMap m ON m.oldId = um.EntityId		   
			
			SET @haveDeleteXml = 1 
		END

		IF EXISTS (SELECT 1 FROM @deleteRelationship)
		BEGIN
			INSERT INTO @deleteRelationshipMapped
			SELECT um.TenantId, um.TypeId, ISNULL(m1.newId, um.FromId), ISNULL(m2.newId, um.ToId)
			FROM @deleteRelationship um
			LEFT JOIN @entityIdMap m1 ON m1.oldId = um.FromId
			LEFT JOIN @entityIdMap m2 ON m2.oldId = um.ToId		

			SET @haveDeleteRelationship = 1
		END

		IF EXISTS (SELECT 1 FROM @deleteRelationshipForward)
		BEGIN
			INSERT INTO @deleteRelationshipForwardMapped
			SELECT um.TenantId, um.TypeId, ISNULL(m.newId, um.FromId)
			FROM @deleteRelationshipForward um
			LEFT JOIN @entityIdMap m ON m.oldId = um.FromId		

			SET @haveDeleteRelationshipForward = 1
		END

		IF EXISTS (SELECT 1 FROM @deleteRelationshipReverse)
		BEGIN
			INSERT INTO @deleteRelationshipReverseMapped
			SELECT um.TenantId, um.TypeId, ISNULL(m.newId, um.ToId)
			FROM @deleteRelationshipReverse um
			LEFT JOIN @entityIdMap m ON m.oldId = um.ToId

			SET @haveDeleteRelationshipReverse = 1
		END

		IF EXISTS (SELECT 1 FROM @shallow)
		BEGIN
			INSERT INTO @shallowMapped
			SELECT ISNULL(m1.newId, um.SourceId), ISNULL(m2.newId, um.DestinationId)
			FROM @shallow um
			LEFT JOIN @entityIdMap m1 ON m1.oldId = um.SourceId
			LEFT JOIN @entityIdMap m2 ON m2.oldId = um.DestinationId

			SET @haveShallow = 1
		END

		IF EXISTS (SELECT 1 FROM @deep)
		BEGIN
			INSERT INTO @deepMapped
			SELECT ISNULL(m1.newId, um.SourceId), ISNULL(m2.newId, um.DestinationId)
			FROM @deep um
			LEFT JOIN @entityIdMap m1 ON m1.oldId = um.SourceId
			LEFT JOIN @entityIdMap m2 ON m2.oldId = um.DestinationId

			SET @haveDeep = 1
		END
		    
		-- Id mapping end
    
		-- Save query start

		IF (@haveShallow = 1)
		BEGIN
			EXEC dbo.spCloneEntityShallow @shallowMapped, @tenantId
		END

		IF (@haveDeep = 1)
		BEGIN
			EXEC dbo.spCloneEntityDeep @deepMapped, @tenantId
		END

		IF (@haveDeleteAlias = 1)
		BEGIN
			EXEC dbo.spDeleteAlias @deleteAliasMapped
		END

		IF (@haveDeleteBit = 1)
		BEGIN
			EXEC dbo.spDeleteBit @deleteBitMapped
		END

		IF (@haveDeleteDateTime = 1)
		BEGIN
			EXEC dbo.spDeleteDateTime @deleteDateTimeMapped
		END

		IF (@haveDeleteDecimal = 1)
		BEGIN
			EXEC dbo.spDeleteDecimal @deleteDecimalMapped
		END
		
		IF (@haveDeleteGuid = 1)
		BEGIN
			EXEC dbo.spDeleteGuid @deleteGuidMapped
		END
		
		IF (@haveDeleteInt = 1)
		BEGIN
			EXEC dbo.spDeleteInt @deleteIntMapped
		END
		
		IF (@haveDeleteNVarChar = 1)
		BEGIN
			EXEC dbo.spDeleteNVarChar @deleteNVarCharMapped
		END
		
		IF (@haveDeleteXml = 1)
		BEGIN
			EXEC dbo.spDeleteXml @deleteXmlMapped
		END
		
		IF (@haveDeleteRelationship = 1)
		BEGIN
			EXEC dbo.spDeleteRelationship @deleteRelationshipMapped
		END
		
		IF (@haveDeleteRelationshipForward = 1)
		BEGIN
			EXEC dbo.spDeleteRelationshipForward @deleteRelationshipForwardMapped
		END
		
		IF (@haveDeleteRelationshipReverse = 1)
		BEGIN
			EXEC dbo.spDeleteRelationshipReverse @deleteRelationshipReverseMapped
		END
		
		IF (@haveMergeAlias = 1)
		BEGIN
			EXEC dbo.spMergeAlias @mergeAliasMapped		
		END

		IF (@haveMergeBit = 1)
		BEGIN
			EXEC dbo.spMergeBit @mergeBitMapped		
		END
		
		IF (@haveMergeDateTime = 1)
		BEGIN
			EXEC dbo.spMergeDateTime @mergeDateTimeMapped		
		END
		
		IF (@haveMergeDecimal = 1)
		BEGIN
			EXEC dbo.spMergeDecimal @mergeDecimalMapped		
		END
		
		IF (@haveMergeGuid = 1)
		BEGIN
			EXEC dbo.spMergeGuid @mergeGuidMapped		
		END
		
		IF (@haveMergeInt = 1)
		BEGIN
			EXEC dbo.spMergeInt @mergeIntMapped		
		END
		
		IF (@haveMergeNVarChar = 1)
		BEGIN
			EXEC dbo.spMergeNVarChar @mergeNVarCharMapped		
		END
		
		IF (@haveMergeXml = 1)
		BEGIN
			EXEC dbo.spMergeXml @mergeXmlMapped		
		END
		
		IF (@haveMergeRelationship = 1)
		BEGIN
			EXEC dbo.spMergeRelationship @mergeRelationshipMapped			
		END
				
		IF (@miscSql IS NOT NULL)
		BEGIN
			EXEC sp_executesql @miscSql
		END					
	
		COMMIT TRANSACTION		
		-- Save query end
	END TRY
	BEGIN CATCH
		ROLLBACK TRANSACTION;
		THROW;
	END CATCH
END