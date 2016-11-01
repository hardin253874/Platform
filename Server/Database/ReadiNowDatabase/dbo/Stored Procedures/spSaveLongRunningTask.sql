-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE PROC [dbo].[spSaveLongRunningTask]
    @taskId UNIQUEIDENTIFIER,
    @status NVARCHAR( 50 ),
    @additionalInfo NVARCHAR( MAX )
AS              

SET NOCOUNT ON

IF ( (
	SELECT
		1
	FROM
		[dbo].LongRunningTask
	WHERE
		TaskId = @taskId
	) > 0 )
BEGIN
    UPDATE
		LongRunningTask
	SET
		LongRunningTask.[Status] = @status,
		LongRunningTask.[AdditionalInfo] = @additionalInfo
	WHERE
		LongRunningTask.[TaskId] = @taskId
END
ELSE
BEGIN
	INSERT INTO
		[dbo].[LongRunningTask] ( TaskId, Status,AdditionalInfo )
	VALUES
		( @taskId, @status,@additionalInfo )
END

SET NOCOUNT OFF