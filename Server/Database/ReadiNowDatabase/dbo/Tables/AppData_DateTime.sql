-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE TABLE [dbo].[AppData_DateTime] (
	[Id]		BIGINT IDENTITY (1, 1) NOT NULL,
    [AppVerUid] UNIQUEIDENTIFIER NOT NULL,
    [EntityUid] UNIQUEIDENTIFIER NOT NULL,
    [FieldUid]  UNIQUEIDENTIFIER NOT NULL,
    [Data]      DATETIME         NULL,
    CONSTRAINT [PK_AppData_DateTime] PRIMARY KEY NONCLUSTERED ([AppVerUid] ASC, [EntityUid] ASC, [FieldUid] ASC)
);


GO
CREATE UNIQUE CLUSTERED INDEX [IX_AppData_DateTime_Id]
    ON [dbo].[AppData_DateTime]([Id] ASC);