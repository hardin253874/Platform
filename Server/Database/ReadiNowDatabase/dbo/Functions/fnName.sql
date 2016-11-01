-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE FUNCTION [dbo].[fnName]
(
	-- Add the parameters for the function here
	@id bigint
)
RETURNS nvarchar(max)
AS
BEGIN

	RETURN dbo.fnFieldNVarCharA(@id, 'name')

END
