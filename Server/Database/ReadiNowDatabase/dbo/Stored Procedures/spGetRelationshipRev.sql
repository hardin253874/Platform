-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE PROCEDURE [dbo].[spGetRelationshipRev]
	@tenantId BIGINT,
	@isOfType BIGINT,
	@relTypeId BIGINT,
	@entityId BIGINT	
AS
BEGIN
	SET NOCOUNT ON;

	SELECT r.FromId, t.ToId FROM dbo.Relationship r
    JOIN dbo.Relationship t ON t.TenantId = @tenantId AND t.TypeId = @isOfType AND t.FromId = r.FromId
    WHERE r.TenantId = @tenantId AND r.TypeId = @relTypeId AND r.ToId = @entityId
END
