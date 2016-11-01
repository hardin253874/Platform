-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE PROCEDURE [dbo].[spGetTenantAppFieldData] 
	@solutionId BIGINT,
	@tenant BIGINT,
	@dataTable NVARCHAR(MAX),
	@extraSql NVARCHAR(MAX) = N'',
	@selfContained BIT = 0
WITH RECOMPILE
AS
BEGIN
	-- Note* The #candidateList table is created externally on the connection prior to this call
	-- and is cleaned up externally unless @selfContained is set to 1.

	SET NOCOUNT ON
	
	DECLARE @inSolution BIGINT = dbo.fnAliasNsId( 'inSolution', 'core', @tenant )
	DECLARE @implicitInSolution BIGINT = dbo.fnAliasNsId( 'implicitInSolution', 'core', @tenant )
	DECLARE @reverseImplicitInSolution BIGINT = dbo.fnAliasNsId( 'reverseImplicitInSolution', 'core', @tenant )

	IF ( @selfContained = 1)
	BEGIN
		CREATE TABLE #candidateList ( UpgradeId UNIQUEIDENTIFIER PRIMARY KEY, [Explicit] BIT )
		CREATE TABLE #dependents ( Id BIGINT PRIMARY KEY )

		EXEC spGetTenantAppEntities @solutionId, @tenant, 0
	END

	DECLARE @aliasData NVARCHAR(MAX) = ''
	DECLARE @collate NVARCHAR( 50 ) = ''

	IF ( @dataTable = 'Alias' )
	BEGIN
		SET @aliasData = ', NULL, 0'
	END

	IF ( @dataTable = 'NVarChar' )
	BEGIN
		SET @collate = ' COLLATE Latin1_General_CS_AS'
	END

	DECLARE @sql NVARCHAR(MAX) = N'
DECLARE @excludeFieldFromPublish BIGINT = dbo.fnAliasNsId( ''excludeFieldFromPublish'', ''core'', @tenant )
DECLARE @applicationId BIGINT = dbo.fnAliasNsId( ''applicationId'', ''core'', 0 )
DECLARE @packageForApplication BIGINT = dbo.fnAliasNsId( ''packageForApplication'', ''core'', 0 )
DECLARE @appVerId BIGINT = dbo.fnAliasNsId( ''appVerId'', ''core'', 0 )

DECLARE @solutionUpgradeId UNIQUEIDENTIFIER
DECLARE @app BIGINT
DECLARE @packageIds TABLE ( PackageId UNIQUEIDENTIFIER PRIMARY KEY )
DECLARE @dependentCount INT
  
SELECT
	@solutionUpgradeId = UpgradeId
FROM
	Entity
WHERE
	TenantId = @tenant AND
Id = @solutionId

SELECT
	@app = EntityId
FROM
	Data_Guid
WHERE
	TenantId = 0 AND
	FieldId = @applicationId AND
	Data = @solutionUpgradeId

INSERT INTO
	@packageIds
SELECT DISTINCT
	g.Data
FROM
	Relationship r
JOIN
	Data_Guid g ON
		r.TenantId = g.TenantId AND
		r.FromId = g.EntityId AND
		g.FieldId = @appVerId
WHERE
	r.TenantId = 0 AND
	r.ToId = @app AND
	r.TypeId = @packageForApplication

;WITH Base ( EntityUid, FieldUid, Data' + @extraSql + ') AS
(
	SELECT
		e.UpgradeId, efield.UpgradeId, d.Data' + @collate + @extraSql + '
	FROM
		Data_' + @dataTable + ' d
	JOIN
		Entity e ON d.EntityId = e.Id AND e.TenantId = d.TenantId
	JOIN
		#candidateList c ON c.UpgradeId = e.UpgradeId
	JOIN
		Entity efield ON efield.Id = d.FieldId
		AND
			efield.TenantId = @tenant
	LEFT JOIN
		Data_Bit exc ON exc.TenantId = @tenant AND d.FieldId = exc.EntityId AND exc.FieldId = @excludeFieldFromPublish
	WHERE
		d.TenantId = @tenant
	AND
			ISNULL( exc.Data, 0 ) = 0
)
SELECT
	EntityUid,
	FieldUid,
	Data' + @extraSql + '
FROM
	Base

UNION

SELECT
	f.EntityUid, f.FieldUid, f.Data' + @aliasData + '
FROM
	AppDeploy_Field f
JOIN
	@packageIds p ON
		f.AppVerUid = p.PackageId
LEFT JOIN
	Base b ON
		f.EntityUid = b.EntityUid AND
		f.FieldUid = b.FieldUid
WHERE
	f.[Type] = @dataTable AND
	f.TenantId = @tenant AND
	b.EntityUid IS NULL'

	DECLARE @params NVARCHAR(MAX) = N'@tenant BIGINT, @solutionId BIGINT, @dataTable NVARCHAR(50)';

	EXEC sp_executesql @sql, @params, @tenant = @tenant, @solutionId = @solutionId, @dataTable = @dataTable

	IF ( @selfContained = 1 )
	BEGIN
		DROP TABLE #candidateList
		DROP TABLE #dependents
	END
END