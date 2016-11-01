-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE FUNCTION [dbo].[fnFromUnixTime] (
@timestamp integer
)
RETURNS datetime
AS
BEGIN
  /* Function body */
  DECLARE @return datetime
   
  SELECT @return = DATEADD(second, @timestamp,{d '1970-01-01'});
   
  RETURN @return
END
