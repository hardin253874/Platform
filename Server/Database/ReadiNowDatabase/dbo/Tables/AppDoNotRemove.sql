-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE TABLE [dbo].[AppDoNotRemove] (
    [Id]        BIGINT           IDENTITY (1, 1) NOT NULL,
    [AppVerUid] UNIQUEIDENTIFIER NULL,
    [EntityUid] UNIQUEIDENTIFIER NULL,
    CONSTRAINT [PK_AppDoNotRemove] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [IX_AppDoNotRemove] UNIQUE NONCLUSTERED ([AppVerUid] ASC, [EntityUid] ASC),
    CONSTRAINT [IX_AppDoNotRemove_1] UNIQUE NONCLUSTERED ([EntityUid] ASC, [AppVerUid] ASC)
);
