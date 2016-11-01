-- Copyright 2011-2016 Global Software Innovation Pty Ltd
CREATE TYPE [dbo].[RelationshipType] AS TABLE (
    [TenantId] BIGINT NOT NULL,
    [TypeId]   BIGINT NOT NULL,
    [FromId]   BIGINT NOT NULL,
    [ToId]     BIGINT NOT NULL,
    PRIMARY KEY CLUSTERED ([TenantId] asc, [TypeId] ASC, [FromId] ASC, [ToId] ASC));

