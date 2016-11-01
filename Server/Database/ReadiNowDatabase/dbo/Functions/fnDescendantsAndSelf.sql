-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE FUNCTION [dbo].[fnDescendantsAndSelf]
(	
	-- Add the parameters for the function here
	@relationTypeId bigint,
	@ancestorId bigint,
	@tenantId bigint
)
RETURNS TABLE
AS
RETURN  
(
	WITH derived ( Id, Depth, Path ) AS
	(
		SELECT
			@ancestorId AS Id,
			0 AS Depth,
			'|' + CONVERT(NVARCHAR(MAX), @ancestorId) + '|' AS Path

		UNION ALL

		SELECT
			r.FromId,
			b.Depth + 1,
			b.Path + CONVERT(NVARCHAR(MAX), r.FromId) + '|'
		FROM
			derived b
		JOIN
			Relationship r ON
				r.TenantId = @tenantId AND
				b.Id = r.ToId AND
				r.TypeId = @relationTypeId AND
				b.Path NOT LIKE ('%|' + CONVERT(NVARCHAR(MAX), r.FromId) + '|%')
	)
	SELECT DISTINCT
		Id
	FROM
		derived
)