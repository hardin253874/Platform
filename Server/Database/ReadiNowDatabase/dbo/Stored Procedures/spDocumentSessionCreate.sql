-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE PROCEDURE [dbo].[spDocumentSessionCreate]
	@dataHash NVARCHAR( 256 ),
	@sessionId NVARCHAR( 255 )
AS
BEGIN

SET NOCOUNT ON

INSERT INTO
	[dbo].[Document_Session] (
		SessionId,
		DataHash,
		SessionTime )
VALUES (
	@sessionId,
	@dataHash,
	SYSUTCDATETIME( )
)

END

