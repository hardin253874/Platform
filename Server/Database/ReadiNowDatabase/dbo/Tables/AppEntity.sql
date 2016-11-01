-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE TABLE [dbo].[AppEntity] (
    [Id]        BIGINT           IDENTITY (1, 1) NOT NULL,
    [AppVerUid] UNIQUEIDENTIFIER NULL,
    [EntityUid] UNIQUEIDENTIFIER NULL,
    CONSTRAINT [PK_AppEntity] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [IX_AppEntity] UNIQUE NONCLUSTERED ([AppVerUid] ASC, [EntityUid] ASC),
    CONSTRAINT [IX_AppEntity_1] UNIQUE NONCLUSTERED ([EntityUid] ASC, [AppVerUid] ASC)
);

