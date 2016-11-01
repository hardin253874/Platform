// Copyright 2011-2016 Global Software Innovation Pty Ltd

namespace TenantDiffTool.Core
{
	/// <summary>
	///     SQL Queries
	/// </summary>
	public static class SqlQueries
	{
		/// <summary>
		///     Get the application library apps.
		/// </summary>
		public static readonly string GetApplicationLibraryApplications = @"
DECLARE @isOfType BIGINT = dbo.fnAliasNsId( 'isOfType', 'core', DEFAULT )
DECLARE @app BIGINT = dbo.fnAliasNsId( 'app', 'core', DEFAULT )
DECLARE @name BIGINT = dbo.fnAliasNsId( 'name', 'core', DEFAULT )
DECLARE @description BIGINT = dbo.fnAliasNsId( 'description', 'core', DEFAULT )
DECLARE @packageForApplication BIGINT = dbo.fnAliasNsId( 'packageForApplication', 'core', DEFAULT )
DECLARE @appVersionString BIGINT = dbo.fnAliasNsId( 'appVersionString', 'core', DEFAULT )
DECLARE @appVerId BIGINT = dbo.fnAliasNsId( 'appVerId', 'core', DEFAULT )

SELECT
	Application = x.Application, Description = x.Description, Version = v.Data, PackageId = pid.Data
FROM
	(
	SELECT
		Application = n.Data, Description = d.Data, ApplicationId = n.EntityId
	FROM
		Relationship r
	LEFT JOIN
		Data_NVarChar n ON r.FromId = n.EntityId AND n.FieldId = @name
	LEFT JOIN
		Data_NVarChar d ON r.FromId = d.EntityId AND d.FieldId = @description
	WHERE
		r.TypeId = @isOfType
		AND
			r.ToId = @app
	) x
JOIN
	Relationship ap ON x.ApplicationId = ap.ToId AND ap.TypeId = @packageForApplication
JOIN
	Data_Guid pid ON ap.FromId = pid.EntityId AND pid.FieldId = @appVerId
LEFT JOIN
	Data_NVarChar v ON ap.FromId = v.EntityId
	AND
		v.FieldId = @appVersionString
ORDER BY
	x.Application, x.ApplicationId, ap.FromId";

		/// <summary>
		///     Get the tenants.
		/// </summary>
		public static readonly string GetTenants = @"
SELECT Id, name FROM _vTenant";

		/// <summary>
		///     Get the tenant applications
		/// </summary>
		public static readonly string GetTenantApplications = @"
DECLARE @tenantId BIGINT
SELECT @tenantId = Id FROM _vTenant WHERE name = @tenantName

DECLARE @name BIGINT = dbo.fnAliasNsId( 'name', 'core', @tenantId )
DECLARE @description BIGINT = dbo.fnAliasNsId( 'description', 'core', @tenantId )
DECLARE @solutionVersionString BIGINT = dbo.fnAliasNsId( 'solutionVersionString', 'core', @tenantId )
DECLARE @solution BIGINT = dbo.fnAliasNsId( 'solution', 'core', @tenantId )
DECLARE @isOfType BIGINT = dbo.fnAliasNsId( 'isOfType', 'core', @tenantId )

SELECT
	Application = sn.Data, Description = sd.Data, Version = sv.Data, Id = s.FromId
FROM
	Relationship r
JOIN
	Relationship s ON s.TenantId = r.FromId
	AND
		s.TypeId = @isOfType
	AND
		s.ToId = @solution
JOIN
	Data_NVarChar sv ON s.FromId = sv.EntityId AND sv.FieldId = @solutionVersionString
JOIN
	Data_NVarChar sn ON s.FromId = sn.EntityId
	AND
		sn.FieldId = @name
LEFT JOIN
	Data_NVarChar sd ON s.FromId = sd.EntityId
	AND
		sd.FieldId = @description
WHERE
	s.TenantId = @tenantId";

		/// <summary>
		///     Gets names CTE.
		/// </summary>
		public static readonly string GetNames = @"
;WITH Names ( EntityId, TenantId, Data )
AS
(
	SELECT
		n.EntityId, n.TenantId, n.Data
	FROM
		Data_NVarChar n
	JOIN
		Entity e ON e.Id = n.FieldId AND e.TenantId = n.TenantId AND e.UpgradeId = 'f8def406-90a1-4580-94f4-1b08beac87af'
)";

