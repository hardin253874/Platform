-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE TABLE [dbo].[PlatformInfo] (
    [ID]             INT      IDENTITY (1, 1) NOT NULL,
    [InstallDate]    DATETIME NULL,
    [BookmarkScheme] INT      NULL,
    CONSTRAINT [PK_PlatformInfo] PRIMARY KEY CLUSTERED ([ID] ASC)
);

