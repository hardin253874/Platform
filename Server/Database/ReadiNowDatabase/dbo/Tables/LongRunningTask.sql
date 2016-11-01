-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE TABLE [dbo].[LongRunningTask] (
    [TaskId]         UNIQUEIDENTIFIER NOT NULL,
    [Status]         NVARCHAR (50)    NULL,
    [AdditionalInfo] XML              NULL,
    CONSTRAINT [PK_LongRunningTask] PRIMARY KEY CLUSTERED ([TaskId] ASC)
);