		/// <summary>
		///     Gets the tenant data.
		/// </summary>
		public static readonly string GetTenantData = @"
SELECT
	Type = '{1}', EntityUpgradeId = ee.UpgradeId, FieldUpgradeId = ef.UpgradeId, Data = CAST( d.Data AS NVARCHAR( MAX ) ) COLLATE DATABASE_DEFAULT, EntityName = n.Data, FieldName = f.Data, Additional = {3}
FROM
	{0} d
JOIN
	Entity ee ON d.EntityId = ee.Id AND d.TenantId = ee.TenantId
LEFT JOIN
	Names n ON d.EntityId = n.EntityId AND d.TenantId = n.TenantId
JOIN
	Entity ef ON d.FieldId = ef.Id AND d.TenantId = ef.TenantId
LEFT JOIN
	Names f ON d.FieldId = f.EntityId AND d.TenantId = f.TenantId
WHERE
	d.TenantId = {2}";

		/// <summary>
		///     Gets the tenant entities.
		/// </summary>
		public static readonly string GetTenantEntities = @"
SELECT
	EntityUpgradeId = e.UpgradeId, EntityName = n.Data
FROM
	Entity e
LEFT JOIN (
	SELECT
		n.EntityId, n.Data
	FROM
		Data_NVarChar n
	JOIN
		Entity e ON e.Id = n.FieldId AND e.TenantId = n.TenantId AND e.UpgradeId = 'f8def406-90a1-4580-94f4-1b08beac87af' ) n ON e.Id = n.EntityId
{0}
WHERE
	e.TenantId = @tenantId
{1}
ORDER BY
	CAST( e.UpgradeId AS NVARCHAR( 100 ) )";

		/// <summary>
		///     Gets the tenant relationships.
		/// </summary>
		public static readonly string GetTenantRelationships = @"
;WITH Names ( EntityId, TenantId, Data )
AS
(
	SELECT
		n.EntityId, n.TenantId, n.Data
	FROM
		Data_NVarChar n
	JOIN
		Entity e ON e.Id = n.FieldId AND e.TenantId = n.TenantId AND e.UpgradeId = 'f8def406-90a1-4580-94f4-1b08beac87af'
)
SELECT
	FromUpgradeId = eFrom.UpgradeId, FromName = nFrom.Data, TypeUpgradeId = eType.UpgradeId, TypeName = nType.Data, ToUpgradeId = eTo.UpgradeId, ToName = nTo.Data
FROM
	Relationship r
JOIN
	Entity eFrom ON r.FromId = eFrom.Id AND r.TenantId = eFrom.TenantId
JOIN
	Entity eType ON r.TypeId = eType.Id AND r.TenantId = eType.TenantId
JOIN
	Entity eTo ON r.ToId = eTo.Id AND r.TenantId = eTo.TenantId
LEFT JOIN
	Names nFrom ON r.FromId = nFrom.EntityId AND r.TenantId = nFrom.TenantId
LEFT JOIN
	Names nType ON r.TypeId = nType.EntityId AND r.TenantId = nType.TenantId
LEFT JOIN
	Names nTo ON r.ToId = nTo.EntityId AND r.TenantId = nTo.TenantId
{0}
WHERE
	r.TenantId = @tenantId
{1}
ORDER BY
	CAST( eFrom.UpgradeId AS NVARCHAR( 100 ) ), CAST( eType.UpgradeId AS NVARCHAR( 100 ) ), CAST( eTo.UpgradeId AS NVARCHAR( 100 ) )";

		/// <summary>
		///     Gets the file data.
		/// </summary>
		public static readonly string GetFileData = @"
SELECT
	'{1}' as Type, d.EntityUid as EntityUpgradeId, d.FieldUid as FieldUpgradeId, CAST( d.Data AS NVARCHAR ) as Data, n.Data as EntityName, f.Data as FieldName, {2} as Additional
FROM
	{0} d
LEFT JOIN
	_Data_NVarChar n ON d.EntityUid = n.EntityUid AND LOWER(n.FieldUid) = 'f8def406-90a1-4580-94f4-1b08beac87af'
LEFT JOIN
	_Data_NVarChar f ON d.EntityUid = f.EntityUid AND LOWER(f.FieldUid) = 'a6907c5a-19db-48ca-b9be-85d81cd9081f'";

