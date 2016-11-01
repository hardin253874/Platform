-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE TYPE [dbo].[BulkFldType] AS TABLE (
    [NodeTag] INT    NOT NULL,
    [FieldId] BIGINT NOT NULL,
    PRIMARY KEY CLUSTERED ([NodeTag] ASC, [FieldId] ASC));

