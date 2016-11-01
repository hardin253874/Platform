-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE Function [fnConvertToInt](@str NVARCHAR(1000))
RETURNS INT
AS
BEGIN
	DECLARE @result INT

    WHILE PATINDEX('%[^0-9]%', @str) > 0
    BEGIN
        SET @str = STUFF(@str, PATINDEX('%[^0-9]%', @str), 1, '')
    END

	IF (@str = '')
		SET @result = NULL
	ELSE
		SET @result = CAST(@str AS INT)

	RETURN @result
END