-- Copyright 2011-2016 Global Software Innovation Pty Ltd
CREATE TYPE [dbo].[Data_AliasType] AS TABLE (
    [EntityId]      BIGINT         NOT NULL,
    [TenantId]      BIGINT         NOT NULL,
    [FieldId]       BIGINT         NOT NULL,
    [Data]          NVARCHAR (100) NOT NULL,
    [Namespace]     NVARCHAR (100) NOT NULL,
    [AliasMarkerId] INT            NOT NULL,
    PRIMARY KEY CLUSTERED ([TenantId] ASC, [EntityId] ASC, [FieldId] ASC));

