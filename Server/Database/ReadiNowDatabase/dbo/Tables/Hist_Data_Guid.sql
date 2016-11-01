-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE TABLE [dbo].[Hist_Data_Guid] (
    [_id]           BIGINT           IDENTITY (1, 1) NOT NULL,
    [TransactionId] BIGINT           NOT NULL,
    [Action]        TINYINT          NOT NULL,
    [EntityId]      BIGINT           NOT NULL,
    [TenantId]      BIGINT           NOT NULL,
    [FieldId]       BIGINT           NOT NULL,
    [Data]          UNIQUEIDENTIFIER NULL
);

GO
CREATE NONCLUSTERED INDEX [IX_Hist_Data_Guid]
    ON [dbo].[Hist_Data_Guid]([TransactionId] ASC);

GO
CREATE CLUSTERED INDEX [IDX_Hist_Data_Guid]
    ON [dbo].[Hist_Data_Guid]([_id] ASC);