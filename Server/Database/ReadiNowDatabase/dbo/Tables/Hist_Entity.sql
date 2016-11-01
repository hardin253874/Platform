-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE TABLE [dbo].[Hist_Entity] (
    [_id]            BIGINT          IDENTITY (1, 1) NOT NULL,
    [TransactionId] BIGINT           NOT NULL,
    [Action]        TINYINT          NOT NULL,
    [Id]            BIGINT           NOT NULL,
    [TenantId]      BIGINT           NOT NULL,
    [UpgradeId]     UNIQUEIDENTIFIER NOT NULL
);

GO
CREATE NONCLUSTERED INDEX [IX_Hist_Entity]
    ON [dbo].[Hist_Entity]([TransactionId] ASC);

GO
CREATE CLUSTERED INDEX [IDX_Hist_Entity]
    ON [dbo].[Hist_Entity]([_id] ASC);