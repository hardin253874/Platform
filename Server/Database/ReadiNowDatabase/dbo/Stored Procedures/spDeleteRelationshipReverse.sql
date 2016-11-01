-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE PROCEDURE [dbo].[spDeleteRelationshipReverse] 
	@reverseRelationship dbo.ReverseRelationshipType READONLY
AS
BEGIN

	SET NOCOUNT ON;

	DELETE
		dbo.Relationship
	FROM
		dbo.Relationship a
	INNER JOIN
		@reverseRelationship b
	ON
		a.TenantId = b.TenantId AND
		a.TypeId = b.TypeId AND
		a.ToId = b.ToId
END