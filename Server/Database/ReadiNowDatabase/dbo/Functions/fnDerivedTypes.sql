-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE FUNCTION [dbo].[fnDerivedTypes]
(	
	@typeId BIGINT,
	@tenantId BIGINT
)
RETURNS TABLE
AS
RETURN
(
	WITH derived ( Id, Depth, Path ) AS
	(
		SELECT
			@typeId,
			0 AS Depth,
			'|' + CONVERT(NVARCHAR(MAX), @typeId) + '|' AS Path
			
		UNION ALL

		SELECT
			r.FromId,
			d.Depth + 1,
			d.Path + CONVERT(NVARCHAR(MAX), r.FromId) + '|'
		FROM
			derived d
		JOIN
			Relationship r ON
				r.TenantId = @tenantId AND
				d.Id = r.ToId
				AND d.Path NOT LIKE ('%|' + CONVERT(NVARCHAR(MAX), r.FromId) + '|%')
		INNER JOIN
			dbo.tblFnAliasId( 'inherits', @tenantId ) a ON
				r.TypeId = a.EntityId
	)
	SELECT DISTINCT
		Id
	FROM
		derived
)