		/// <summary>
		///     Gets the file entities.
		/// </summary>
		public static readonly string GetFileEntities = @"
SELECT
	e.Uid as EntityUpgradeId, n.Data as EntityName
FROM
	_Entity e
LEFT JOIN
	_Data_NVarChar n ON e.Uid = n.EntityUid AND LOWER(n.FieldUid) = 'f8def406-90a1-4580-94f4-1b08beac87af'
{0}
ORDER BY
	CAST( e.Uid AS NVARCHAR )";

		/// <summary>
		///     Gets the file relationships.
		/// </summary>
		public static readonly string GetFileRelationships = @"
SELECT
	r.FromUid as FromUpgradeId, nFrom.Data as FromName, r.TypeUid as TypeUpgradeId, nType.Data as TypeName, r.ToUid as ToUpgradeId, nTo.Data as ToName
FROM
	_Relationship r
LEFT JOIN
	_Data_NVarChar nFrom ON r.FromUid = nFrom.EntityUid AND LOWER(nFrom.FieldUid) = 'f8def406-90a1-4580-94f4-1b08beac87af'
LEFT JOIN
	_Data_NVarChar nType ON r.TypeUid = nType.EntityUid AND LOWER(nType.FieldUid) = 'f8def406-90a1-4580-94f4-1b08beac87af'
LEFT JOIN
	_Data_NVarChar nTo ON r.ToUid = nTo.EntityUid AND LOWER(nTo.FieldUid) = 'f8def406-90a1-4580-94f4-1b08beac87af'
{0}
ORDER BY
	CAST( r.FromUid AS NVARCHAR ), CAST( r.TypeUid AS NVARCHAR ), CAST( r.ToUid AS NVARCHAR )";

		/// <summary>
		///     Get the application library app data.
		/// </summary>
		public static readonly string GetAppLibData = @"
SELECT
	'{1}' as Type, d.EntityUid as EntityUpgradeId, d.FieldUid as FieldUpgradeId, CAST( d.Data AS NVARCHAR( MAX ) ) COLLATE SQL_Latin1_General_CP1_CS_AS as Data, {2} as Additional
FROM
	{0} d
WHERE
	d.AppVerUid = @appVerId";

		/// <summary>
		///     Get the application library entities.
		/// </summary>
		public static readonly string GetAppLibEntities = @"
SELECT
	EntityUpgradeId = e.EntityUid, EntityName = n.Data
FROM
	AppEntity e
LEFT JOIN
	AppData_NVarChar n ON e.EntityUid = n.EntityUid AND n.FieldUid = 'f8def406-90a1-4580-94f4-1b08beac87af' AND n.AppVerUid = @appVerId
{0}
WHERE
	e.AppVerUid = @appVerId
{1}
ORDER BY
	CAST( e.EntityUid AS NVARCHAR( 100 ) )";

		/// <summary>
		///     Get the application library relationships.
		/// </summary>
		public static readonly string GetAppLibRelationships = @"
;WITH Names ( EntityId, Data )
AS
(
	SELECT
		n.EntityUid, n.Data
	FROM
		AppData_NVarChar n
	WHERE
		n.FieldUid = 'f8def406-90a1-4580-94f4-1b08beac87af'
	AND
		n.AppVerUid = @appVerId
)
SELECT
	FromUpgradeId = r.FromUid, FromName = nFrom.Data, TypeUpgradeId = r.TypeUid, TypeName = nType.Data, ToUpgradeId = r.ToUid, ToName = nTo.Data
FROM
	AppRelationship r
LEFT JOIN
	Names nFrom ON r.FromUid = nFrom.EntityId
LEFT JOIN
	Names nType ON r.TypeUid = nType.EntityId
LEFT JOIN
	Names nTo ON r.ToUid = nTo.EntityId
{0}
WHERE
	r.AppVerUid = @appVerId
{1}
ORDER BY
	CAST( r.FromUid AS NVARCHAR( 100 ) ), CAST( r.TypeUid AS NVARCHAR( 100 ) ), CAST( r.ToUid AS NVARCHAR( 100 ) )";

