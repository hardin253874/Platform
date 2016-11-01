-- Copyright 2011-2016 Global Software Innovation Pty Ltd
CREATE TYPE [dbo].[Data_DateTimeType] AS TABLE (
    [EntityId] BIGINT   NOT NULL,
    [TenantId] BIGINT   NOT NULL,
    [FieldId]  BIGINT   NOT NULL,
    [Data]     DATETIME NOT NULL,
    PRIMARY KEY CLUSTERED ([TenantId] ASC, [EntityId] ASC, [FieldId] ASC));
