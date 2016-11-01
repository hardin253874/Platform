-- Copyright 2011-2016 Global Software Innovation Pty Ltd

/*
Post-Deployment Script Template							
--------------------------------------------------------------------------------------
 This file contains SQL statements that will be appended to the build script.		
 Use SQLCMD syntax to include a file in the post-deployment script.			
 Example:      :r .\myfile.sql								
 Use SQLCMD syntax to reference a variable in the post-deployment script.		
 Example:      :setvar TableName MyTable							
               SELECT * FROM [$(TableName)]					
--------------------------------------------------------------------------------------
*/

IF ( SELECT size / 128 FROM sys.sysfiles WHERE name = '$(DatabaseName)' ) < 2048
BEGIN
	ALTER DATABASE [$(DatabaseName)]
		MODIFY FILE
		(
			NAME = [$(DatabaseName)],
			SIZE = 2GB,
			MAXSIZE = UNLIMITED,
			FILEGROWTH = 100MB
		)
END

GO

IF ( SELECT size / 128 FROM sys.sysfiles WHERE name = '$(DatabaseName)_log' ) < 4096
BEGIN
	ALTER DATABASE [$(DatabaseName)]
		MODIFY FILE
		(
			NAME = [$(DatabaseName)_log],
			SIZE = 4GB,
			MAXSIZE = UNLIMITED,
			FILEGROWTH = 100MB
		)
END