		/// <summary>
		///     Get Tenant App Data.
		/// </summary>
		public static readonly string GetTenantAppData = @"
DECLARE @inSolution BIGINT = dbo.fnAliasNsId( 'inSolution', 'core', @tenant )
DECLARE @implicitInSolution BIGINT = dbo.fnAliasNsId( 'implicitInSolution', 'core', @tenant )
DECLARE @reverseImplicitInSolution BIGINT = dbo.fnAliasNsId( 'reverseImplicitInSolution', 'core', @tenant )

CREATE TABLE #candidateList ( UpgradeId UNIQUEIDENTIFIER PRIMARY KEY, [Explicit] BIT )

INSERT INTO
	#candidateList
SELECT
	UpgradeId, 1
FROM
	Entity e
JOIN
	Relationship r ON r.FromId = e.Id
	AND
		r.TypeId = @inSolution
	AND
		r.ToId = @solutionId
	AND
		r.TenantId = @tenant
	WHERE
		e.TenantId = @tenant
UNION
SELECT
	UpgradeId, 1
FROM
	Entity
WHERE
	Id = @solutionId

WHILE ( @@ROWCOUNT > 0 )
BEGIN
	INSERT INTO
		#candidateList
	SELECT
		a.UpgradeId, 0
	FROM
	(
		SELECT	
			e2.UpgradeId
		FROM
			Relationship r
		JOIN
			Entity e ON r.FromId = e.Id
		JOIN
			#candidateList b ON e.UpgradeId = b.UpgradeId
		LEFT JOIN
			Relationship r2 ON r.ToId = r2.FromId AND r2.TypeId = @inSolution
		JOIN
			Data_Bit db ON r.TypeId = db.EntityId AND db.FieldId = @implicitInSolution
		JOIN
			Entity e2 ON r.ToId = e2.Id
		WHERE
			r2.ToId IS NULL
			AND
				db.Data = 1
		UNION
		SELECT
			e2.UpgradeId
		FROM
			Relationship r
		JOIN
			Entity e ON r.ToId = e.Id
		JOIN
			#candidateList b ON e.UpgradeId = b.UpgradeId
		LEFT JOIN
			Relationship r2 ON r.FromId = r2.FromId AND r2.TypeId = @inSolution
		JOIN
			Data_Bit db ON r.TypeId = db.EntityId AND db.FieldId = @reverseImplicitInSolution
		JOIN
			Entity e2 ON r.FromId = e2.Id
		WHERE
			r2.ToId IS NULL
			AND
				db.Data = 1
	) a
	LEFT JOIN
		#candidateList c ON a.UpgradeId = c.UpgradeId
	WHERE
		c.UpgradeId IS NULL
END

CREATE TABLE #Data ( Type NVARCHAR( 100 ), EntityUid UNIQUEIDENTIFIER, FieldUid UNIQUEIDENTIFIER, Data NVARCHAR( MAX ) )

INSERT INTO #Data
SELECT 'Alias',	e.UpgradeId, efield.UpgradeId, d.Namespace + ':' + d.Data + ':' + CAST( d.AliasMarkerId AS NVARCHAR( 1 ) )
	FROM
		Data_Alias d
	JOIN
		Entity e ON d.EntityId = e.Id
	JOIN
		#candidateList c ON c.UpgradeId = e.UpgradeId
	JOIN
		Entity efield ON efield.Id = d.FieldId
		AND
			efield.TenantId = @tenant
	WHERE
		d.TenantId = @tenant
	{0}
		
INSERT INTO #Data
SELECT 'Bit',	e.UpgradeId, efield.UpgradeId, d.Data
	FROM
		Data_Bit d
	JOIN
		Entity e ON d.EntityId = e.Id
	JOIN
		#candidateList c ON c.UpgradeId = e.UpgradeId
	JOIN
		Entity efield ON efield.Id = d.FieldId
		AND
			efield.TenantId = @tenant
	WHERE
		d.TenantId = @tenant
	{0}
		
INSERT INTO #Data
SELECT 'DateTime',	e.UpgradeId, efield.UpgradeId, d.Data
	FROM
		Data_DateTime d
	JOIN
		Entity e ON d.EntityId = e.Id
	JOIN
		#candidateList c ON c.UpgradeId = e.UpgradeId
	JOIN
		Entity efield ON efield.Id = d.FieldId
		AND
			efield.TenantId = @tenant
	WHERE
		d.TenantId = @tenant
	{0}
		
INSERT INTO #Data
SELECT 'Decimal',	e.UpgradeId, efield.UpgradeId, d.Data
	FROM
		Data_Decimal d
	JOIN
		Entity e ON d.EntityId = e.Id
	JOIN
		#candidateList c ON c.UpgradeId = e.UpgradeId
	JOIN
		Entity efield ON efield.Id = d.FieldId
		AND
			efield.TenantId = @tenant
	WHERE
		d.TenantId = @tenant
	{0}
		
INSERT INTO #Data
SELECT 'Guid',	e.UpgradeId, efield.UpgradeId, d.Data
	FROM
		Data_Guid d
	JOIN
		Entity e ON d.EntityId = e.Id
	JOIN
		#candidateList c ON c.UpgradeId = e.UpgradeId
	JOIN
		Entity efield ON efield.Id = d.FieldId
		AND
			efield.TenantId = @tenant
	WHERE
		d.TenantId = @tenant
	{0}
		
INSERT INTO #Data
SELECT 'Int',	e.UpgradeId, efield.UpgradeId, d.Data
	FROM
		Data_Int d
	JOIN
		Entity e ON d.EntityId = e.Id
	JOIN
		#candidateList c ON c.UpgradeId = e.UpgradeId
	JOIN
		Entity efield ON efield.Id = d.FieldId
		AND
			efield.TenantId = @tenant
	WHERE
		d.TenantId = @tenant
	{0}
		
INSERT INTO #Data
SELECT 'NVarChar',	e.UpgradeId, efield.UpgradeId, d.Data
	FROM
		Data_NVarChar d
	JOIN
		Entity e ON d.EntityId = e.Id
	JOIN
		#candidateList c ON c.UpgradeId = e.UpgradeId
	JOIN
		Entity efield ON efield.Id = d.FieldId
		AND
			efield.TenantId = @tenant
	WHERE
		d.TenantId = @tenant
	{0}
		
INSERT INTO #Data
SELECT 'Xml',	e.UpgradeId, efield.UpgradeId, d.Data
	FROM
		Data_Xml d
	JOIN
		Entity e ON d.EntityId = e.Id
	JOIN
		#candidateList c ON c.UpgradeId = e.UpgradeId
	JOIN
		Entity efield ON efield.Id = d.FieldId
		AND
			efield.TenantId = @tenant
	WHERE
		d.TenantId = @tenant
	{0}
		
CREATE TABLE #Names ( EntityId BIGINT, TenantId BIGINT, Data NVARCHAR( MAX ), CONSTRAINT [PK_Names] PRIMARY KEY
(
	[EntityId] ASC,
	[TenantId] ASC
) )

INSERT INTO
	#Names
SELECT
	n.EntityId, n.TenantId, n.Data
FROM
	Data_NVarChar n
JOIN
	Entity e ON e.Id = n.FieldId AND e.TenantId = n.TenantId AND e.UpgradeId = 'f8def406-90a1-4580-94f4-1b08beac87af'

SELECT d.Type, d.EntityUid, d.FieldUid, d.Data, EntityName = ne.Data, FieldName = nf.Data
FROM #Data d
JOIN Entity ee On d.EntityUid = ee.UpgradeId AND ee.TenantId = @tenant
JOIN Entity ef On d.FieldUid = ef.UpgradeId AND ef.TenantId = @tenant
LEFT JOIN #Names	ne ON ne.EntityId = ee.Id AND ne.TenantId = @tenant
LEFT JOIN #Names	nf ON nf.EntityId = ef.Id AND ne.TenantId = @tenant
ORDER BY CAST( d.EntityUid AS NVARCHAR( 100 ) ), CAST( d.FieldUid AS NVARCHAR( 100 ) )

DROP TABLE #Names
DROP TABLE #Data		
DROP TABLE #candidateList";

