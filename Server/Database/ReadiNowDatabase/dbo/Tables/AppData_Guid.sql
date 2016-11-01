-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE TABLE [dbo].[AppData_Guid] (
	[Id]		BIGINT IDENTITY (1, 1) NOT NULL,
    [AppVerUid] UNIQUEIDENTIFIER NOT NULL,
    [EntityUid] UNIQUEIDENTIFIER NOT NULL,
    [FieldUid]  UNIQUEIDENTIFIER NOT NULL,
    [Data]      UNIQUEIDENTIFIER NULL,
    CONSTRAINT [PK_AppData_Guid] PRIMARY KEY NONCLUSTERED ([AppVerUid] ASC, [EntityUid] ASC, [FieldUid] ASC)
);


GO
CREATE UNIQUE CLUSTERED INDEX [IX_AppData_Guid_Id]
    ON [dbo].[AppData_Guid]([Id] ASC);