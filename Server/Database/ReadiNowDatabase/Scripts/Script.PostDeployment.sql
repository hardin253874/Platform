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

/* NOTE: If you are getting an error in VS here... Enable the SQLCMD Mode button above */

/* Ensure the file size and growth settings are correct */
:r ".\File\FileSettings.sql"

/* Populate the Country Table with seed data */
:r ".\Data\Country.sql"

/* Populate the TimeZoneMap Table with seed data */
:r ".\Data\TimeZoneMap.sql"

/* Populate the Zone Table with seed data */
:r ".\Data\Zone.sql"

/* Populate the Zone Table with seed data */
:r ".\Data\TimeZone.sql"

/* Populate Table Names used in Licensing */
:r ".\Data\Lic_Table_Name.sql"

/* Create a unique identifier for the database */
:r ".\Data\RNDB.sql"

/* Create the 'Software Platform Nightly Statistics Update' job */
:r ".\Jobs\SoftwarePlatformNightlyStatisticsUpdate.sql"

/* Create the 'Software Platform Cycle Error Logs' job */
:r ".\Jobs\CycleErrorLogs.sql"

/* Create the 'Licensing Metrics Update' job */
:r ".\Jobs\LicensingMetricsUpdate.sql"
