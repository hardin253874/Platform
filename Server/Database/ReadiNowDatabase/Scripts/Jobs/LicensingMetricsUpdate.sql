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

DECLARE @LicensingJobId BINARY( 16 )

SELECT @LicensingJobId = job_id FROM msdb.dbo.sysjobs WHERE (name = N'$(DatabaseName) Licensing Metrics Update')

IF (@LicensingJobId IS NOT NULL)
BEGIN
    EXEC msdb.dbo.sp_delete_job @LicensingJobId
END

GO

/****** Object:  Job [Licensing Metrics Update]    Script Date: 6/05/2015 10:55:20 AM ******/
BEGIN TRANSACTION

DECLARE @ReturnCode INT = 0

/****** Object:  JobCategory [Database Maintenance]    Script Date: 6/05/2015 10:55:20 AM ******/
IF NOT EXISTS (SELECT name FROM msdb.dbo.syscategories WHERE name=N'Database Maintenance' AND category_class=1)
BEGIN
	EXEC @ReturnCode = msdb.dbo.sp_add_category @class=N'JOB', @type=N'LOCAL', @name=N'Database Maintenance'

	IF (@@ERROR <> 0 OR @ReturnCode <> 0)
		GOTO QuitWithRollback
END

DECLARE @LicensingJobId BINARY( 16 )

EXEC @ReturnCode =  msdb.dbo.sp_add_job @job_name=N'$(DatabaseName) Licensing Metrics Update', 
		@enabled=1, 
		@notify_level_eventlog=0, 
		@notify_level_email=0, 
		@notify_level_netsend=0, 
		@notify_level_page=0, 
		@delete_level=0, 
		@description=N'Collect licensing metrics from the ''$(DatabaseName)'' database daily.', 
		@category_name=N'Database Maintenance', 
		@owner_login_name=N'sa', @job_id = @LicensingJobId OUTPUT

IF (@@ERROR <> 0 OR @ReturnCode <> 0)
	GOTO QuitWithRollback

