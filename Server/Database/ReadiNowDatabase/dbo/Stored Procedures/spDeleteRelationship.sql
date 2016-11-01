-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE PROCEDURE [dbo].[spDeleteRelationship] 
	@relationship dbo.RelationshipType READONLY
AS
BEGIN

	SET NOCOUNT ON;

	DELETE
		dbo.Relationship
	FROM
		dbo.Relationship a
	INNER JOIN
		@relationship b
	ON
		a.TenantId = b.TenantId AND
		a.TypeId = b.TypeId AND
		a.FromId = b.FromId AND
		a.ToId = b.ToId
END