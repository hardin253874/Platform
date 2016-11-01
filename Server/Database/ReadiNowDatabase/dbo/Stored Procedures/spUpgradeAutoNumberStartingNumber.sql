-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE PROCEDURE [dbo].[spUpgradeAutoNumberStartingNumber]
	@tenantId BIGINT,
	@solutionId BIGINT,
	@context VARCHAR(128) = NULL
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	IF ( @context IS NULL )
	BEGIN
		SET @context = OBJECT_NAME(@@PROCID)
	END

	IF ( @context IS NOT NULL )
	BEGIN
		DECLARE @contextInfo VARBINARY(128) = CONVERT( VARBINARY(128), @context )
		SET CONTEXT_INFO @contextInfo
	END

	DECLARE @autoNumberField BIGINT = dbo.fnAliasNsId( 'autoNumberField', 'core', @tenantId )
	DECLARE @autoNumberValue BIGINT = dbo.fnAliasNsId( 'autoNumberValue', 'core', @tenantId )
	DECLARE @inSolution BIGINT = dbo.fnAliasNsId( 'inSolution', 'core', @tenantId )
	DECLARE @isOfType BIGINT = dbo.fnAliasNsId( 'isOfType', 'core', @tenantId )

	INSERT INTO
		Data_Int (
			EntityId,
			TenantId,
			FieldId,
			Data )
	SELECT
		r.FromId,
		@tenantId,
		@autoNumberValue,
		MAX( i.Data )
	FROM
		Relationship r
	JOIN
		Relationship s ON
			r.TenantId = s.TenantId
			AND r.FromId = s.FromId
			AND s.TypeId = @inSolution
			AND s.ToId = @solutionId	
	LEFT JOIN
		Data_Int i ON
			r.TenantId = i.TenantId
			AND r.FromId = i.FieldId
	LEFT JOIN
		Data_Int v ON
			r.TenantId = v.TenantId
			AND r.FromId = v.EntityId
			AND v.FieldId = @autoNumberValue
	WHERE
		r.TenantId = @tenantId
		AND r.TypeId = @isOfType
		AND r.ToId = @autoNumberField
		AND v.Data IS NULL
		AND i.Data IS NOT NULL
	GROUP BY
		r.FromId
END