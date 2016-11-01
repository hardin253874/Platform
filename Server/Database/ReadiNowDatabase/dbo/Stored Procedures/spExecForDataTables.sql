-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE PROCEDURE [dbo].[spExecForDataTables]
	@sql NVARCHAR( MAX ),
	@excludedTableNames NVARCHAR(MAX) = NULL
AS
BEGIN
	-- Run the specified query once for each data table.
	-- Any '?' in the query will be substituted with the current table name.
	-- E.g.   exec spExecForDataTables 'delete from ?'

	SET NOCOUNT ON;
	
	DECLARE @includedTables TABLE ( Name NVARCHAR( 128 ) )
	DECLARE @excludedTables TABLE ( Excluded NVARCHAR( MAX ) )

	INSERT INTO
		@excludedTables ( Excluded )
	SELECT
		LTRIM( RTRIM( NString ) )
	FROM
		dbo.fnListToTable( @excludedTableNames, DEFAULT, DEFAULT, DEFAULT )

	DECLARE @tableName NVARCHAR(MAX)
	DECLARE @curSql NVARCHAR(MAX)

	INSERT INTO
		@includedTables
	SELECT
		s.name
	FROM
		sysobjects s
	LEFT JOIN
		@excludedTables e ON
			LOWER( s.name ) = LOWER( e.Excluded )
	WHERE
		s.xtype = CAST( 'U' AS NCHAR( 2 ) ) AND
		s.name LIKE 'Data_%' AND
		e.Excluded IS NULL
	
	-- Open cursor
	DECLARE Data_Tables CURSOR FAST_FORWARD FOR
		SELECT
			Name
		FROM
			@includedTables

	OPEN Data_Tables;
	
	-- Visit each table
	FETCH NEXT FROM
		Data_Tables
	INTO
		@tableName;

	WHILE @@fetch_status = 0
	BEGIN
		SET @curSql = replace( @sql, '?', @tableName )

		IF @tableName = 'Data_Alias'
		BEGIN
			SET @curSql = REPLACE( @curSql, '{{,ExtraFields}}', ',Namespace,AliasMarkerId' )
			SET @curSql = REPLACE( @curSql, '{{,d.ExtraFields}}', ',d.Namespace,d.AliasMarkerId' )
		END
		ELSE
		BEGIN			
			SET @curSql = REPLACE( @curSql, '{{,ExtraFields}}', '' )
			SET @curSql = REPLACE( @curSql, '{{,d.ExtraFields}}', '' )
		END
			
		EXEC sp_executesql @curSql

		FETCH NEXT FROM
			Data_Tables
		INTO
			@tableName;
	END;

	CLOSE Data_Tables;
	DEALLOCATE Data_Tables;
END