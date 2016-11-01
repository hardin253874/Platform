-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE PROCEDURE [dbo].[spSecuredDataDelete]
	@secureId UNIQUEIDENTIFIER,
	@context VARCHAR( 128 ) = NULL
AS
BEGIN
	SET NOCOUNT ON

	IF ( @context IS NULL )
	BEGIN
		SET @context = OBJECT_NAME(@@PROCID)
	END

	IF ( @context IS NOT NULL )
	BEGIN
		DECLARE @contextInfo VARBINARY(128) = CONVERT( VARBINARY(128), @context )
		SET CONTEXT_INFO @contextInfo
	END
	
	DELETE FROM  [dbo].Secured
	WHERE SecureId = @secureId

END