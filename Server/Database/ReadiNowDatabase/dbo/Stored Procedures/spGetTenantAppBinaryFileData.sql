-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE PROCEDURE [dbo].[spGetTenantAppBinaryFileData] 
	@solutionId BIGINT,
	@tenant BIGINT,
	@selfContained BIT = 0
AS
BEGIN
	-- Note* The #candidateList table is created externally on the connection prior to this call
	-- and is cleaned up externally unless @selfContained is set to 1.

	SET NOCOUNT ON
	
	DECLARE @fileDataHashFieldId BIGINT = dbo.fnAliasNsId( 'fileDataHash', 'core', @tenant )
	DECLARE @imageFileTypeId BIGINT = dbo.fnAliasNsId( 'imageFileType', 'core', @tenant )
	DECLARE @isOfType BIGINT = dbo.fnAliasNsId( 'isOfType', 'core', @tenant )

	DECLARE @derived TABLE ( Id BIGINT PRIMARY KEY )

	INSERT INTO @derived
	SELECT Id FROM dbo.fnDerivedTypes(@imageFileTypeId, @tenant)

	IF ( @selfContained = 1)
	BEGIN
		CREATE TABLE #candidateList ( UpgradeId UNIQUEIDENTIFIER PRIMARY KEY, [Explicit] BIT )

		EXEC spGetTenantAppEntities @solutionId, @tenant, 0
	END

	SELECT DISTINCT
		d.Data		
	FROM
		Data_NVarChar d		
	JOIN
		Entity e ON
			e.TenantId = d.TenantId AND
			e.Id = d.EntityId
	JOIN
		#candidateList c ON
			c.UpgradeId = e.UpgradeId				
	WHERE
		d.FieldId = @fileDataHashFieldId AND
		d.TenantId = @tenant AND
		EXISTS (
			SELECT 1
				FROM
					[dbo].[Relationship] r
				WHERE
					r.FromId = d.EntityId AND 
					r.TenantId = @tenant AND 
					r.TypeId = @isOfType AND
					r.ToId IN (SELECT Id FROM @derived)
		)

	IF ( @selfContained = 1 )
	BEGIN
		DROP TABLE #candidateList
	END
END