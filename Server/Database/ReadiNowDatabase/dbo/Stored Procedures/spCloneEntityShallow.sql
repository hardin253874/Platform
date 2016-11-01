-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE PROCEDURE [dbo].[spCloneEntityShallow]
	@entities dbo.EntityMapType READONLY,
	@tenantId BIGINT,
	@debug BIT = 0
AS
BEGIN

	DECLARE @debugCount INT

	SET NOCOUNT ON

	-----
	-- Store the mapping from source entity to destination entity.
	-----
	CREATE TABLE #ClonedEntities
	(
		SourceId BIGINT PRIMARY KEY,
		DestinationId BIGINT
	)
	
	-----
	-- Create the new entities storing the resulting mapping in the temporary table.
	-----
	INSERT INTO
		#ClonedEntities
		(
			SourceId,
			DestinationId
		)
	SELECT
		SourceId,
		DestinationId
	FROM
		@entities

	-----
	-- Clone data
	-----
	DECLARE @sql NVARCHAR(MAX) =
	'INSERT INTO ?
	(
		EntityId,
		TenantId,
		FieldId,
		Data
		{{,ExtraFields}}
	)
	SELECT
		c.DestinationId,
		d.TenantId,
		d.FieldId,
		d.Data
		{{,d.ExtraFields}}
	FROM
		? d
	INNER JOIN
		#ClonedEntities c ON d.EntityId = c.SourceId
	WHERE
		d.TenantId = ' + CAST( @tenantId AS NVARCHAR( MAX ) )

	-----					
	-- Run for every data table
	-----
	EXEC spExecForDataTables @sql, 'Data_Alias'

	SELECT *
	FROM #ClonedEntities 

	DROP TABLE #ClonedEntities
						
END