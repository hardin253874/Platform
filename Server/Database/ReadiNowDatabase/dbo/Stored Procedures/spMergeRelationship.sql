-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE PROCEDURE [dbo].[spMergeRelationship]
	@data dbo.RelationshipType READONLY
AS
BEGIN

	SET NOCOUNT ON;

	DECLARE @typeId BIGINT = NULL
	DECLARE @fromId BIGINT = NULL
	DECLARE @toId BIGINT = NULL
	DECLARE @errorMessage NVARCHAR( MAX ) = NULL

	SELECT
		@typeId = v.TypeId,
		@fromId = CASE WHEN
					c.ToId IS NOT NULL AND (
						c.ToId = oneToOne.EntityId AND
						(
							fwd.Count <> 0 OR
							rev.Count <> 0
						)
					) OR (
						c.ToId = manyToOne.EntityId AND
						fwd.Count <> 0
					)
				THEN
					v.FromId
				ELSE
					v.ToId
				END,
		@toId = CASE WHEN
					c.ToId IS NOT NULL AND (
						c.ToId = oneToOne.EntityId AND
						(
							fwd.Count <> 0 OR
							rev.Count <> 0
						)
					) OR (
						c.ToId = manyToOne.EntityId AND
						fwd.Count <> 0
					)
				THEN
					v.ToId
				ELSE
					v.FromId
				END
	FROM
		@data v
	LEFT JOIN
		dbo.Relationship r
	ON
		r.TenantId = v.TenantId AND
		r.TypeId = v.TypeId AND
		r.FromId = v.FromId AND
		r.ToId = v.ToId
	CROSS APPLY
		dbo.tblFnAliasNsId( 'cardinality', 'core', v.TenantId ) cardinality
	CROSS APPLY
		dbo.tblFnAliasNsId( 'oneToOne', 'core', v.TenantId ) oneToOne
	CROSS APPLY
		dbo.tblFnAliasNsId( 'oneToMany', 'core', v.TenantId ) oneToMany
	CROSS APPLY
		dbo.tblFnAliasNsId( 'manyToOne', 'core', v.TenantId ) manyToOne
	CROSS APPLY
		dbo.tblFnAliasNsId( 'manyToMany', 'core', v.TenantId ) manyToMany
	LEFT JOIN
		dbo.Relationship c
	ON
		v.TypeId = c.FromId AND
		v.TenantId = c.TenantId AND
		c.TypeId = cardinality.EntityId
	LEFT JOIN (
		SELECT
			rFwd.TenantId,
			rFwd.TypeId,
			rFwd.FromId,
			Count = COUNT ( rFwd.ToId )
		FROM
			dbo.Relationship rFwd
		GROUP BY
			rFwd.TenantId,
			rFwd.TypeId,
			rFwd.FromId ) fwd
	ON
		v.TenantId = fwd.TenantId AND
		v.TypeId = fwd.TypeId AND
		v.FromId = fwd.FromId
	LEFT JOIN (
		SELECT
			rRev.TenantId,
			rRev.TypeId,
			rRev.ToId,
			Count = COUNT ( rRev.FromId )
		FROM
			dbo.Relationship rRev
		GROUP BY
			rRev.TenantId,
			rRev.TypeId,
			rRev.ToId ) rev
	ON
		v.TenantId = rev.TenantId AND
		v.TypeId = rev.TypeId AND
		v.ToId = rev.ToId
	WHERE
		r.TenantId IS NULL AND
		c.ToId IS NOT NULL AND
		(
			(
				c.ToId = oneToOne.EntityId AND
				(
					fwd.Count <> 0 OR
					rev.Count <> 0
				)
			) OR (
				c.ToId = manyToOne.EntityId AND
				fwd.Count <> 0
			) OR (
				c.ToId = oneToMany.EntityId AND
				rev.Count <> 0
			)
		)

	-----
	-- If there are any cardinality violations, raise an error
	-----
	IF ( @typeId IS NOT NULL )
	BEGIN
		SET @errorMessage = N'Cardinality violation detected. TypeId: ' + CAST( @typeId As NVARCHAR( 100 ) ) + ', FromId: ' + CAST( @fromId As NVARCHAR( 100 ) ) + ', ToId: ' + CAST( @toId As NVARCHAR( 100 ) ) + '.';
		THROW 50000, @errorMessage, 1;
	END

	-----
	-- Insert into the Relationship table
	-----
	INSERT INTO
		dbo.Relationship ( TenantId, TypeId, FromId, ToId )
	SELECT
		v.TenantId,
		v.TypeId,
		v.FromId,
		v.ToId
	FROM
		@data v
	LEFT JOIN
		dbo.Relationship r
	ON
		r.TenantId = v.TenantId AND
		r.TypeId = v.TypeId AND
		r.FromId = v.FromId AND
		r.ToId = v.ToId
	WHERE
		r.TenantId IS NULL
END