-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE PROC [dbo].[spDeleteLongRunningTask]
    @taskId UNIQUEIDENTIFIER
AS

SET NOCOUNT ON

DELETE FROM
	LongRunningTask
WHERE
	TaskId = @taskId
                
SET NOCOUNT OFF