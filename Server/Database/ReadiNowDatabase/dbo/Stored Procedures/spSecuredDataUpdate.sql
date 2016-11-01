-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE PROCEDURE [dbo].[spSecuredDataUpdate]
	@secureId UNIQUEIDENTIFIER,
	@data nvarchar(MAX),
	@context VARCHAR( 128 ) = NULL
AS
BEGIN

	IF ( @context IS NULL )
	BEGIN
		SET @context = OBJECT_NAME(@@PROCID)
	END

	IF ( @context IS NOT NULL )
	BEGIN
		DECLARE @contextInfo VARBINARY(128) = CONVERT( VARBINARY(128), @context )
		SET CONTEXT_INFO @contextInfo
	END

	OPEN SYMMETRIC KEY key_Secured DECRYPTION BY CERTIFICATE cert_keyProtection

	-- Update the row if it exists.    
    UPDATE [dbo].Secured
	SET Data = encryptbykey(key_guid('key_Secured'), @data)
	WHERE SecureId = @secureId
	
	CLOSE SYMMETRIC KEY key_Secured

END