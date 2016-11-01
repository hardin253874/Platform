// Copyright 2011-2016 Global Software Innovation Pty Ltd

namespace EDC.SoftwarePlatform.Migration.Processing
{
	/// <summary>
	/// </summary>
	public static class CommandText
	{				
		/// <summary>
		///     The delete entities command text
		/// </summary>
		public const string TenantMergeTargetDeleteEntitiesCommandText = @"
INSERT INTO
	EntityBatch (BatchId, EntityId)
SELECT DISTINCT
	@batchId, ee.Id
FROM
	#Entities e
JOIN
	Entity ee ON ee.TenantId = @tenant AND e.UpgradeId = ee.UpgradeId
LEFT JOIN
	#externallyReferencedEntities r ON r.Id = ee.Id
WHERE
	r.Id IS NULL

EXEC spDeleteBatch @batchId, @tenant, 0, 1

SELECT @@ROWCOUNT

DROP TABLE #externallyReferencedEntities";

		/// <summary>
		/// The tenant merge target pre-delete entities command text
		/// </summary>
		public const string TenantMergeTargetPreDeleteEntitiesCommandText = @"
DECLARE @solutionId BIGINT = NULL
CREATE TABLE #externallyReferencedEntities ( Id BIGINT )
DECLARE @inSolution BIGINT = dbo.fnAliasNsId( 'inSolution', 'core', @tenant )
DECLARE @indirectInSolution BIGINT = dbo.fnAliasNsId( 'indirectInSolution', 'core', @tenant )

SET @solutionId = (
SELECT
	e.Id
FROM
	Entity e
WHERE
	e.TenantId = @tenant
	AND
		e.UpgradeId = @applicationId
)

-- Determined the entities that are referenced externally
INSERT INTO
	#externallyReferencedEntities ( Id )
SELECT DISTINCT
	e.Id
FROM
	Relationship r
JOIN
	Entity e ON e.TenantId = @tenant AND r.FromId = e.Id
JOIN
	#Entities s ON e.UpgradeId = s.UpgradeId
WHERE
	r.TenantId = @tenant
	AND (
		r.TypeId = @inSolution OR
		r.TypeId = @indirectInSolution
	)
	AND
		r.TenantId = @tenant
	AND (
		@solutionId IS NULL
		OR
			@solutionId <> r.ToId
	)

SELECT
	Id
FROM
	#externallyReferencedEntities

DROP TABLE #externallyReferencedEntities";

		/// <summary>
		///     The delete field command text
		/// </summary>
		public const string TenantMergeTargetDeleteFieldCommandText = @"
DELETE
	Data_{0} WITH (PAGLOCK)
FROM
	Data_{0} d
JOIN
	#{0} o ON o.EntityId = d.EntityId AND o.FieldId = d.EntityId AND o.Data = d.Data
WHERE
	d.TenantId = @tenant";

		/// <summary>
		///     The delete relationships command text.
		/// </summary>
		public const string TenantMergeTargetDeleteRelationshipsCommandText = @"
DELETE
	Relationship WITH (PAGLOCK)
FROM
	Relationship r
JOIN
	#Relationships v ON r.TypeId = v.TypeId AND r.FromId = v.FromId AND r.ToId = v.ToId
WHERE
	r.TenantId = @tenant
";

		/// <summary>
		///     The delete entity information solution relationships
		/// </summary>
		public const string TenantMergeTargetDeleteEntityInSolutionRelationships = @"
DECLARE @inSolution BIGINT = dbo.fnAliasNsId( 'inSolution', 'core', @tenant )
DECLARE @solutionId BIGINT

SELECT
	@solutionId = Id
FROM
	Entity
WHERE
	UpgradeId = @applicationId AND
	TenantId = @tenant

DELETE
	Relationship
FROM
	Relationship r
JOIN
	Entity e ON e.TenantId = @tenant AND e.Id = r.FromId
JOIN
	#Entities s ON s.UpgradeId = e.UpgradeId
WHERE
	r.TenantId = @tenant
	AND
		r.TypeId = @inSolution
	AND
		r.ToId = @solutionId";
		

