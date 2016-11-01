-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE TABLE [dbo].[Logs] (
    [Date]    DATETIME       NOT NULL,
    [Message] NVARCHAR (MAX) NULL,
	CONSTRAINT [PK_Logs] PRIMARY KEY CLUSTERED ([Date])
);