		/// <summary>
		///     Get the tenant app entities.
		/// </summary>
		public static readonly string GetTenantAppEntities = @"
CREATE TABLE #Entities ( EntityUid UNIQUEIDENTIFIER )

INSERT INTO #Entities
EXEC spGetTenantAppEntities {0}, {1}, 1, 1

CREATE TABLE #Names ( EntityId UNIQUEIDENTIFIER PRIMARY KEY, Data NVARCHAR( MAX ) COLLATE SQL_Latin1_General_CP1_CS_AS )

INSERT INTO
	#Names
SELECT
	e.UpgradeId, n.Data COLLATE SQL_Latin1_General_CP1_CS_AS 
FROM
	Data_NVarChar n
JOIN
	Entity f ON f.Id = n.FieldId AND f.TenantId = n.TenantId AND f.UpgradeId = 'f8def406-90a1-4580-94f4-1b08beac87af'
JOIN
	Entity e ON e.Id = n.EntityId AND e.TenantId = n.TenantId
WHERE
	n.TenantId = {1}

SELECT e.EntityUid, n.Data
FROM #Entities e
LEFT JOIN #Names n ON e.EntityUid = n.EntityId
{2}
ORDER BY CAST( e.EntityUid AS NVARCHAR(100))

DROP TABLE #Names
DROP TABLE #Entities";

