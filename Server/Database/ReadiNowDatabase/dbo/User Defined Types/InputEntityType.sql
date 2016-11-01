-- Copyright 2011-2016 Global Software Innovation Pty Ltd
CREATE TYPE [dbo].[InputEntityType] AS TABLE (
	[Id] BIGINT NOT NULL,
    [TypeId] BIGINT NOT NULL,
	[IsClone] INT NOT NULL,
	PRIMARY KEY CLUSTERED ([Id] ASC, [TypeId] ASC));
