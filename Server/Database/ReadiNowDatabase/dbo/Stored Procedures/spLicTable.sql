-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE PROCEDURE [dbo].[spLicTable]
AS
BEGIN

DECLARE @idx BIGINT

SELECT TOP 1
	@idx = [Id]
FROM
	[dbo].[Lic_Index]
ORDER BY
	[Timestamp] DESC

DECLARE @fraglist TABLE
(
	ObjectName VARCHAR(255),
	ObjectId INT,
	IndexName VARCHAR(255),
	IndexId INT,
	Lvl INT,CountPages INT,
	CountRows INT,
	MinRecSize INT,
	MaxRecSize INT,
	AvgRecSize INT,
	ForRecCount INT,
	Extents INT,
	ExtentSwitches INT,
	AvgFreeBytes INT,
	AvgPageDensity INT,
	ScanDensity DECIMAL,
	BestCount INT,
	ActualCount INT,
	LogicalFrag DECIMAL,
	ExtentFrag DECIMAL
)

INSERT INTO
	@fraglist
EXEC ( 'DBCC SHOWCONTIG(''Data_Alias'') WITH TABLERESULTS' )

INSERT INTO
	@fraglist
EXEC ( 'DBCC SHOWCONTIG(''Data_Bit'') WITH TABLERESULTS')

INSERT INTO
	@fraglist
EXEC ( 'DBCC SHOWCONTIG(''Data_DateTime'') WITH TABLERESULTS' )

INSERT INTO
	@fraglist
EXEC ( 'DBCC SHOWCONTIG(''Data_Decimal'') WITH TABLERESULTS' )

INSERT INTO
	@fraglist
EXEC ( 'DBCC SHOWCONTIG(''Data_Guid'') WITH TABLERESULTS' )

INSERT INTO
	@fraglist
EXEC ( 'DBCC SHOWCONTIG(''Data_Int'') WITH TABLERESULTS' )

INSERT INTO
	@fraglist
EXEC ( 'DBCC SHOWCONTIG(''Data_NVarChar'') WITH TABLERESULTS' )

INSERT INTO
	@fraglist
EXEC ( 'DBCC SHOWCONTIG(''Data_Xml'') WITH TABLERESULTS' )

INSERT INTO
	@fraglist
EXEC ( 'DBCC SHOWCONTIG(''Entity'') WITH TABLERESULTS' )

INSERT INTO
	@fraglist
EXEC ( 'DBCC SHOWCONTIG(''Relationship'') WITH TABLERESULTS' )

INSERT INTO
	[dbo].[Lic_Table]
SELECT
	@idx,
	l.[Id],
	CountRows,
	MinRecSize,
	MaxRecSize,
	AvgRecSize
FROM
	@fraglist f
JOIN
	[dbo].[Lic_Table_Name] l ON
		f.ObjectName = l.[Name]

END
