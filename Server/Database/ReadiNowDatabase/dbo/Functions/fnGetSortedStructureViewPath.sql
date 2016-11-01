-- Copyright 2011-2016 Global Software Innovation Pty Ltd
CREATE FUNCTION [dbo].[fnGetSortedStructureViewPath]
(
	@path NVARCHAR(MAX),
	@delimiter NVARCHAR(1),
	@pathDelimiter NVARCHAR(1)
)
RETURNS NVARCHAR(MAX)
AS
BEGIN
	DECLARE @result NVARCHAR(MAX)
	DECLARE @subPath NVARCHAR(MAX)
	DECLARE @index INT
	DECLARE @pathTable TABLE (Value NVARCHAR(MAX))
	
	-- Return input value if null or empty
	IF @path IS NULL OR LEN(@path) = 0 
	BEGIN
		RETURN @path
	END
	
	-- Split the input path and add leaf paths into the pathTable
	WHILE LEN(@path) > 0
	BEGIN
		SET @index = CHARINDEX(@pathDelimiter, @path)

		IF @index > 0
		BEGIN
			SET @subPath = SUBSTRING(@path, 0, @index)

			IF CHARINDEX(@delimiter + @subPath, @path) = 0
			BEGIN
				INSERT INTO @pathTable (Value)
				VALUES(@subPath)
			END

			SET @path = SUBSTRING(@path, @index + 1, LEN(@path))
		END
		ELSE
		BEGIN
			INSERT INTO @pathTable (Value)
			VALUES(@path)

			SET @path = ''
		END
	END

	-- Sort paths and create path
	SELECT @result = COALESCE( @result + @pathDelimiter, '' ) + p.[Value]
	FROM @pathTable p				
	ORDER BY p.[Value]		
	
	RETURN @result
END