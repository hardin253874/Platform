-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE FUNCTION [dbo].[fnListToTable]
(
    @list NVARCHAR( MAX ),
    @delimiter NCHAR( 1 ) = N',', 
	@nullDelimiter NVARCHAR( 40 ) = NULL, 
	@emptyDelimiter NVARCHAR( 40 ) = NULL 
)
RETURNS @Table TABLE (
	Position INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
	String VARCHAR( 4000 ),
	NString NVARCHAR( 2000 )
)
AS
BEGIN
	DECLARE @pos INT
	DECLARE @textPos INT
	DECLARE @blockLen SMALLINT
	DECLARE @tmpStr NVARCHAR( 4000 )
	DECLARE @leftover NVARCHAR( 4000 )
	DECLARE @tmpVal NVARCHAR( 4000 )
	DECLARE @startPos INT
  
	SET @textPos = 1
	SET @leftover = ''
                
	WHILE @textPos <= DATALENGTH( @list ) / 2
	BEGIN
		SET @blockLen = 4000 - DATALENGTH( @leftover ) / 2
                   
		IF @leftover = ''
		BEGIN
			SET @tmpStr = SUBSTRING( @list, @textPos, @blockLen )
		END
		ELSE
		BEGIN
			SET @tmpStr = @leftover + SUBSTRING( @list, @textPos, @blockLen )
		END
                   
		SET @textPos = @textPos + @blockLen
		SET @startPos = 0
		SET @pos = CHARINDEX( @delimiter, @tmpStr )

		WHILE @pos > 0
		BEGIN
			SET @tmpVal = SUBSTRING( @tmpStr, @startPos + 1, ( @pos - @startPos ) - 1 )

			IF @nullDelimiter IS NOT NULL AND @tmpVal = @nullDelimiter
			BEGIN
				SET @tmpVal = NULL
			END

			IF @emptyDelimiter IS NOT NULL AND @tmpVal = @emptyDelimiter
			BEGIN
				SET @tmpVal = ''
			END
		
			INSERT
				@Table ( String, NString )
			VALUES
				( @tmpVal, @tmpVal )
                      
			SET @startPos = @pos
			SET @pos = CHARINDEX( @delimiter, @tmpStr, @startPos + 1 )
		END

		SET @leftover = RIGHT( @tmpStr, DATALENGTH( @tmpStr ) / 2 - @startPos )
	END

	SET @tmpVal = @leftover

	IF @nullDelimiter IS NOT NULL AND @tmpVal = @nullDelimiter
	BEGIN
		SET @tmpVal = NULL
	END

	IF @emptyDelimiter IS NOT NULL AND @tmpVal = @emptyDelimiter
	BEGIN
		SET @tmpVal = ''
	END

	INSERT
		@Table ( String, NString )
	VALUES
		( @tmpVal, @tmpVal )

	RETURN
END