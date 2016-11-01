-- Copyright 2011-2016 Global Software Innovation Pty Ltd

-- Note: this function is used by ReadiMon DbHealth tests (only)
-- It is not safe for use with cycles.


CREATE FUNCTION [dbo].[tblFnRelationshipRecWithSelf] 
(
	@tenantId BIGINT = -1,
	@typeId BIGINT = -1,
	@fromId BIGINT = -1,
	@toId BIGINT = -1,
	@depth INT = -1
)
RETURNS 
TABLE 
AS
RETURN
(
	WITH Recursion (
		TenantId,
		TypeId,
		FromId,
		ToId,
		Depth
	)
	AS
	(
		SELECT
			TenantId,
			TypeId,
			FromId,
			ToId,
			Depth = 1
		FROM
			Relationship
		WHERE
			FromId <> ToId AND (
				@tenantId = -1 OR
				@tenantId = TenantId
			) AND (
				@typeId = -1 OR
				@typeId = TypeId
			)

		UNION ALL 
		
		SELECT
			r.TenantId,
			r.TypeId,
			r.FromId,
			rr.ToId,
			rr.Depth + 1
		FROM
			Recursion rr
		JOIN
			Relationship r ON
				r.ToId = rr.FromId AND
				r.TenantId = rr.TenantId AND
				r.TypeId = rr.TypeId
		WHERE
			r.FromId <> r.ToId AND
			r.ToId <> rr.ToId AND (
				@tenantId = -1 OR
				@tenantId = r.TenantId
			) AND (
				@typeId = -1 OR
				@typeId = r.TypeId
			)

	), Partitioning
	AS
	(
		SELECT
			*,
			ROW_NUMBER( )
		OVER (
			PARTITION BY
				TenantId,
				TypeId,
				FromId,
				Depth,
				ToId
			ORDER BY
				TenantId,
				TypeId,
				FromId,
				Depth,
				ToId
			) RowId
		FROM
			Recursion
	)
	SELECT
		*
	FROM
	(
		SELECT DISTINCT
			TenantId,
			TypeId,
			FromId,
			ToId,
			Depth
		FROM
			Partitioning
		WHERE
			RowId = 1 AND (
				@depth = -1 OR
				@depth = Depth
			)

		UNION

		SELECT DISTINCT
			r.TenantId,
			r.TypeId,
			e.Id,
			e.Id,
			0
		FROM
			Relationship r
		JOIN
			Entity e ON
				r.TenantId = e.TenantId
		WHERE
			@depth = -1 OR
			@depth = 0
	) a
	WHERE
		(
			@tenantId = -1 OR
			@tenantId = a.TenantId
		) AND (
			@typeId = -1 OR
			@typeId = a.TypeId
		) AND (
			@fromId = -1 OR
			@fromId = a.FromId
		) AND (
			@toId = -1 OR
			@toId = a.ToId
		)
)