-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE TYPE [dbo].[EntityBatchType] AS TABLE (
    [BatchId]  BIGINT NOT NULL,
    [EntityId] BIGINT NOT NULL,
    PRIMARY KEY CLUSTERED ([BatchId] ASC, [EntityId] ASC));

