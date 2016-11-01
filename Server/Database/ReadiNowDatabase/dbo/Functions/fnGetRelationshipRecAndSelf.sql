-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE FUNCTION [dbo].[fnGetRelationshipRecAndSelf]
(	
	@relationTypeId BIGINT,
	@tenantId BIGINT,
	@includeSelf BIT,
	@fromTypeId BIGINT = NULL,
	@toTypeId BIGINT = NULL
)
RETURNS TABLE
AS
RETURN
(
	WITH fromTypes ( Id ) AS
	(
		-- Get all the from type derived types.
		SELECT
			Id
		FROM
			dbo.fnDerivedTypes( ISNULL (@fromTypeId, -1 ), @tenantId )
		WHERE
			@includeSelf = 1				
	),	
	toTypes ( Id ) AS
	(
		-- Get all the to type derived types.
		-- Optimisation: Only do this if the from type is different to the to type.
		SELECT
			Id
		FROM
			dbo.fnDerivedTypes( ISNULL( @toTypeId, -1 ), @tenantId )
		WHERE @includeSelf = 1	
			AND ISNULL( @toTypeId, -1 ) != ISNULL( @fromTypeId, -1 )
	),
	fromInstances ( Id ) AS 
	(
		-- Get all the from type instances.
		SELECT
			e.Id
		FROM
			[Entity] e    
		WHERE
			@includeSelf = 1 AND
			e.TenantId = @tenantId AND EXISTS (
				SELECT
					1
				FROM
					[Relationship] r
				JOIN
					fromTypes dt ON dt.Id = r.ToId
				CROSS APPLY
					dbo.tblFnAliasId( 'isOfType', @tenantId ) a
				WHERE
					r.FromId = e.Id AND
					r.TenantId = @tenantId AND
					r.TypeId = a.EntityId
		)
    ),	
	toInstances ( Id ) AS 
	(
		-- Get all the to type instances.
		-- Optimisation: Only do this if the from type is different to the to type.
		SELECT
			e.Id
		FROM
			[Entity] e    
		WHERE
			@includeSelf = 1 AND
			ISNULL( @toTypeId, -1 ) != ISNULL( @fromTypeId, -1 ) AND
			e.TenantId = @tenantId AND EXISTS (
				SELECT
					1
				FROM
					[Relationship] r
				JOIN
					toTypes dt ON dt.Id = r.ToId
				CROSS APPLY
					dbo.tblFnAliasId( 'isOfType', @tenantId ) a
				WHERE
					r.FromId = e.Id AND
					r.TenantId = @tenantId AND
					r.TypeId = a.EntityId
		)
    ),	
	relRec ( FromId, ToId, Depth, Path ) AS
	(		
		SELECT
			f.Id AS FromId,
			f.Id AS ToId,
			0 AS Depth,
			'|' + CONVERT(NVARCHAR(MAX), f.Id) + '|' AS Path
		FROM
			fromInstances f
		WHERE
			@includeSelf = 1	

		UNION

		SELECT
			t.Id AS FromId,
			t.Id AS ToId,
			0 AS Depth,
			'|' + CONVERT(NVARCHAR(MAX), t.Id) + '|'
		FROM
			toInstances t
		WHERE
			@includeSelf = 1	
		
		UNION

		SELECT
			r.FromId,
			r.ToId,
			1 AS Depth,
			'|' + CONVERT(NVARCHAR(MAX), r.FromId) + '|'
		FROM
			Relationship r
		WHERE
			r.TenantId = @tenantId AND
			r.TypeId = @relationTypeId AND
			r.FromId <> r.ToId

		UNION ALL -- and now for the recursive part  

		SELECT
			rr.FromId,
			r.ToId,
			rr.Depth + 1,
			rr.Path + CONVERT(NVARCHAR(MAX), r.FromId) + '|'
		FROM
			relRec rr
		JOIN
			Relationship r ON
				rr.ToId = r.FromId AND
				r.TenantId = @tenantId AND
				r.TypeId = @relationTypeId AND
				r.FromId <> r.ToId
		WHERE
			rr.FromId <>  r.ToId
			AND rr.Path NOT LIKE ('%|' + CONVERT(NVARCHAR(MAX), r.ToId) + '|%')
	)
	SELECT DISTINCT
		FromId,
		ToId,
		Depth,
		@relationTypeId AS TypeId,
		@tenantId AS TenantId
	FROM
		relRec
)
GO

