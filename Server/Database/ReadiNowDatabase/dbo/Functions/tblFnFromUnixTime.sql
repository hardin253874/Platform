-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE FUNCTION [dbo].[tblFnFromUnixTime] (
	@timestamp INTEGER
)
RETURNS TABLE
AS
RETURN
(
	SELECT
		DATEADD( second, @timestamp, {d '1970-01-01'} ) [Time]
)