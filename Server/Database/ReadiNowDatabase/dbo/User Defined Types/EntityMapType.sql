-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE TYPE [dbo].[EntityMapType] AS TABLE (
    [SourceId]      BIGINT NOT NULL,
    [DestinationId] BIGINT NOT NULL,
    PRIMARY KEY CLUSTERED ([SourceId] ASC));

