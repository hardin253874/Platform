-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE TABLE [dbo].[AppData_NVarChar] (
	[Id]		BIGINT IDENTITY (1, 1) NOT NULL,
    [AppVerUid] UNIQUEIDENTIFIER NOT NULL,
    [EntityUid] UNIQUEIDENTIFIER NOT NULL,
    [FieldUid]  UNIQUEIDENTIFIER NOT NULL,
    [Data]      NVARCHAR (MAX)   COLLATE Latin1_General_CI_AI NULL,
    CONSTRAINT [PK_AppData_NVarChar] PRIMARY KEY NONCLUSTERED ([AppVerUid] ASC, [EntityUid] ASC, [FieldUid] ASC)
);


GO
CREATE UNIQUE CLUSTERED INDEX [IX_AppData_NVarChar_Id]
    ON [dbo].[AppData_NVarChar]([Id] ASC);	


GO
CREATE NONCLUSTERED INDEX [AppData_NVarChar_EntityUid]
    ON [dbo].[AppData_NVarChar]([EntityUid] ASC)
    INCLUDE([Data]);