		/// <summary>
		///     Get the tenant app relationships.
		/// </summary>
		public static readonly string GetTenantAppRelationships = @"
CREATE TABLE #Relationships ( TypeId UNIQUEIDENTIFIER, FromId UNIQUEIDENTIFIER, ToId UNIQUEIDENTIFIER, Cardinality BIGINT )

INSERT INTO #Relationships
EXEC spGetTenantAppRelationships {0}, {1}, 1

CREATE TABLE #Names ( EntityId UNIQUEIDENTIFIER PRIMARY KEY, Data NVARCHAR( MAX ) )

INSERT INTO
	#Names
SELECT
	e.UpgradeId, n.Data
FROM
	Data_NVarChar n
JOIN
	Entity f ON f.Id = n.FieldId AND f.TenantId = n.TenantId AND f.UpgradeId = 'f8def406-90a1-4580-94f4-1b08beac87af'
JOIN
	Entity e ON e.Id = n.EntityId AND e.TenantId = n.TenantId
WHERE
	n.TenantId = {1}

SELECT r.FromId, nFrom.Data, r.TypeId, nType.Data, r.ToId, nTo.Data
FROM #Relationships r
LEFT JOIN #Names nFrom ON r.FromId = nFrom.EntityId
LEFT JOIN #Names nType ON r.TypeId = nType.EntityId
LEFT JOIN #Names nTo ON r.ToId = nTo.EntityId
{2}
ORDER BY CAST( r.FromId AS NVARCHAR(100)), CAST( r.TypeId AS NVARCHAR(100)), CAST( r.ToId AS NVARCHAR(100))

DROP TABLE #Names
DROP TABLE #Relationships";

		/// <summary>
		/// </summary>
		public static readonly string GetTenantAppRelationshipsViewer = @"
CREATE TABLE #Relationships ( TypeId UNIQUEIDENTIFIER, FromId UNIQUEIDENTIFIER, ToId UNIQUEIDENTIFIER, Cardinality BIGINT )

INSERT INTO #Relationships
EXEC spGetTenantAppRelationships {0}, {1}, 1

;WITH Names ( EntityId, TenantId, Data )
AS
(
	SELECT
		e.UpgradeId, n.TenantId, n.Data
	FROM
		Data_NVarChar n
	JOIN
		Entity f ON f.Id = n.FieldId AND f.TenantId = n.TenantId AND f.UpgradeId = 'f8def406-90a1-4580-94f4-1b08beac87af'
	JOIN
		Entity e ON e.Id = n.EntityId AND e.TenantId = n.TenantId
	WHERE
		n.TenantId = {1}
)
SELECT Name = ISNULL( nType.Data, r.TypeId ), Relationship = ISNULL( nTo.Data, r.ToId ), 'Forward'
FROM #Relationships r
LEFT JOIN Names nType ON r.TypeId = nType.EntityId
LEFT JOIN Names nTo ON r.ToId = nTo.EntityId
WHERE r.FromId = '{2}'
UNION ALL
SELECT Name = ISNULL( nType.Data, r.TypeId ), Relationship = ISNULL( nFrom.Data, r.FromId ), 'Reverse'
FROM #Relationships r
LEFT JOIN Names nType ON r.TypeId = nType.EntityId
LEFT JOIN Names nFrom ON r.FromId = nFrom.EntityId
WHERE r.ToId = '{2}'

DROP TABLE #Relationships";

