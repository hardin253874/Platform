-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE PROCEDURE [dbo].[spSecuredDataCreate]
	@tenantId BIGINT,
	@context NVARCHAR(100),
	@secureId UNIQUEIDENTIFIER,
	@data NVARCHAR(MAX)
AS
BEGIN
	SET NOCOUNT ON

	OPEN SYMMETRIC KEY key_Secured DECRYPTION BY CERTIFICATE cert_keyProtection

	INSERT INTO [dbo].Secured (TenantId, Context, SecureId, Data)
		VALUES (@tenantId, @context, @secureId, encryptbykey(key_guid('key_Secured'), @data))

	CLOSE SYMMETRIC KEY key_Secured

END