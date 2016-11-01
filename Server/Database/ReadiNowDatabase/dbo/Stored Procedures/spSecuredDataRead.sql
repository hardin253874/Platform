-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE PROCEDURE [dbo].[spSecuredDataRead]
	@secureId UNIQUEIDENTIFIER
AS
BEGIN
	SET NOCOUNT ON

	OPEN SYMMETRIC KEY key_Secured DECRYPTION BY CERTIFICATE cert_keyProtection

	SELECT CONVERT(nvarchar(MAX), DECRYPTBYKEY(s.Data))
    FROM dbo.Secured s
    WHERE s.SecureId = @secureId

	CLOSE SYMMETRIC KEY key_Secured

END