-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE FUNCTION [dbo].[fnAncestorsAndSelf]
(	
	@relationTypeId bigint,
	@descendantId bigint,
	@tenantId bigint
)
RETURNS TABLE
AS
RETURN 
(
	WITH Ancestors ( Id, Depth, Path ) AS
	(
		SELECT
			@descendantId AS Id,
			0 AS Depth,
			'|' + CONVERT(NVARCHAR(MAX), @descendantId) + '|' AS Path

		UNION ALL -- and now for the recursive part  

		SELECT
			r.ToId,
			b.Depth + 1,
			b.Path + CONVERT(NVARCHAR(MAX), r.ToId) + '|'
		FROM
			Ancestors b
		JOIN
			Relationship r ON
				b.Id = r.FromId AND
				r.TenantId = @tenantId AND
				r.TypeId = @relationTypeId AND
				b.Path NOT LIKE ('%|' + CONVERT(NVARCHAR(MAX), r.ToId) + '|%')
	)
	SELECT DISTINCT
		Id,
		Depth
	FROM
		Ancestors
)