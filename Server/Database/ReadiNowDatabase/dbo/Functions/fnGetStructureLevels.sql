-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE FUNCTION [dbo].[fnGetStructureLevels]
(
	@TenantId bigint,
	@ResourceId bigint,
	@StructureViewId bigint,
	@StructureRootsRelationshipId bigint,
	@StructureMembershipRelationshipId bigint,
	@StructureHierarchyRelationshipId bigint,
	@Delimiter NVARCHAR( 1 ) = ',',
	@PathDelimiter NVARCHAR( 1 ) = '|'
)
	RETURNS NVARCHAR( MAX )
AS
BEGIN
	DECLARE @result NVARCHAR( MAX )

	-- Chain of relationships:
	-- resource --A--> intermediateNode --B--> rootNode --C--> structureView
	-- A=StructureMembershipRelationshipId
	-- B=StructureMembershipRelationshipId
	-- C=@StructureRootsRelationshipId	

	;WITH Paths( [Id], [Path] )
	AS (
        SELECT [Id] = svMemb.ToId,
			[Path] = CAST ( svMemb.ToId AS NVARCHAR( MAX ) )
		FROM Relationship svMemb
		WHERE svMemb.FromId = @ResourceId
		    AND svMemb.TenantId = @TenantId
		    AND svMemb.TypeId = @StructureMembershipRelationshipId
		    		

		UNION ALL

		SELECT [Id] = svTree.ToId,
			[Path] = CAST ( CAST( svTree.ToId AS NVARCHAR( MAX ) ) + @Delimiter + p.[Path] AS NVARCHAR( MAX ) )
		FROM Relationship svTree
		INNER JOIN Paths p ON svTree.FromId = p.Id
		WHERE svTree.TypeId = @StructureHierarchyRelationshipId
		    AND svTree.TenantId = @TenantId

	)
	SELECT @result = COALESCE( @result + @PathDelimiter, '' ) + [Path]
	FROM Paths
	INNER JOIN Relationship svRoots ON svRoots.FromId = Paths.Id
	    WHERE svRoots.TypeId = @StructureRootsRelationshipId
	        AND svRoots.TenantId = @TenantId
		    AND svRoots.ToId = @StructureViewId


	RETURN( @result )
END