		/// <summary>
		///     The update field command text
		/// </summary>
		public const string TenantMergeTargetUpdateFieldCommandText = @"
UPDATE
	d WITH (PAGLOCK)
SET
	d.Data = u.Data{1}
FROM
	Data_{0} d
JOIN
	#{0} u ON u.EntityId = d.EntityId AND u.FieldId = d.FieldId
WHERE
	d.TenantId = @tenant
	AND
	d.Data = u.ExistingData
";

		/// <summary>
		///     The update field command text
		/// </summary>
		public const string TenantMergeTargetUpdateAliasCommandText = @"
UPDATE
	d WITH (PAGLOCK)
SET
	d.Data = u.Data{1}
FROM
	Data_{0} d
JOIN
	#{0} u ON u.EntityId = d.EntityId AND u.FieldId = d.FieldId
WHERE
	d.TenantId = @tenant
";

		/// <summary>
		///     The update relationships command text
		/// </summary>
		public const string TenantMergeTargetUpdateRelationshipsCommandText = @"
UPDATE
	r WITH (PAGLOCK)
SET
	r.ToId = u.ToId
FROM
	Relationship r
JOIN
	#Relationships u ON u.TypeId = r.TypeId AND u.FromId = r.FromId AND u.OldToId = r.ToId
WHERE
	r.TenantId = @tenant AND
	u.OldToId IS NOT NULL AND
    NOT EXISTS (SELECT Id FROM Relationship WHERE TypeId = u.TypeId AND FromId = u.FromId AND ToId = u.ToId AND TenantId = @tenant)

UPDATE
	r
SET
	r.FromId = u.FromId
FROM
	Relationship r
JOIN
	#Relationships u ON u.TypeId = r.TypeId AND u.ToId = r.ToId AND u.OldFromId = r.FromId
WHERE
	r.TenantId = @tenant AND
	u.OldFromId IS NOT NULL AND
    NOT EXISTS (SELECT Id FROM Relationship WHERE TypeId = u.TypeId AND FromId = u.FromId AND ToId = u.ToId AND TenantId = @tenant)
";			

		/// <summary>
		///     The write entities command text
		/// </summary>
		public const string TenantMergeTargetWriteEntitiesCommandText = @"
INSERT INTO Entity WITH (PAGLOCK) (TenantId, UpgradeId)
SELECT DISTINCT
	@tenant, e.UpgradeId
FROM
	#Entities e
LEFT JOIN
	Entity e2 ON e.UpgradeId = e2.UpgradeId AND e2.TenantId = @tenant
WHERE
	e2.Id IS NULL";

