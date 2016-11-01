-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE FUNCTION [dbo].[fnReverseDelimitedList]
(
	@value NVARCHAR(MAX),
	@delimiter NVARCHAR(1)
)
RETURNS NVARCHAR(MAX)
AS
BEGIN
	DECLARE @result NVARCHAR(MAX)
	DECLARE @index INT
	
	-- Return input value if null or empty
	IF @value IS NULL OR LEN(@value) = 0 
	BEGIN
		RETURN @value
	END

	-- Reverse the string
	WHILE LEN(@value) > 0
	BEGIN
		SET @index = CHARINDEX(@delimiter, @value)

		IF @index > 0
		BEGIN
			SET @result = SUBSTRING(@value, 0, @index) + COALESCE(@delimiter + @result, '')
			SET @value = SUBSTRING(@value, @index + 1, LEN(@value))
		END
		ELSE
		BEGIN
			SET @result = @value + COALESCE(@delimiter + @result, '')
			SET @value = ''
		END
	END
	
	RETURN @result
END