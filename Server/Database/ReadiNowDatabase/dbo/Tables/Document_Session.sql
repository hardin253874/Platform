-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE TABLE [dbo].[Document_Session] (
    [ID]          BIGINT         IDENTITY (1, 1) NOT NULL,
    [SessionId]   NVARCHAR (255) NOT NULL,
    [DataHash]    NVARCHAR (256) NOT NULL,
    [SessionTime] DATETIME2 (7)  NOT NULL,
    CONSTRAINT [PK_Document_Session] PRIMARY KEY CLUSTERED ([ID] ASC),
    CONSTRAINT [IDX_Document_Session_SessionIdSessionTime] UNIQUE NONCLUSTERED ([SessionId] ASC, [SessionTime] ASC)
);

