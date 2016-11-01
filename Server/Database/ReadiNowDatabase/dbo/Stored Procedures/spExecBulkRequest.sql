-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE PROCEDURE [dbo].[spExecBulkRequest]
	@tenantId BIGINT,	
	@entityId BIGINT = NULL,
	@entityIds [dbo].[UniqueIdListType] READONLY,
	@relFwd [dbo].[BulkRelType] READONLY,
	@relRev [dbo].[BulkRelType] READONLY,	
	@fldData_Alias [dbo].[BulkFldType] READONLY,	
	@fldData_NVarChar [dbo].[BulkFldType] READONLY,	
	@fldData_Bit [dbo].[BulkFldType] READONLY,	
	@fldData_Int [dbo].[BulkFldType] READONLY,	
	@fldData_Decimal [dbo].[BulkFldType] READONLY,	
	@fldData_DateTime [dbo].[BulkFldType] READONLY,	
	@fldData_Guid [dbo].[BulkFldType] READONLY,	
	@fldData_Xml [dbo].[BulkFldType] READONLY	
AS
BEGIN
	SET NOCOUNT ON	

	DECLARE @haveRelFwd BIT
	DECLARE @haveRelRev BIT
	DECLARE @haveRel BIT	

	CREATE TABLE #process (
		EntityId BIGINT NOT NULL,
		NodeTag INT NOT NULL,
		State INT NOT NULL,
		RelSrcId BIGINT NOT NULL,
		RelTypeId BIGINT NOT NULL
	)
	ALTER TABLE #process ADD PRIMARY KEY ( EntityId, NodeTag, RelSrcId, RelTypeId )

	IF EXISTS (SELECT 1 FROM @entityIds)
	BEGIN
		INSERT INTO #process 
		SELECT t.Id, 0, 1, 0, 0 FROM @entityIds t
		JOIN dbo.Entity e ON e.Id = t.Id WHERE e.TenantId = @tenantId
	END

	IF (@entityId IS NOT NULL)
	BEGIN
		INSERT INTO #process 
		SELECT e.Id, 0, 1, 0, 0 FROM dbo.Entity e
		WHERE e.Id = @entityId AND e.TenantId = @tenantId
	END
	
	IF EXISTS (SELECT 1 FROM @relFwd)
	BEGIN
		SET @haveRelFwd = 1
		SET @haveRel = 1
	END

	IF EXISTS (SELECT 1 FROM @relRev)
	BEGIN
		SET @haveRelRev = 1
		SET @haveRel = 1
	END

	IF (@haveRel = 1)
	BEGIN
		DECLARE @count INT = 1
		WHILE @count > 0
		BEGIN
			SET @count = 0
			IF (@haveRelFwd = 1)
			BEGIN
				INSERT INTO #process ( EntityId, NodeTag, State, RelSrcId, RelTypeId )
					SELECT DISTINCT r.ToId, t.NextTag, 0, r.FromId, r.TypeId
						FROM #process p
						INNER LOOP JOIN @relFwd t
						ON p.NodeTag = t.NodeTag
						INNER LOOP JOIN dbo.Relationship r ON p.EntityId = r.FromId AND t.RelTypeId = r.TypeId AND r.TenantId = @tenantId
						WHERE p.State = 1
						AND NOT EXISTS ( SELECT 1 FROM #process x WHERE r.ToId = x.EntityId AND t.NextTag = x.NodeTag AND r.TypeId = x.RelTypeId AND r.FromId = x.RelSrcId )
				SET @count = @count + @@ROWCOUNT
			END
			IF (@haveRelRev = 1)
			BEGIN
				INSERT INTO #process ( EntityId, NodeTag, State, RelSrcId, RelTypeId )
					SELECT DISTINCT r.FromId, t.NextTag, 0, r.ToId, -r.TypeId
						FROM #process p
						INNER LOOP JOIN @relRev t
						ON p.NodeTag = t.NodeTag
						INNER LOOP JOIN dbo.Relationship r ON p.EntityId = r.ToId AND t.RelTypeId = r.TypeId AND r.TenantId = @tenantId
						WHERE p.State = 1
						AND NOT EXISTS ( SELECT 1 FROM #process x WHERE r.FromId = x.EntityId AND t.NextTag = x.NodeTag AND -r.TypeId = x.RelTypeId AND r.ToId = x.RelSrcId )
				SET @count = @count + @@ROWCOUNT
			END
			UPDATE #process SET State = State + 1 WHERE State < 2
		END
	END

	SELECT EntityId, NodeTag, RelSrcId, RelTypeId FROM #process

	IF EXISTS (SELECT 1 FROM @fldData_Alias)
	BEGIN
		SELECT DISTINCT d.EntityId, d.FieldId, d.Data, d.Namespace
		FROM @fldData_Alias t
		JOIN #process p ON p.NodeTag = t.NodeTag
		JOIN dbo.Data_Alias d ON p.EntityId = d.EntityId AND t.FieldId = d.FieldId AND d.TenantId = @tenantId
	END

	IF EXISTS (SELECT 1 FROM @fldData_NVarChar)
	BEGIN
		SELECT DISTINCT d.EntityId, d.FieldId, d.Data
		FROM @fldData_NVarChar t
		JOIN #process p ON p.NodeTag = t.NodeTag
		JOIN dbo.Data_NVarChar d ON p.EntityId = d.EntityId AND t.FieldId = d.FieldId AND d.TenantId = @tenantId
	END

	IF EXISTS (SELECT 1 FROM @fldData_Bit)
	BEGIN
		SELECT DISTINCT d.EntityId, d.FieldId, d.Data
		FROM @fldData_Bit t
		JOIN #process p ON p.NodeTag = t.NodeTag
		JOIN dbo.Data_Bit d ON p.EntityId = d.EntityId AND t.FieldId = d.FieldId AND d.TenantId = @tenantId
	END

	IF EXISTS (SELECT 1 FROM @fldData_Int)
	BEGIN
		SELECT DISTINCT d.EntityId, d.FieldId, d.Data
		FROM @fldData_Int t
		JOIN #process p ON p.NodeTag = t.NodeTag
		JOIN dbo.Data_Int d ON p.EntityId = d.EntityId AND t.FieldId = d.FieldId AND d.TenantId = @tenantId
	END

	IF EXISTS (SELECT 1 FROM @fldData_Decimal)
	BEGIN
		SELECT DISTINCT d.EntityId, d.FieldId, d.Data
		FROM @fldData_Decimal t
		JOIN #process p ON p.NodeTag = t.NodeTag
		JOIN dbo.Data_Decimal d ON p.EntityId = d.EntityId AND t.FieldId = d.FieldId AND d.TenantId = @tenantId
	END

	IF EXISTS (SELECT 1 FROM @fldData_DateTime)
	BEGIN
		SELECT DISTINCT d.EntityId, d.FieldId, d.Data
		FROM @fldData_DateTime t
		JOIN #process p ON p.NodeTag = t.NodeTag
		JOIN dbo.Data_DateTime d ON p.EntityId = d.EntityId AND t.FieldId = d.FieldId AND d.TenantId = @tenantId
	END

	IF EXISTS (SELECT 1 FROM @fldData_Guid)
	BEGIN
		SELECT DISTINCT d.EntityId, d.FieldId, d.Data
		FROM @fldData_Guid t
		JOIN #process p ON p.NodeTag = t.NodeTag
		JOIN dbo.Data_Guid d ON p.EntityId = d.EntityId AND t.FieldId = d.FieldId AND d.TenantId = @tenantId
	END

	IF EXISTS (SELECT 1 FROM @fldData_Xml)
	BEGIN
		SELECT DISTINCT d.EntityId, d.FieldId, d.Data
		FROM @fldData_Xml t
		JOIN #process p ON p.NodeTag = t.NodeTag
		JOIN dbo.Data_Xml d ON p.EntityId = d.EntityId AND t.FieldId = d.FieldId AND d.TenantId = @tenantId
	END

	DROP TABLE #process
END