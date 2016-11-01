-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE TABLE [dbo].[RNDB] (
    [Id]      UNIQUEIDENTIFIER CONSTRAINT [DFLT_RNDB_Id] DEFAULT (newid()) NOT NULL,
    [Created] DATETIME         CONSTRAINT [DFLT_RNDB_Created] DEFAULT (getutcdate()) NOT NULL
);