/****** Object:  Step [Create Index]    Script Date: 12/05/2015 10:36:37 AM ******/
EXEC @ReturnCode = msdb.dbo.sp_add_jobstep @job_id=@LicensingJobId, @step_name=N'Create Index', 
		@step_id=1, 
		@cmdexec_success_code=0, 
		@on_success_action=3, 
		@on_success_step_id=0, 
		@on_fail_action=2, 
		@on_fail_step_id=0, 
		@retry_attempts=0, 
		@retry_interval=0, 
		@os_run_priority=0, @subsystem=N'TSQL', 
		@command=N'INSERT INTO [dbo].[Lic_Index] DEFAULT VALUES', 
		@database_name=N'$(DatabaseName)', 
		@flags=0
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
/****** Object:  Step [Get Table Metrics]    Script Date: 20/05/2015 10:42:11 AM ******/
EXEC @ReturnCode = msdb.dbo.sp_add_jobstep @job_id=@LicensingJobId, @step_name=N'Get Table Metrics', 
		@step_id=2, 
		@cmdexec_success_code=0, 
		@on_success_action=3, 
		@on_success_step_id=0, 
		@on_fail_action=2, 
		@on_fail_step_id=0, 
		@retry_attempts=0, 
		@retry_interval=0, 
		@os_run_priority=0, @subsystem=N'TSQL', 
		@command=N'EXECUTE [dbo].[spLicTable]', 
		@database_name=N'$(DatabaseName)', 
		@flags=0
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
/****** Object:  Step [Get Tenants]    Script Date: 20/05/2015 11:25:38 AM ******/
EXEC @ReturnCode = msdb.dbo.sp_add_jobstep @job_id=@LicensingJobId, @step_name=N'Get Tenants', 
		@step_id=3, 
		@cmdexec_success_code=0, 
		@on_success_action=3, 
		@on_success_step_id=0, 
		@on_fail_action=2, 
		@on_fail_step_id=0, 
		@retry_attempts=0, 
		@retry_interval=0, 
		@os_run_priority=0, @subsystem=N'TSQL', 
		@command=N'EXECUTE [dbo].[spLicTenant]', 
		@database_name=N'$(DatabaseName)', 
		@flags=0
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
/****** Object:  Step [Get Applications]    Script Date: 20/05/2015 10:42:11 AM ******/
EXEC @ReturnCode = msdb.dbo.sp_add_jobstep @job_id=@LicensingJobId, @step_name=N'Get Applications', 
		@step_id=4, 
		@cmdexec_success_code=0, 
		@on_success_action=3, 
		@on_success_step_id=0, 
		@on_fail_action=2, 
		@on_fail_step_id=0, 
		@retry_attempts=0, 
		@retry_interval=0, 
		@os_run_priority=0, @subsystem=N'TSQL', 
		@command=N'EXECUTE [dbo].[spLicApplication]', 
		@database_name=N'$(DatabaseName)', 
		@flags=0
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
/****** Object:  Step [Object Counts Update]    Script Date: 20/05/2015 10:42:11 AM ******/
EXEC @ReturnCode = msdb.dbo.sp_add_jobstep @job_id=@LicensingJobId, @step_name=N'Object Counts Update', 
		@step_id=5, 
		@cmdexec_success_code=0, 
		@on_success_action=3, 
		@on_success_step_id=0, 
		@on_fail_action=2, 
		@on_fail_step_id=0, 
		@retry_attempts=0, 
		@retry_interval=0, 
		@os_run_priority=0, @subsystem=N'TSQL', 
		@command=N'EXECUTE [dbo].[spLicObjectCount]', 
		@database_name=N'$(DatabaseName)', 
		@flags=0
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
/****** Object:  Step [Users Update]    Script Date: 20/05/2015 1:13:52 PM ******/
EXEC @ReturnCode = msdb.dbo.sp_add_jobstep @job_id=@LicensingJobId, @step_name=N'Users Update', 
		@step_id=6, 
		@cmdexec_success_code=0, 
		@on_success_action=3, 
		@on_success_step_id=0, 
		@on_fail_action=2, 
		@on_fail_step_id=0, 
		@retry_attempts=0, 
		@retry_interval=0, 
		@os_run_priority=0, @subsystem=N'TSQL', 
		@command=N'EXECUTE [dbo].[spLicUser]', 
		@database_name=N'$(DatabaseName)', 
		@flags=0
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
/****** Object:  Step [Records Update]    Script Date: 20/05/2015 10:42:12 AM ******/
EXEC @ReturnCode = msdb.dbo.sp_add_jobstep @job_id=@LicensingJobId, @step_name=N'Records Update', 
		@step_id=7, 
		@cmdexec_success_code=0, 
		@on_success_action=3, 
		@on_success_step_id=0, 
		@on_fail_action=2, 
		@on_fail_step_id=0, 
		@retry_attempts=0, 
		@retry_interval=0, 
		@os_run_priority=0, @subsystem=N'TSQL', 
		@command=N'EXECUTE [dbo].[spLicRecord]', 
		@database_name=N'$(DatabaseName)', 
		@flags=0
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
/****** Object:  Step [File Counts Update]    Script Date: 21/05/2015 1:12:44 PM ******/
EXEC @ReturnCode = msdb.dbo.sp_add_jobstep @job_id=@LicensingJobId, @step_name=N'File Counts Update', 
		@step_id=8, 
		@cmdexec_success_code=0, 
		@on_success_action=3, 
		@on_success_step_id=0, 
		@on_fail_action=2, 
		@on_fail_step_id=0, 
		@retry_attempts=0, 
		@retry_interval=0, 
		@os_run_priority=0, @subsystem=N'TSQL', 
		@command=N'EXECUTE [dbo].[spLicFileCount]', 
		@database_name=N'$(DatabaseName)', 
		@flags=0
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
/****** Object:  Step [Workflows Update]    Script Date: 21/05/2015 1:12:44 PM ******/
EXEC @ReturnCode = msdb.dbo.sp_add_jobstep @job_id=@LicensingJobId, @step_name=N'Workflows Update', 
		@step_id=9, 
		@cmdexec_success_code=0, 
		@on_success_action=1, 
		@on_success_step_id=0, 
		@on_fail_action=2, 
		@on_fail_step_id=0, 
		@retry_attempts=0, 
		@retry_interval=0, 
		@os_run_priority=0, @subsystem=N'TSQL', 
		@command=N'EXECUTE [dbo].[spLicWorkflow]', 
		@database_name=N'$(DatabaseName)', 
		@flags=0
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback

EXEC @ReturnCode = msdb.dbo.sp_update_job @job_id = @LicensingJobId, @start_step_id = 1

IF (@@ERROR <> 0 OR @ReturnCode <> 0)
	GOTO QuitWithRollback

DECLARE @ScheduleId UNIQUEIDENTIFIER

SET @ScheduleId = NEWID()

EXEC @ReturnCode = msdb.dbo.sp_add_jobschedule @job_id=@LicensingJobId, @name=N'$(DatabaseName) Licensing Metrics Update Schedule', 
		@enabled=1, 
		@freq_type=4, 
		@freq_interval=1, 
		@freq_subday_type=1, 
		@freq_subday_interval=0, 
		@freq_relative_interval=0, 
		@freq_recurrence_factor=0, 
		@active_start_date=20150506, 
		@active_end_date=99991231, 
		@active_start_time=10000, 
		@active_end_time=235959, 
		@schedule_uid=@ScheduleId

IF (@@ERROR <> 0 OR @ReturnCode <> 0)
	GOTO QuitWithRollback

EXEC @ReturnCode = msdb.dbo.sp_add_jobserver @job_id = @LicensingJobId, @server_name = N'(local)'

IF (@@ERROR <> 0 OR @ReturnCode <> 0)
	GOTO QuitWithRollback

COMMIT TRANSACTION

GOTO EndSave

QuitWithRollback:
IF (@@TRANCOUNT > 0)
	ROLLBACK TRANSACTION

EndSave:
