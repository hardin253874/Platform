-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE PROCEDURE spTrustForeignKeys
AS
BEGIN
	SET NOCOUNT ON;

    DECLARE @schema NVARCHAR( 255 )
	DECLARE @table NVARCHAR( 255 )
	DECLARE @foreignKey NVARCHAR( 255 )
	DECLARE @query NVARCHAR( MAX )

	DECLARE untrustedConstraints CURSOR FAST_FORWARD READ_ONLY
	FOR
	SELECT
		SCH.name
		,TBL.name
		,FK.name
	FROM
		sys.foreign_keys AS FK 
	INNER JOIN
		sys.objects AS TBL ON
			FK.parent_object_id = TBL.object_id 
	INNER JOIN
		sys.schemas AS SCH ON
			FK.schema_id = SCH.schema_id 
	 WHERE
		FK.is_not_trusted = 1 
	 ORDER BY
		SCH.name
		,TBL.name
		,FK.name

	OPEN untrustedConstraints

	FETCH NEXT FROM
		untrustedConstraints
	INTO
		@schema
		,@table
		,@foreignKey

	WHILE @@FETCH_STATUS = 0
	BEGIN
		SELECT @query = N'BEGIN TRY ALTER TABLE ' + QUOTENAME( @schema ) + N'.' + QUOTENAME( @table ) + 
					N' WITH CHECK CHECK CONSTRAINT ' + QUOTENAME( @foreignKey ) + N'; END TRY ' + CHAR( 13 ) + CHAR( 10 ) + 
		   N'BEGIN CATCH PRINT ERROR_MESSAGE(); END CATCH;'

		EXEC sp_executesql @query

		FETCH NEXT FROM
			untrustedConstraints
		INTO
			@schema
			,@table
			,@foreignKey
	END

	CLOSE untrustedConstraints
	DEALLOCATE untrustedConstraints
END