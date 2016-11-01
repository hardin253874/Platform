-- Copyright 2011-2016 Global Software Innovation Pty Ltd
CREATE TYPE [dbo].[AliasLookupType] AS TABLE (
    [TenantId]  BIGINT         NOT NULL,
    [Namespace] NVARCHAR (100) NOT NULL,
    [Data]      NVARCHAR (100) NOT NULL,
    PRIMARY KEY CLUSTERED ([TenantId] ASC, [Namespace] ASC, [Data] ASC));

