-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE SYMMETRIC KEY [key_Secured]
    AUTHORIZATION [dbo]
    WITH ALGORITHM = AES_256
    ENCRYPTION BY CERTIFICATE [cert_keyProtection];

