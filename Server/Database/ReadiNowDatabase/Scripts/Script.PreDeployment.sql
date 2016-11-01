-- Copyright 2011-2016 Global Software Innovation Pty Ltd

/*
 Pre-Deployment Script Template							
--------------------------------------------------------------------------------------
 This file contains SQL statements that will be executed before the build script.	
 Use SQLCMD syntax to include a file in the pre-deployment script.			
 Example:      :r .\myfile.sql								
 Use SQLCMD syntax to reference a variable in the pre-deployment script.		
 Example:      :setvar TableName MyTable							
               SELECT * FROM [$(TableName)]					
--------------------------------------------------------------------------------------
*/

DECLARE @cmd nvarchar(MAX)
set @cmd = N'IF NOT EXISTS ( SELECT 1 from sys.symmetric_keys where name like ''%DatabaseMasterKey%'' )
BEGIN
	CREATE MASTER KEY ENCRYPTION BY PASSWORD = ''This key will be replaced in production - U1# squirrel noise''
END'
EXEC sp_executesql @cmd

GO