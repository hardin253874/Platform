-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE TABLE [dbo].[Hist_Relationship] (
    [_id]           BIGINT  IDENTITY (1, 1) NOT NULL,
    [TransactionId] BIGINT  NOT NULL,
    [Action]        TINYINT NOT NULL,
    [Id]            BIGINT  NOT NULL,
    [TenantId]      BIGINT  NOT NULL,
    [TypeId]        BIGINT  NOT NULL,
    [FromId]        BIGINT  NOT NULL,
    [ToId]          BIGINT  NOT NULL
);



GO
CREATE NONCLUSTERED INDEX [IX_Hist_Relationship]
    ON [dbo].[Hist_Relationship]([TransactionId] ASC)
    INCLUDE([_id], [Action], [Id], [TenantId], [TypeId], [FromId], [ToId]);



GO
CREATE CLUSTERED INDEX [IDX_Hist_Relationship]
    ON [dbo].[Hist_Relationship]([_id] ASC);