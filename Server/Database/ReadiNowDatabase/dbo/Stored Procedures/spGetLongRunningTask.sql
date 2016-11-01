-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE PROC [dbo].[spGetLongRunningTask]
  	@taskId UNIQUEIDENTIFIER
AS

SET NOCOUNT ON

SELECT
	LongRunningTask.[TaskId],
	LongRunningTask.[Status],
	LongRunningTask.[AdditionalInfo]
FROM
	LongRunningTask            
WHERE
	LongRunningTask.[TaskId] = @taskId

SET NOCOUNT OFF