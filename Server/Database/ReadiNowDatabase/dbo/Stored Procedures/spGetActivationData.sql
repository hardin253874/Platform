-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE PROCEDURE [dbo].[spGetActivationData]
	@tenantId BIGINT,
	@isOfType BIGINT,
	@entityIds dbo.[UniqueIdListType] READONLY
AS
BEGIN
	SET NOCOUNT ON

	SELECT r.FromId, r.ToId
    FROM dbo.Relationship r
		JOIN @entityIds e ON r.FromId = e.Id
    WHERE r.TenantId = @tenantId AND r.TypeId = @isOfType
END