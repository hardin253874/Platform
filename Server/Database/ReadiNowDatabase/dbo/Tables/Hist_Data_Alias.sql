-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE TABLE [dbo].[Hist_Data_Alias] (
    [_id]           BIGINT         IDENTITY (1, 1) NOT NULL,
    [TransactionId] BIGINT         NOT NULL,
    [Action]        TINYINT        NOT NULL,
    [EntityId]      BIGINT         NOT NULL,
    [TenantId]      BIGINT         NOT NULL,
    [FieldId]       BIGINT         NOT NULL,
    [Data]          NVARCHAR (100) NOT NULL,
    [Namespace]     NVARCHAR (100) NOT NULL,
    [AliasMarkerId] INT            NOT NULL
);

GO
CREATE NONCLUSTERED INDEX [IX_Hist_Data_Alias]
    ON [dbo].[Hist_Data_Alias]([TransactionId] ASC);

GO
CREATE CLUSTERED INDEX [IDX_Hist_Data_Alias]
    ON [dbo].[Hist_Data_Alias]([_id] ASC);

