-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE TABLE [dbo].[ImportMap] (
    [BatchId]          BIGINT           NOT NULL,
    [ImportedEntityId] BIGINT           NOT NULL,
    [UpgradeId]        UNIQUEIDENTIFIER NULL,
    [State]            NVARCHAR (10)    NULL,
    [EntityId]         BIGINT           NULL,
    [Alias]            NVARCHAR (100)   NULL,
    [Namespace]        NVARCHAR (100)   NULL,
    [CompletedState]   NVARCHAR (10)    NULL,
    CONSTRAINT [PK_ImportMap] PRIMARY KEY CLUSTERED ([BatchId] ASC, [ImportedEntityId] ASC)
);

