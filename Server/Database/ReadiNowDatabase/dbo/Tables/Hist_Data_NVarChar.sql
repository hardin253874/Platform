-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE TABLE [dbo].[Hist_Data_NVarChar] (
    [_id]           BIGINT         IDENTITY (1, 1) NOT NULL,
    [TransactionId] BIGINT         NOT NULL,
    [Action]        TINYINT        NOT NULL,
	[Id]            BIGINT         NOT NULL,
    [EntityId]      BIGINT         NOT NULL,
    [TenantId]      BIGINT         NOT NULL,
    [FieldId]       BIGINT         NOT NULL,
    [Data]          NVARCHAR (MAX) COLLATE Latin1_General_CI_AI NULL
);

GO
CREATE NONCLUSTERED INDEX [IX_Hist_Data_NVarChar]
    ON [dbo].[Hist_Data_NVarChar]([TransactionId] ASC);

GO
CREATE CLUSTERED INDEX [IDX_Hist_Data_NVarChar]
    ON [dbo].[Hist_Data_NVarChar]([_id] ASC);