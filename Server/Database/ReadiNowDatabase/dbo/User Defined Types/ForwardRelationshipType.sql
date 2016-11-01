-- Copyright 2011-2016 Global Software Innovation Pty Ltd
CREATE TYPE [dbo].[ForwardRelationshipType] AS TABLE (
    [TenantId] BIGINT NOT NULL,
    [TypeId]   BIGINT NOT NULL,
    [FromId]   BIGINT NOT NULL,
    PRIMARY KEY CLUSTERED ([TenantId] ASC, [TypeId] ASC, [FromId] ASC));

