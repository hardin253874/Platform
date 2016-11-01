-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE PROCEDURE [dbo].[spForwardRelationshipReadMany]
	@data dbo.ForwardRelationshipType READONLY
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		a.TenantId,
		a.TypeId,
		a.FromId,
		a.ToId
	FROM
		Relationship a
	JOIN
		@data d
	ON
		a.TenantId = d.TenantId AND
		a.TypeId = d.TypeId AND
		a.FromId = d.FromId
END