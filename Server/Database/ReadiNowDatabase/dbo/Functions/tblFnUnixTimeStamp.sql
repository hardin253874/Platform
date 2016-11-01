-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE FUNCTION [dbo].[tblFnUnixTimeStamp] (
	@ctimestamp DATETIME
)
RETURNS TABLE
AS
RETURN
(
	SELECT
		DATEDIFF( SECOND, {d '1970-01-01'}, @ctimestamp) [TimeStamp]
)