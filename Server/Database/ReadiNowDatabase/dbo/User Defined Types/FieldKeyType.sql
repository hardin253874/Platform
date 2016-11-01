-- Copyright 2011-2016 Global Software Innovation Pty Ltd
CREATE TYPE [dbo].[FieldKeyType] AS TABLE (
    [EntityId] BIGINT NOT NULL,
    [TenantId] BIGINT NOT NULL,
    [FieldId]  BIGINT NOT NULL,
    PRIMARY KEY CLUSTERED ([EntityId] ASC, [FieldId] ASC));