		/// <summary>
		///     Get the tenant entity fields.
		/// </summary>
		public static readonly string GetTenantEntityViewerFields = @"
CREATE TABLE #fields ( Name NVARCHAR(50), Value NVARCHAR(1024) )

EXEC spExecForDataTables 'INSERT INTO #fields SELECT ISNULL( n.Data, CASE WHEN a.Namespace IS NULL THEN CAST( t1.Data AS NVARCHAR( 100 ) ) ELSE a.Namespace + '':'' + a.Data END ), CAST( t1.Data AS NVARCHAR(1024) ) FROM ? t1 WITH (NOLOCK) LEFT JOIN Data_Alias a WITH (NOLOCK) ON t1.FieldId = a.EntityId LEFT JOIN Data_NVarChar n WITH (NOLOCK)  ON t1.FieldId = n.EntityId AND ( SELECT EntityId FROM Data_Alias a WITH (NOLOCK) WHERE a.Data = ''name'' AND a.Namespace = ''core'' AND a.TenantId = {1} ) = n.FieldId WHERE t1.EntityId = {0}'

SELECT * FROM #fields ORDER BY Name, Value

DROP TABLE #fields";

		/// <summary>
		///     Get the tenant entity relationships.
		/// </summary>
		public static readonly string GetTenantEntityViewerRelationships = @"
SELECT * FROM
(
SELECT
	Name = ISNULL( n.Data, CASE WHEN tya.Data IS NULL THEN CAST ( r.TypeId AS NVARCHAR( 100 ) ) ELSE tya.Namespace + ':' + tya.Data END ),
	Relationship = ISNULL( d.Data, CASE WHEN toa.Data IS NULL THEN CAST( r.ToId AS NVARCHAR( 100 ) ) ELSE toa.Namespace + ':' + toa.Data END ),
	Direction = 'Forward'
FROM
	Relationship r WITH (NOLOCK)
LEFT JOIN
	Data_Alias tya WITH (NOLOCK) ON r.TypeId = tya.EntityId AND tya.AliasMarkerId = 0
LEFT JOIN
	Data_NVarChar n WITH (NOLOCK) ON r.TypeId = n.EntityId AND (
	SELECT
		EntityId
	FROM
		Data_Alias a WITH (NOLOCK)
	WHERE
		a.Data = 'name' AND a.Namespace = 'core' AND a.TenantId = @tenantId ) = n.FieldId
LEFT JOIN
	Data_Alias toa WITH (NOLOCK) ON r.ToId = toa.EntityId AND toa.AliasMarkerId = 0
LEFT JOIN
	Data_NVarChar d WITH (NOLOCK) ON r.ToId = d.EntityId AND (
	SELECT
		EntityId
	FROM
		Data_Alias a WITH (NOLOCK)
	WHERE
		a.Data = 'name' AND a.Namespace = 'core' AND a.TenantId = @tenantId ) = d.FieldId
LEFT JOIN
	Data_Alias c WITH (NOLOCK) ON d.EntityId = c.EntityId AND c.AliasMarkerId = 0
WHERE
	FromId = @entityId AND r.TenantId = @tenantId
UNION
SELECT
	Name = ISNULL( n.Data, CASE WHEN tya.Data IS NULL THEN CAST ( r.TypeId AS NVARCHAR( 100 ) ) ELSE tya.Namespace + ':' + tya.Data END ),
	Relationship = ISNULL( d.Data, CASE WHEN fra.Data IS NULL THEN CAST( r.FromId AS NVARCHAR( 100 ) ) ELSE fra.Namespace + ':' + fra.Data END ),
	Direction = 'Reverse'
FROM
	Relationship r WITH (NOLOCK)
LEFT JOIN
	Data_Alias tya WITH (NOLOCK) ON r.TypeId = tya.EntityId AND tya.AliasMarkerId = 0
LEFT JOIN
	Data_NVarChar n WITH (NOLOCK) ON r.TypeId = n.EntityId AND (
	SELECT
		EntityId
	FROM
		Data_Alias a WITH (NOLOCK) WHERE a.Data = 'name' AND a.Namespace = 'core' AND a.TenantId = @tenantId ) = n.FieldId
LEFT JOIN
	Data_Alias fra WITH (NOLOCK) ON r.FromId = fra.EntityId AND fra.AliasMarkerId = 0
LEFT JOIN
	Data_NVarChar d WITH (NOLOCK) ON r.FromId = d.EntityId AND (
	SELECT
		EntityId
	FROM
		Data_Alias a WITH (NOLOCK)
	WHERE
		a.Data = 'name' AND a.Namespace = 'core' AND a.TenantId = @tenantId ) = d.FieldId
WHERE
	ToId = @entityId AND r.TenantId = @tenantId
) a
ORDER BY a.Direction, a.Name, a.Relationship ASC";