		/// <summary>
		///     The write field command text
		/// </summary>
		public const string TenantMergeTargetWriteFieldCommandText = @"
MERGE Data_{0} WITH ( PAGLOCK, HOLDLOCK ) o 
USING (
	SELECT
		EntityId,
		FieldId,
		Data{1}
	FROM
		#{0} )
	AS d (
		EntityId,
		FieldId,
		Data{1} )
	ON (
		o.EntityId = d.EntityId AND
		o.TenantId = @tenant AND
		o.FieldId = d.FieldId )
WHEN MATCHED AND ( d.Data <> o.Data{4} ) THEN
	UPDATE SET o.Data = d.Data{3}
WHEN NOT MATCHED THEN
INSERT (
	EntityId,
	TenantId,
	FieldId,
	Data{1} )
VALUES (
	EntityId,
	@tenant,
	FieldId,
	Data{2} );
";


        /// <summary>
        ///     The write secured command text
        /// </summary>
        public const string TenantMergeTargetWriteSecuredCommandText = @"
OPEN SYMMETRIC KEY key_Secured DECRYPTION BY CERTIFICATE cert_keyProtection

MERGE Secured WITH ( PAGLOCK, HOLDLOCK ) o 
USING (
	SELECT
		SecureId,
		Context,
		Data
	FROM
		#{0} )
	AS d (
		SecureId,
		Context,
		Data )
	ON (
		o.SecureId = d.SecureId AND
		o.TenantId = @tenant )
WHEN MATCHED THEN
	UPDATE 
        SET o.Data = ENCRYPTBYKEY(key_guid('key_Secured'), d.Data), o.Context = d.Context
WHEN NOT MATCHED THEN
INSERT (
	SecureId,
	TenantId,
	Context,
	Data )
VALUES (
	SecureId,
	@tenant,
	Context,
	ENCRYPTBYKEY(key_guid('key_Secured'), Data) );

CLOSE SYMMETRIC KEY key_Secured

";

        /// <summary>
        ///     The write relationships command text
        /// </summary>
        public const string TenantMergeTargetWriteRelationshipsCommandText = @"
DECLARE @cardinality BIGINT = dbo.fnAliasNsId( 'cardinality', 'core', @tenant )
DECLARE @manyToOne BIGINT = dbo.fnAliasNsId( 'manyToOne', 'core', @tenant )
DECLARE @oneToOne BIGINT = dbo.fnAliasNsId( 'oneToOne', 'core', @tenant )
DECLARE @oneToMany BIGINT = dbo.fnAliasNsId( 'oneToMany', 'core', @tenant )

DELETE
	Relationship WITH (PAGLOCK)
FROM
	Relationship r
JOIN
	#Relationships rr ON r.TenantId = @tenant AND r.TypeId = rr.TypeId AND r.FromId = rr.FromId
WHERE
	rr.TypeId = @cardinality

DELETE
	Relationship
FROM
	Relationship r
JOIN (
	SELECT
		r.*,
		ROW_NUMBER() OVER (PARTITION BY @tenant, r.TypeId, r.FromId ORDER BY r.ToId DESC ) AS Row
	FROM
		Relationship r
	JOIN
		#Relationships s ON r.TenantId = @tenant AND r.TypeId = s.TypeId AND r.FromId = s.FromId
	LEFT JOIN
		Relationship c ON c.TenantId = r.TenantId AND c.FromId = r.TypeId AND c.TypeId = @cardinality
	WHERE
		r.TenantId = @tenant AND
		(
			(
				c.ToId = @manyToOne OR
				c.ToId = @oneToOne
			)
		)
	) a ON
	r.TenantId = a.TenantId AND
	r.TypeId = a.TypeId AND
	r.FromId = a.FromId
WHERE
	a.Row > 1

UPDATE
	r WITH (PAGLOCK)
SET
	r.ToId = s.ToId
FROM
	Relationship r
JOIN
	#Relationships s ON r.TenantId = @tenant AND r.TypeId = s.TypeId AND r.FromId = s.FromId
LEFT JOIN
	Relationship o ON o.TenantId = @tenant AND o.TypeId = s.TypeId AND o.FromId = s.FromId AND o.ToId = s.ToId
LEFT JOIN
	Relationship c ON c.TenantId = r.TenantId AND c.FromId = r.TypeId AND c.TypeId = @cardinality
WHERE
	r.TenantId = @tenant AND
	o.Id IS NULL AND
	r.ToId <> s.ToId AND
	(
		(
			c.ToId = @manyToOne OR
			c.ToId = @oneToOne
		)
	)

DELETE
	Relationship
FROM
	Relationship r
JOIN (
	SELECT
		r.*,
		ROW_NUMBER() OVER (PARTITION BY @tenant, r.TypeId, r.ToId ORDER BY r.FromId DESC ) AS Row
	FROM
		Relationship r
	JOIN
		#Relationships s ON r.TenantId = @tenant AND r.TypeId = s.TypeId AND r.ToId = s.ToId
	LEFT JOIN
		Relationship c ON c.TenantId = r.TenantId AND c.FromId = r.TypeId AND c.TypeId = @cardinality
	WHERE
		r.TenantId = @tenant AND
		(
			(
				c.ToId = @oneToMany OR
				c.ToId = @oneToOne
			)
		)
	) a ON
	r.TenantId = a.TenantId AND
	r.TypeId = a.TypeId AND
	r.ToId = a.ToId
WHERE
	a.Row > 1

UPDATE
	r WITH (PAGLOCK)
SET
	r.FromId = s.FromId
FROM
	Relationship r
JOIN
	#Relationships s ON r.TypeId = s.TypeId AND r.ToId = s.ToId
LEFT JOIN
	Relationship o ON o.TenantId = @tenant AND o.TypeId = s.TypeId AND o.FromId = s.FromId AND o.ToId = s.ToId
LEFT JOIN
	Relationship c ON c.TenantId = r.TenantId AND c.FromId = r.TypeId AND c.TypeId = @cardinality
WHERE
	r.TenantId = @tenant AND
	o.Id IS NULL AND
	r.FromId <> s.FromId AND
	(
		(
			c.ToId = @oneToMany OR
			c.ToId = @oneToOne
		)
	)

INSERT INTO
	Relationship WITH (PAGLOCK) (TenantId, TypeId, FromId, ToId )
SELECT DISTINCT
	@tenant, r.TypeId, r.FromId, r.ToId
FROM
	#Relationships r
LEFT JOIN
	Relationship o ON o.TenantId = @tenant AND o.TypeId = r.TypeId AND o.FromId = r.FromId AND o.ToId = r.ToId
LEFT JOIN
	Relationship r2 ON r2.TenantId = @tenant AND r2.FromId = r.TypeId AND r2.TypeId = @cardinality
LEFT JOIN (
		SELECT
			rFwd.TenantId,
			rFwd.TypeId,
			rFwd.FromId,
			Count = COUNT ( rFwd.ToId )
		FROM
			dbo.Relationship rFwd
		GROUP BY
			rFwd.TenantId,
			rFwd.TypeId,
			rFwd.FromId ) fwd
	ON
		fwd.TenantId = @tenant AND
		r.TypeId = fwd.TypeId AND
		r.FromId = fwd.FromId
LEFT JOIN (
		SELECT
			rRev.TenantId,
			rRev.TypeId,
			rRev.ToId,
			Count = COUNT ( rRev.FromId )
		FROM
			dbo.Relationship rRev
		GROUP BY
			rRev.TenantId,
			rRev.TypeId,
			rRev.ToId ) rev
	ON
		rev.TenantId = @tenant AND
		r.TypeId = rev.TypeId AND
		r.ToId = rev.ToId
WHERE o.Id IS NULL
AND (
	r2.ToId IS NULL
	OR r.TypeId = @cardinality
	OR
	(
		(
			(
				r2.ToId <> @manyToOne
				AND
				r2.ToId <> @oneToOne
			)
			OR
				fwd.FromId IS NULL
			OR
				fwd.Count <= 0
		)
		AND
		(
			(
				r2.ToId <> @oneToMany
				AND
				r2.ToId <> @oneToOne
			)
			OR
				rev.ToId IS NULL
			OR
				rev.Count <= 0
		)
	)
) OPTION (MAXDOP 1)";

		/// <summary>
		///     The TenantMergeTarget write missing relationships command text
		/// </summary>
		public const string TenantMergeTargetWriteMissingRelationshipsCommandText = @"
INSERT INTO
	AppDeploy_Relationship ( AppVerUid, TenantId, TypeUid, FromUid, ToUid )
SELECT DISTINCT
	r.AppVerUid, r.TenantId, r.TypeUid, r.FromUid, r.ToUid
FROM
	#AppDeploy_Relationship r
LEFT JOIN
	AppDeploy_Relationship o ON o.AppVerUid = r.AppVerUid AND o.TenantId = r.TenantId AND o.TypeUid = r.TypeUid AND o.FromUid = r.FromUid AND o.ToUid = r.ToUid
WHERE o.AppVerUid IS NULL";

		/// <summary>
		///     The tenant merge target write missing fields command text
		/// </summary>
		public const string TenantMergeTargetWriteMissingFieldsCommandText = @"
INSERT INTO
	AppDeploy_Field ( AppVerUid, TenantId, EntityUid, FieldUid, Data, [Type] )
SELECT DISTINCT
	r.AppVerUid, r.TenantId, r.EntityUid, r.FieldUid, r.Data, r.[Type]
FROM
	#AppDeploy_Field r
LEFT JOIN
	AppDeploy_Field o ON o.AppVerUid = r.AppVerUid AND o.TenantId = r.TenantId AND o.EntityUid = r.EntityUid AND o.FieldUid = r.FieldUid
WHERE o.AppVerUid IS NULL";

		/// <summary>
		///     The TenantMergeTarget setup command text
		/// </summary>
		public const string TenantMergeTargetSetupCommandText = @"
CREATE TABLE #Entities ( UpgradeId UNIQUEIDENTIFIER PRIMARY KEY )

CREATE TABLE #Relationships ( TypeId BIGINT NOT NULL, FromId BIGINT NOT NULL, ToId BIGINT NOT NULL, OldFromId BIGINT NULL, OldToId BIGINT NULL )
ALTER TABLE #Relationships ADD PRIMARY KEY (TypeId, FromId, ToId)
CREATE NONCLUSTERED INDEX [#Relationship_TypeId_ToId] ON [#Relationships]([TypeId] ASC, [ToId] ASC, [FromId] ASC)

CREATE TABLE #Alias ( EntityId BIGINT NOT NULL, FieldId BIGINT NOT NULL, Data NVARCHAR(100) COLLATE Latin1_General_CS_AS, Namespace NVARCHAR(100) COLLATE Latin1_General_CS_AS, AliasMarkerId INT )
ALTER TABLE #Alias ADD PRIMARY KEY (EntityId, FieldId)

CREATE TABLE #Bit ( EntityId BIGINT NOT NULL, FieldId BIGINT NOT NULL, Data BIT NULL, ExistingData BIT NULL )
ALTER TABLE #Bit ADD PRIMARY KEY (EntityId, FieldId)

CREATE TABLE #DateTime ( EntityId BIGINT NOT NULL, FieldId BIGINT NOT NULL, Data DATETIME NULL, ExistingData DATETIME NULL )
ALTER TABLE #DateTime ADD PRIMARY KEY (EntityId, FieldId)

CREATE TABLE #Decimal ( EntityId BIGINT NOT NULL, FieldId BIGINT NOT NULL, Data DECIMAL(38,10) NULL, ExistingData DECIMAL(38,10) NULL )
ALTER TABLE #Decimal ADD PRIMARY KEY (EntityId, FieldId)

CREATE TABLE #Guid ( EntityId BIGINT NOT NULL, FieldId BIGINT NOT NULL, Data UNIQUEIDENTIFIER NULL, ExistingData UNIQUEIDENTIFIER NULL )
ALTER TABLE #Guid ADD PRIMARY KEY (EntityId, FieldId)

CREATE TABLE #Int ( EntityId BIGINT NOT NULL, FieldId BIGINT NOT NULL, Data INT NULL, ExistingData INT NULL )
ALTER TABLE #Int ADD PRIMARY KEY (EntityId, FieldId)

CREATE TABLE #NVarChar ( EntityId BIGINT NOT NULL, FieldId BIGINT NOT NULL, Data NVARCHAR(MAX) COLLATE Latin1_General_CI_AI NULL, ExistingData NVARCHAR(MAX) COLLATE Latin1_General_CI_AI NULL )
ALTER TABLE #NVarChar ADD PRIMARY KEY (EntityId, FieldId)

CREATE TABLE #Xml ( EntityId BIGINT NOT NULL, FieldId BIGINT NOT NULL, Data NVARCHAR(MAX) COLLATE Latin1_General_CS_AS NULL, ExistingData NVARCHAR(MAX) COLLATE Latin1_General_CS_AS NULL )
ALTER TABLE #Xml ADD PRIMARY KEY (EntityId, FieldId)

CREATE TABLE #Secured ( SecureId UNIQUEIDENTIFIER NOT NULL, Context NVARCHAR(MAX) NOT NULL, Data NVARCHAR(MAX) NULL )
ALTER TABLE #Secured ADD PRIMARY KEY (SecureId)

CREATE TABLE #AppDeploy_Field ( AppVerUid UNIQUEIDENTIFIER NOT NULL, TenantId BIGINT NOT NULL, EntityUid UNIQUEIDENTIFIER NOT NULL, FieldUid UNIQUEIDENTIFIER NOT NULL, Data NVARCHAR(MAX), [Type] NVARCHAR(50))
ALTER TABLE #AppDeploy_Field ADD PRIMARY KEY ( AppVerUid, TenantId, EntityUid, FieldUid )

CREATE TABLE #AppDeploy_Relationship ( AppVerUid UNIQUEIDENTIFIER NOT NULL, TenantId BIGINT NOT NULL, TypeUid UNIQUEIDENTIFIER NOT NULL, FromUid UNIQUEIDENTIFIER NOT NULL, ToUid UNIQUEIDENTIFIER NOT NULL )
ALTER TABLE #AppDeploy_Relationship ADD PRIMARY KEY ( AppVerUid, TenantId, TypeUid, FromUid, ToUid )
";

		/// <summary>
		///     The TenantMergeTarget tear down command text
		/// </summary>
		public const string TenantMergeTargetTearDownCommandText = @"
DROP TABLE #Entities
DROP TABLE #Relationships
DROP TABLE #Alias
DROP TABLE #Bit
DROP TABLE #DateTime
DROP TABLE #Decimal
DROP TABLE #Guid
DROP TABLE #Int
DROP TABLE #NVarChar
DROP TABLE #Xml
DROP TABLE #Secured
DROP TABLE #AppDeploy_Field
DROP TABLE #AppDeploy_Relationship";

        /// <summary>
        ///     The TenantRepairTarget setup command text
        /// </summary>
        public const string TenantRepairTargetSetupCommandText = @"
CREATE TABLE #Entities ( UpgradeId UNIQUEIDENTIFIER PRIMARY KEY )
CREATE TABLE #Relationships ( TypeId BIGINT, FromId BIGINT, ToId BIGINT )
CREATE TABLE #Alias ( EntityId BIGINT, FieldId BIGINT, Data NVARCHAR(100) COLLATE Latin1_General_CS_AS, Namespace NVARCHAR(100) COLLATE Latin1_General_CS_AS, AliasMarkerId INT )
CREATE TABLE #Bit ( EntityId BIGINT, FieldId BIGINT, Data BIT NULL )
CREATE TABLE #DateTime ( EntityId BIGINT, FieldId BIGINT, Data DATETIME NULL )
CREATE TABLE #Decimal ( EntityId BIGINT, FieldId BIGINT, Data DECIMAL(38,10) NULL )
CREATE TABLE #Guid ( EntityId BIGINT, FieldId BIGINT, Data UNIQUEIDENTIFIER NULL )
CREATE TABLE #Int ( EntityId BIGINT, FieldId BIGINT, Data INT NULL )
CREATE TABLE #NVarChar ( EntityId BIGINT, FieldId BIGINT, Data NVARCHAR(MAX) COLLATE Latin1_General_CI_AI NULL )
CREATE TABLE #Xml ( EntityId BIGINT, FieldId BIGINT, Data NVARCHAR(MAX) COLLATE Latin1_General_CS_AS NULL )
CREATE TABLE #Secured ( SecureId UNIQUEIDENTIFIER NOT NULL, Context NVARCHAR(MAX) NOT NULL, Data NVARCHAR(MAX) NULL )";


        /// <summary>
        ///     The TenantRepairTarget tear down command text
        /// </summary>
        public const string TenantRepairTargetTearDownCommandText = @"
DROP TABLE #Entities
DROP TABLE #Relationships
DROP TABLE #Alias
DROP TABLE #Bit
DROP TABLE #DateTime
DROP TABLE #Decimal
DROP TABLE #Guid
DROP TABLE #Int
DROP TABLE #NVarChar
DROP TABLE #Xml
DROP TABLE #Secured";

		/// <summary>
		///     The get batch unique identifier command text
		/// </summary>
		public const string GetBatchIdCommandText = @"declare @batchGuid uniqueidentifier = newid();
                                     insert into Batch (BatchGuid) values (@batchGuid)
	                                 select @@identity";

		/// <summary>
		///     The populate upgrade map command text
		/// </summary>
		public const string PopulateUpgradeMapCommandText = "SELECT UpgradeId, Id FROM Entity WHERE TenantId = @tenant";

		/// <summary>
		///     The write field command text
		/// </summary>
		public const string TenantRepairTargetWriteFieldCommandText = @"
INSERT INTO
	Data_{0} (EntityId, TenantId, FieldId, Data)
SELECT DISTINCT
	d.EntityId, @tenant, d.FieldId, d.Data
FROM
	#{0} d
LEFT JOIN
	Data_{0} o ON o.TenantId = @tenant AND o.EntityId = d.EntityId AND o.FieldId = d.FieldId
WHERE
	o.EntityId IS NULL

UPDATE
	o
SET
	o.Data = d.Data
FROM
	#{0} d
JOIN
	Data_{0} o ON o.TenantId = @tenant AND o.EntityId = d.EntityId AND o.FieldId = d.FieldId
WHERE
	d.Data <> o.Data
";

		/// <summary>
		///     The tenant source get binary data command text
		/// </summary>
		public const string TenantSourceGetBinaryDataCommandText = @"
DECLARE @fileDataHashFieldId BIGINT = dbo.fnAliasNsId( 'fileDataHash', 'core', @tenant )
DECLARE @imageFileTypeId BIGINT = dbo.fnAliasNsId( 'imageFileType', 'core', @tenant )
DECLARE @isOfType BIGINT = dbo.fnAliasNsId( 'isOfType', 'core', @tenant )

DECLARE @derived TABLE ( Id BIGINT PRIMARY KEY )

INSERT INTO @derived
SELECT Id FROM dbo.fnDerivedTypes(@imageFileTypeId, @tenant)

SELECT 
	DISTINCT d.Data	
FROM	
	[dbo].Data_NVarChar d
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
";

		/// <summary>
		///     The tenant source get document data command text
		/// </summary>
		public const string TenantSourceGetDocumentDataCommandText = @"
DECLARE @fileDataHashFieldId BIGINT = dbo.fnAliasNsId( 'fileDataHash', 'core', @tenant )
DECLARE @imageFileTypeId BIGINT = dbo.fnAliasNsId( 'imageFileType', 'core', @tenant )
DECLARE @fileTypeId BIGINT = dbo.fnAliasNsId( 'fileType', 'core', @tenant )
DECLARE @isOfType BIGINT = dbo.fnAliasNsId( 'isOfType', 'core', @tenant )
DECLARE @derivedFileType TABLE ( Id BIGINT PRIMARY KEY )

INSERT INTO @derivedFileType
SELECT Id 
FROM 
    dbo.fnDerivedTypes(@fileTypeId, @tenant)
WHERE
    Id NOT IN (SELECT Id FROM dbo.fnDerivedTypes(@imageFileTypeId, @tenant))

SELECT 
	DISTINCT d.Data	
FROM	
	[dbo].Data_NVarChar d
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
            r.ToId IN (SELECT Id FROM @derivedFileType)
    )
";

        /// <summary>
        ///     The tenant source get entities command text
        /// </summary>
        public const string TenantSourceGetEntitiesCommandText = @"
SELECT UpgradeId FROM Entity WHERE TenantId = @tenant
";

		/// <summary>
		///     The tenant source get field data command text
		/// </summary>
		public const string TenantSourceGetFieldDataCommandText = @"
SELECT
	e.UpgradeId, f.UpgradeId, d.Data{1}
FROM
	Data_{0} d
JOIN
	Entity e ON e.TenantId = @tenant AND d.EntityId = e.Id
JOIN
	Entity f ON f.TenantId = @tenant AND d.FieldId = f.Id
WHERE
	d.TenantId = @tenant
";

		/// <summary>
		///     The tenant source get relationships command text
		/// </summary>
		public const string TenantSourceGetRelationshipsCommandText = @"
SELECT
	etype.UpgradeId, efrom.UpgradeId, eto.UpgradeId
FROM
	Relationship r
JOIN
	Entity efrom ON efrom.TenantId = @tenant AND efrom.Id = r.FromId
JOIN
	Entity eto ON eto.TenantId = @tenant AND eto.Id = r.ToId
JOIN
	Entity etype ON etype.TenantId = @tenant AND etype.Id = r.TypeId
WHERE
	r.TenantId = @tenant
";


        /// <summary>
        /// The app library upgrade file data hashes and extensions command text.
        /// </summary>
	    public const string AppLibraryUpgradeFileDataHashesAndFileExtensions = @"
UPDATE
	d
SET
	d.Data = b.NewDataHash
FROM
	AppData_NVarChar d
    JOIN {0} b ON b.OldDataHash = d.Data
WHERE
	d.AppVerUid = @appVerId AND
	d.FieldUid = @fileDataHashFieldId AND
	d.Data <> b.NewDataHash AND
    b.NewDataHash IS NOT NULL

INSERT INTO AppData_NVarChar (AppVerUid, EntityUid, FieldUid, Data)
SELECT 
    DISTINCT @appVerId, dhash.EntityUid, @fileExtensionFieldId, b.FileExtension
FROM 
    {0} b
    JOIN AppData_NVarChar dhash ON dhash.Data = b.NewDataHash
    LEFT JOIN AppData_NVarChar dext ON dext.AppVerUid = dhash.AppVerUid AND dext.EntityUid = dhash.EntityUid 
WHERE
    dhash.AppVerUid = @appVerId AND
    dhash.FieldUid = @fileDataHashFieldId AND    
    dext.FieldUid = @fileExtensionFieldId AND
    dext.EntityUid IS NULL AND
    b.FileExtension IS NOT NULL

UPDATE	
    dext
SET
	dext.Data = b.FileExtension
FROM
	{0} b
    JOIN AppData_NVarChar dhash ON dhash.Data = b.NewDataHash
    JOIN AppData_NVarChar dext ON dext.EntityUid = dhash.EntityUid
WHERE
    dhash.AppVerUid = @appVerId AND
    dhash.FieldUid = @fileDataHashFieldId AND    
    dext.AppVerUid = @appVerId AND
    dext.FieldUid = @fileExtensionFieldId AND
    dext.Data <> b.FileExtension AND
    b.FileExtension IS NOT NULL
";        

        /// <summary>
        /// The app library get binary data command text.
        /// </summary>
        public const string AppLibraryGetBinaryData = @"
;WITH derivedBinaryTypes ( EntityUid, AppVerUid ) AS
(
	SELECT 
        EntityUid, 
        AppVerUid
	FROM 
        AppEntity
	WHERE 
        EntityUid = @imageFileTypeId

	UNION ALL

	SELECT
		r.FromUid,
		r.AppVerUid
	FROM
		derivedBinaryTypes d
	    JOIN AppRelationship r ON 
            d.EntityUid = r.ToUid AND            
			d.AppVerUid = r.AppVerUid AND
			r.TypeUid = @inheritsId
)
SELECT DISTINCT d.Data
FROM 
	AppData_NVarChar d
WHERE 
	d.FieldUid = @fileDataHashFieldId AND
    d.AppVerUid = @appVer AND
	EXISTS (
		SELECT 1 FROM AppRelationship r
		WHERE r.FromUid = d.EntityUid AND
			r.AppVerUid = @appVer AND
			r.TypeUid = @isOfTypeId AND
			r.ToUid IN (SELECT EntityUid FROM derivedBinaryTypes)
	)
";


        /// <summary>
        /// The app library get document data command text.
        /// </summary>
        public const string AppLibraryGetDocumentData = @"
;WITH derivedBinaryTypes ( EntityUid, AppVerUid ) AS
(
	SELECT 
        EntityUid, 
        AppVerUid
	FROM 
        AppEntity
	WHERE 
        EntityUid = @imageFileTypeId

	UNION ALL

	SELECT
		r.FromUid,
		r.AppVerUid
	FROM
		derivedBinaryTypes d
	    JOIN AppRelationship r ON 
            d.EntityUid = r.ToUid AND            
			d.AppVerUid = r.AppVerUid AND
			r.TypeUid = @inheritsId
),
derivedFileTypes ( EntityUid, AppVerUid ) AS
(
    SELECT 
        EntityUid, 
        AppVerUid
	FROM 
        AppEntity
	WHERE 
        EntityUid = @fileTypeId	

	UNION ALL

	SELECT
		r.FromUid,
		r.AppVerUid
	FROM
		derivedFileTypes d
	    JOIN AppRelationship r ON 
            d.EntityUid = r.ToUid AND            
			d.AppVerUid = r.AppVerUid AND        
			r.TypeUid = @inheritsId
)
SELECT DISTINCT d.Data
FROM 
	AppData_NVarChar d
WHERE 
	d.FieldUid = @fileDataHashFieldId AND
    d.AppVerUid = @appVer AND
	EXISTS (
		SELECT 1 FROM AppRelationship r
		WHERE r.FromUid = d.EntityUid AND
			r.AppVerUid = @appVer AND
			r.TypeUid = @isOfTypeId AND
			r.ToUid IN (SELECT EntityUid FROM derivedFileTypes
                        EXCEPT
                        SELECT EntityUid FROM derivedBinaryTypes)
	)
";

        /// <summary>
        /// The load file data hashes command.
        /// </summary>
	    public const string LoadFileDataHashesCommandText = @"
SELECT 
    d.Data 
FROM
	Data_Alias fdha
JOIN Data_NVarChar d ON
	d.FieldId = fdha.EntityId AND 
	d.TenantId = fdha.TenantId			
WHERE
	fdha.Namespace = 'core' AND 
	fdha.Data = 'fileDataHash' AND  
	fdha.AliasMarkerId = 0
";
	}
}