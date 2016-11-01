-- Copyright 2011-2016 Global Software Innovation Pty Ltd
CREATE TYPE [dbo].[ReverseRelationshipType] AS TABLE (
    [TenantId] BIGINT NOT NULL,
    [TypeId]   BIGINT NOT NULL,
    [ToId]     BIGINT NOT NULL,
    PRIMARY KEY CLUSTERED ([TenantId] ASC, [TypeId] ASC, [ToId] ASC));

