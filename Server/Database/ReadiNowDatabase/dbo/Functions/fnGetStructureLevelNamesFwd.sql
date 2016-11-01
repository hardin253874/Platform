-- Copyright 2011-2016 Global Software Innovation Pty Ltd
-- Get structure level names following the hierarchy relationship in the forward direction
CREATE FUNCTION [dbo].[fnGetStructureLevelNamesFwd]
(
	@TenantId BIGINT,
	@ResourceId BIGINT,
	@StructureViewId BIGINT,
	@StructureRootsRelationshipId BIGINT,	
	@StructureHierarchyRelationshipId BIGINT,	
	@NameFieldId BIGINT,
	@DetectRoots BIT = 0,
	@Delimiter NVARCHAR( 1 ) = ',',
	@PathDelimiter NVARCHAR( 1 ) = '|',
	@Depth INT = 2,
	@ForSorting BIT = 0
)
	RETURNS NVARCHAR( MAX )
AS
BEGIN
	DECLARE @result NVARCHAR( MAX )	

	IF (@DetectRoots = 1)
	BEGIN
		;WITH RootNodes (Id)
		AS (
			-- Any optional root nodes for this structure view
			SELECT svRoots.FromId FROM Relationship svRoots
			WHERE svRoots.TenantId = @TenantId
			  AND svRoots.TypeId = @StructureRootsRelationshipId	        
			  AND svRoots.ToId = @StructureViewId		  
		),
		Paths( [Id], [Name], [Depth], [PathIds] )
		AS (
			-- The anchor node
			SELECT [Id] = e.Id,
				[Name] = CASE 
							WHEN @ForSorting = 0 THEN CAST ( svName.EntityId AS NVARCHAR( MAX ) ) + ':'
							WHEN @ForSorting = 1 THEN ''
						 END + svName.Data,
				[Depth] = 0,
				[PathIds] = ',' + CAST ( svName.EntityId AS NVARCHAR( MAX ) ) + ','
			FROM Entity e
			INNER JOIN Data_NVarChar svName ON e.Id = svName.EntityId AND svName.TenantId = @TenantId AND svName.FieldId = @NameFieldId
			WHERE e.Id = @ResourceId
				AND e.TenantId = @TenantId	
		    		
			UNION ALL

			-- Recursive join
			SELECT [Id] = svTree.ToId,				
				[Name] = CASE 
							WHEN @ForSorting = 0 THEN CAST ( svName.EntityId AS NVARCHAR( MAX ) ) + ':'
							WHEN @ForSorting = 1 THEN ''
						 END + svName.Data + @Delimiter + p.Name,
				[Depth] = p.Depth + 1,
				[PathIds] = ',' + CAST ( svName.EntityId AS NVARCHAR( MAX ) ) + p.[PathIds]
			FROM Relationship svTree
			INNER JOIN Paths p ON svTree.FromId = p.Id
			JOIN Data_NVarChar svName ON svTree.ToId = svName.EntityId AND svName.TenantId = @TenantId AND svName.FieldId = @NameFieldId
			WHERE svTree.TypeId = @StructureHierarchyRelationshipId
				AND svTree.TenantId = @TenantId
				AND CHARINDEX( ',' + CAST ( svTree.ToId AS NVARCHAR( MAX ) ) + ',', p.[PathIds] ) = 0
		)
		SELECT @result = COALESCE( @result + @PathDelimiter, '' ) + p.[Name]
		FROM Paths p
		LEFT JOIN RootNodes svRoots ON svRoots.Id = p.Id
		WHERE (svRoots.Id IS NOT NULL OR NOT EXISTS (SELECT Id FROM RootNodes))
			AND (@Depth IS NULL OR p.Depth < @Depth)
	END
	ELSE
	BEGIN
		;WITH Paths( [Id], [Name], [Depth], [PathIds] )
		AS (
			-- The anchor node
			SELECT [Id] = e.Id,				
				[Name] = CASE 
							WHEN @ForSorting = 0 THEN CAST ( svName.EntityId AS NVARCHAR( MAX ) ) + ':'
							WHEN @ForSorting = 1 THEN ''
						 END + svName.Data,
				[Depth] = 0,
				[PathIds] = ',' + CAST ( svName.EntityId AS NVARCHAR( MAX ) ) + ','
			FROM Entity e
			INNER JOIN Data_NVarChar svName ON e.Id = svName.EntityId AND svName.TenantId = @TenantId AND svName.FieldId = @NameFieldId
			WHERE e.Id = @ResourceId
				AND e.TenantId = @TenantId	
		    		
			UNION ALL

			-- Recursive join
			SELECT [Id] = svTree.ToId,				
				[Name] = CASE 
							WHEN @ForSorting = 0 THEN CAST ( svName.EntityId AS NVARCHAR( MAX ) ) + ':'
							WHEN @ForSorting = 1 THEN ''
						 END + svName.Data + @Delimiter + p.Name,
				[Depth] = p.Depth + 1,
				[PathIds] = ',' + CAST ( svName.EntityId AS NVARCHAR( MAX ) ) + p.[PathIds]
			FROM Relationship svTree
			INNER JOIN Paths p ON svTree.FromId = p.Id
			JOIN Data_NVarChar svName ON svTree.ToId = svName.EntityId AND svName.TenantId = @TenantId AND svName.FieldId = @NameFieldId
			WHERE svTree.TypeId = @StructureHierarchyRelationshipId
				AND svTree.TenantId = @TenantId
				AND CHARINDEX( ',' + CAST ( svTree.ToId AS NVARCHAR( MAX ) ) + ',', p.[PathIds] ) = 0
		)
		SELECT @result = COALESCE( @result + @PathDelimiter, '' ) + p.[Name]
		FROM Paths p		
		WHERE @Depth IS NULL OR p.Depth < @Depth
	END
	
	IF @ForSorting = 1
	BEGIN
		SET @result = dbo.fnGetSortedStructureViewPath(@result, @Delimiter, @PathDelimiter)
	END

	RETURN( @result )
END
