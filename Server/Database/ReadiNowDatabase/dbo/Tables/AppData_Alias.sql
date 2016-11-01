-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE TABLE [dbo].[AppData_Alias] (
	[Id]			BIGINT IDENTITY (1, 1) NOT NULL,
    [AppVerUid]     UNIQUEIDENTIFIER NOT NULL,
    [EntityUid]     UNIQUEIDENTIFIER NOT NULL,
    [FieldUid]      UNIQUEIDENTIFIER NOT NULL,
    [Data]          NVARCHAR (100)   NULL,
    [Namespace]     NVARCHAR (100)   NULL,
    [AliasMarkerId] INT              NULL,
    CONSTRAINT [PK_AppData_Alias] PRIMARY KEY NONCLUSTERED ([AppVerUid] ASC, [EntityUid] ASC, [FieldUid] ASC)
);


GO
CREATE UNIQUE CLUSTERED INDEX [IX_AppData_Alias_Id]
    ON [dbo].[AppData_Alias]([Id] ASC);


GO
CREATE NONCLUSTERED INDEX [AppData_Alias_Data_Namespace_AliasMarkerId]
    ON [dbo].[AppData_Alias]([Data] ASC, [Namespace] ASC, [AliasMarkerId] ASC);


GO
CREATE NONCLUSTERED INDEX [AppData_Alias_EntityId]
    ON [dbo].[AppData_Alias]([EntityUid] ASC)
    INCLUDE([Data]);

