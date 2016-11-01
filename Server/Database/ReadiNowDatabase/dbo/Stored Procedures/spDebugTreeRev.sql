-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE PROCEDURE [dbo].[spDebugTreeRev]
	-- Add the parameters for the stored procedure here
	@alias NVARCHAR ( MAX ),
	@tenantId BIGINT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	WITH tree( id, parentid, name )
	AS
	(
		SELECT
			ToId,
			FromId,
			n.Data
		FROM
			Relationship
		CROSS APPLY
			dbo.tblFnAliasNsId( @alias, 'core', @tenantId ) t
		OUTER APPLY
			dbo.tblFnFieldNVarCharA( ToId, @tenantId, 'name', 'core' ) n
		WHERE
			TenantId = @tenantId AND
			TypeId = t.EntityId

		UNION 

		SELECT DISTINCT
			FromId,
			0,
			n.Data
		FROM
			Relationship
		CROSS APPLY
			dbo.tblFnAliasNsId( @alias, 'core', @tenantId ) t
		OUTER APPLY
			dbo.tblFnFieldNVarCharA( FromId, @tenantId, 'name', 'core' ) n
		WHERE
			TypeId = t.EntityId AND
			FromId NOT IN (
				SELECT
					ToId
				FROM
					Relationship
				CROSS APPLY
					dbo.tblFnAliasNsId( @alias, 'core', @tenantId ) t
				WHERE
					TenantId = @tenantId AND
					TypeId = t.EntityId
			)

		UNION 

		SELECT
			0,
			NULL,
			'Root'
	),
	tree2 ( id, [path], [parent], name, depth )
	AS
	(
		SELECT
			id,
			CONVERT( VARCHAR( MAX ), '' ),
			CONVERT( VARCHAR( MAX ), NULL ),
			name,
			0
		FROM
			tree
		WHERE
			parentid IS NULL

	  UNION ALL

	  SELECT
		t1.id,
		t2.path + '.' + CONVERT( VARCHAR( MAX ), t1.id ),
		t2.path,
		t1.name,
		t2.depth + 1
	FROM
		tree2 t2
	JOIN
		tree t1 ON
			t1.parentid = t2.id
	),
	points ( x, y, name, [path], [parent] )
	AS
	(
		SELECT
			depth, 
			-row_number( )
		OVER (
			ORDER BY
				[path]
			),
			name,
			[path],
			parent
		FROM
			tree2
	),
	points2 ( x, y, name, yparent )
	AS
	(
		SELECT
			t.x,
			t.y,
			t.name,
			p.y
		FROM
			points t
		JOIN
			points p ON
				p.[path] = t.parent
	)
	SELECT
		geometry::STGeomFromText( 'LINESTRING(' +
		CONVERT( VARCHAR, x ) + ' ' + CONVERT( VARCHAR, y ) + ',' +
		CONVERT( VARCHAR, x ) + ' ' + CONVERT( VARCHAR, yparent ) + ')', 0 ),
		'' Name
	FROM
		points2

	UNION ALL

	SELECT
		geometry::STGeomFromText( 'LINESTRING(' +
		CONVERT( VARCHAR, x ) + ' ' + CONVERT( VARCHAR, y ) + ',' +
		CONVERT( VARCHAR, x + 5 ) + ' ' + CONVERT( VARCHAR, y ) + ')', 0 ),
		name Name
	FROM
		points2
END
