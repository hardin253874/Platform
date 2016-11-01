-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE PROCEDURE [dbo].[spSecuredDataReadTenant]
	@tenantId bigint
AS
BEGIN
	SET NOCOUNT ON

	OPEN SYMMETRIC KEY key_Secured DECRYPTION BY CERTIFICATE cert_keyProtection

	SELECT 
		s.SecureId as SecureId, 
		s.Context as Context, 
		CONVERT(nvarchar(MAX), DECRYPTBYKEY(s.Data)) as Data
    FROM dbo.Secured s
    WHERE s.TenantId = @tenantId

	CLOSE SYMMETRIC KEY key_Secured

END

