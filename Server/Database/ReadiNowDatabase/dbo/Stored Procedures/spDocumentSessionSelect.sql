-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE PROCEDURE [dbo].[spDocumentSessionSelect]
	@sessionId NVARCHAR( 255 )
AS
BEGIN
	SET NOCOUNT ON

	DECLARE @dataHash NVARCHAR( 256 ) = ''
	DECLARE @sessionDateAge DATETIME2 = DATEADD( SECOND, -30000, SYSUTCDATETIME( ) )

	SELECT TOP 1
		@dataHash = DataHash
	FROM
		[dbo].Document_Session
	WHERE
		SessionId = @sessionId AND
		SessionTime > @sessionDateAge

	if ( @dataHash <> '' )
	BEGIN
		-- delete this entity so that it cannot be requested again
		DELETE FROM
			[dbo].[Document_Session]
		WHERE
			DataHash = @dataHash AND
			SessionId = @sessionId
	END

	-- General housekeeping to remove stale records if any.
	DELETE FROM
		[dbo].[Document_Session]
	WHERE
		SessionTime < @sessionDateAge

	SELECT
		@dataHash
END
