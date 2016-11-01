-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE TABLE [dbo].[QRTZ_LOCKS] (
    [SCHED_NAME] NVARCHAR (100) NOT NULL,
    [LOCK_NAME]  NVARCHAR (40)  NOT NULL,
    CONSTRAINT [PK_QRTZ_LOCKS] PRIMARY KEY CLUSTERED ([SCHED_NAME] ASC, [LOCK_NAME] ASC)
);

