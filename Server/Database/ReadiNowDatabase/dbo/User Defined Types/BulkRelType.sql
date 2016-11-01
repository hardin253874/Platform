-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE TYPE [dbo].[BulkRelType] AS TABLE (
    [NodeTag]   INT    NOT NULL,
    [RelTypeId] BIGINT NOT NULL,
    [NextTag]   INT    NOT NULL,
    PRIMARY KEY CLUSTERED ([NodeTag] ASC, [RelTypeId] ASC, [NextTag] ASC));