		/// <summary>
		///     Gets the file entity fields.
		/// </summary>
		public static string GetFileEntityViewerFields = @"
SELECT
	f.Data as FieldName, CAST( d.Data AS NVARCHAR ) as Data, d.FieldUid as FieldUid
FROM
	{0} d
LEFT JOIN
	_Data_NVarChar f ON d.EntityUid = f.EntityUid AND LOWER(f.FieldUid) = 'a6907c5a-19db-48ca-b9be-85d81cd9081f'
WHERE
	LOWER(d.EntityUid) = '{1}'";

		/// <summary>
		///     Get the file entity relationships.
		/// </summary>
		public static readonly string GetFileEntityViewerRelationships = @"
SELECT
	'Forward' as Direction, r.TypeUid as TypeUpgradeId, nType.Data as TypeName, r.ToUid as ToUpgradeId, nTo.Data as ToName
FROM
	_Relationship r
LEFT JOIN
	_Data_NVarChar nType ON r.TypeUid = nType.EntityUid AND LOWER(nType.FieldUid) = 'f8def406-90a1-4580-94f4-1b08beac87af'
LEFT JOIN
	_Data_NVarChar nTo ON r.ToUid = nTo.EntityUid AND LOWER(nTo.FieldUid) = 'f8def406-90a1-4580-94f4-1b08beac87af'
WHERE
	LOWER( r.FromUid ) = '{0}'
UNION ALL
SELECT
	'Reverse' as Direction, r.TypeUid as TypeUpgradeId, nType.Data as TypeName, r.FromUid as FromUpgradeId, nFrom.Data as FromName
FROM
	_Relationship r
LEFT JOIN
	_Data_NVarChar nType ON r.TypeUid = nType.EntityUid AND LOWER(nType.FieldUid) = 'f8def406-90a1-4580-94f4-1b08beac87af'
LEFT JOIN
	_Data_NVarChar nFrom ON r.FromUid = nFrom.EntityUid AND LOWER(nFrom.FieldUid) = 'f8def406-90a1-4580-94f4-1b08beac87af'
WHERE
	LOWER( r.ToUid ) = '{0}'";

		/// <summary>
		///     Get the application library entity viewer data.
		/// </summary>
		public static readonly string GetAppLibEntityViewerData = @"
SELECT
	'{1}' as Type, d.EntityUid as EntityUpgradeId, d.FieldUid as FieldUpgradeId, CAST( d.Data AS NVARCHAR( MAX ) ) COLLATE SQL_Latin1_General_CP1_CS_AS as Data, {2} as Additional
FROM
	{0} d
WHERE
	d.AppVerUid = @appVerId
AND
	d.EntityUid = @entityUid";

		/// <summary>
		///     Gets the Application Library viewer relationships.
		/// </summary>
		public static readonly string GetAppLibraryEntityViewerRelationships = @"
;WITH Names ( EntityId, Data )
AS
(
	SELECT
		n.EntityUid, n.Data
	FROM
		AppData_NVarChar n
	WHERE
		n.FieldUid = 'f8def406-90a1-4580-94f4-1b08beac87af'
	AND
		n.AppVerUid = @appVerId
)
SELECT
	Name = nType.Data, Relationship = nTo.Data, 'Forward', r.TypeUid, r.ToUid
FROM
	AppRelationship r
LEFT JOIN
	Names nType ON r.TypeUid = nType.EntityId
LEFT JOIN
	Names nTo ON r.ToUid = nTo.EntityId
WHERE
	r.AppVerUid = @appVerId
AND
	r.FromUid = @entityUid
UNION ALL
SELECT
	Name = nType.Data, Relationship = nFrom.Data, 'Reverse', r.TypeUid, r.FromUid
FROM
	AppRelationship r
LEFT JOIN
	Names nType ON r.TypeUid = nType.EntityId
LEFT JOIN
	Names nFrom ON r.FromUid = nFrom.EntityId
WHERE
	r.AppVerUid = @appVerId
AND
	r.ToUid = @entityUid";
	}
}