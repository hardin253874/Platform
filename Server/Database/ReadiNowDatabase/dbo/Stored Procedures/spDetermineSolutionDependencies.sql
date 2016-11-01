-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE PROCEDURE [dbo].[spDetermineSolutionDependencies]
	@solutionId BIGINT,
	@tenantId BIGINT
AS
BEGIN
	-- Note* The #uniqueCandidates table is created externally on the connection prior to this call
	-- and is cleaned up externally unless @selfContained is set to 1.

	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	-- To resolve undefined references create the temp table if it doesn't exist
	DECLARE @managedTempTable BIT = 0

	IF OBJECT_ID('tempdb..#uniqueCandidates') IS NULL
	BEGIN
		CREATE TABLE #uniqueCandidates ( UpgradeId UNIQUEIDENTIFIER PRIMARY KEY )
		SET @managedTempTable = 1
	END

	-----
	-- Select the id, name and version of the dependent solutions.
	-----
	SELECT
		sp.Data,
		n.Data,
		v.Data
	FROM (
		SELECT DISTINCT
			r.ToId
		FROM
			#uniqueCandidates d
		JOIN
			Entity e ON
				d.UpgradeId = e.UpgradeId AND
				e.TenantId = @tenantId
		JOIN
			Relationship r ON e.TenantId = r.TenantId AND e.Id = r.FromId AND r.ToId <> @solutionId
		CROSS APPLY
			dbo.tblFnAliasNsId( 'inSolution', 'core', @tenantId ) ra
		WHERE
			r.TenantId = @tenantId AND
			r.TypeId = ra.EntityId
		) a
	CROSS APPLY
		dbo.tblFnFieldNVarCharA( a.ToId, @tenantId, 'name', 'core' ) n
	CROSS APPLY
		dbo.tblFnFieldGuidA( a.ToId, @tenantId, 'packageId', 'core' ) sp
	JOIN (
		SELECT
			g.EntityId,
			g.TenantId,
			g.Data
		FROM
			Data_Guid g
		JOIN
			Data_Alias a ON
				g.TenantId = a.TenantId AND
				g.FieldId = a.EntityId AND
				a.AliasMarkerId = 0 AND
				a.Data = 'appVerId' AND
				a.Namespace = 'core'
		WHERE
			g.TenantId = 0
		) p ON
			p.TenantId = 0 AND
			sp.Data = p.Data
	OUTER APPLY
		dbo.tblFnFieldNVarCharA( p.EntityId, 0, 'appVersionString', 'core' ) v

	-- Drop the temporary table if it is managed.
	IF @managedTempTable = 1
	BEGIN
		DROP TABLE #uniqueCandidates
	END
END

