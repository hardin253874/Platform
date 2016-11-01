-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE TYPE [dbo].[UniqueGuidListType] AS TABLE (
    [Id] [uniqueidentifier] NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC));
