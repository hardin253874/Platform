-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE TABLE [dbo].[EntityBatch] (
    [BatchId]  BIGINT NOT NULL,
    [EntityId] BIGINT NOT NULL,
    CONSTRAINT [PK_EntityBatch] PRIMARY KEY CLUSTERED ([BatchId] ASC, [EntityId] ASC)
);




GO
CREATE NONCLUSTERED INDEX [IDX_EntityBatch_EntityId]
    ON [dbo].[EntityBatch]([EntityId] ASC);

