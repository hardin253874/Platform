-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE TABLE [dbo].[AppData_Int] (
	[Id]		BIGINT IDENTITY (1, 1) NOT NULL,
    [AppVerUid] UNIQUEIDENTIFIER NOT NULL,
    [EntityUid] UNIQUEIDENTIFIER NOT NULL,
    [FieldUid]  UNIQUEIDENTIFIER NOT NULL,
    [Data]      INT              NULL,
    CONSTRAINT [PK_AppData_Int] PRIMARY KEY NONCLUSTERED ([AppVerUid] ASC, [EntityUid] ASC, [FieldUid] ASC)
);


GO
CREATE UNIQUE CLUSTERED INDEX [IX_AppData_Int_Id]
    ON [dbo].[AppData_Int]([Id] ASC);	