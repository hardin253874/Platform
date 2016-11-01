-- Copyright 2011-2016 Global Software Innovation Pty Ltd
CREATE TYPE [dbo].[Data_NVarCharType] AS TABLE (
    [EntityId] BIGINT         NOT NULL,
    [TenantId] BIGINT         NOT NULL,
    [FieldId]  BIGINT         NOT NULL,
    [Data]     NVARCHAR (MAX) COLLATE Latin1_General_CI_AI NOT NULL,
    PRIMARY KEY CLUSTERED ([TenantId] ASC, [EntityId] ASC, [FieldId] ASC));

