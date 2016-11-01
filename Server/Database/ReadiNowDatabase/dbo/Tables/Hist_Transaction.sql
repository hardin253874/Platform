-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE TABLE [dbo].[Hist_Transaction] (
    [TransactionId] BIGINT        IDENTITY (1, 1) NOT NULL,
    [InternalId]    BIGINT        NOT NULL,
    [UserId]        BIGINT        NOT NULL,
    [TenantId]      BIGINT        NOT NULL,
    [Spid]          SMALLINT      NOT NULL,
    [Timestamp]     DATETIME      NOT NULL,
    [HostName]      VARCHAR (128) NULL,
    [ProgramName]   VARCHAR (128) NULL,
    [Domain]        VARCHAR (128) NULL,
    [Username]      VARCHAR (128) NULL,
    [LoginName]     VARCHAR (128) NULL,
    [Context]       VARCHAR (128) NULL,
    [UserDefined]   BIT           CONSTRAINT [DF_Hist_Transaction_UserDefined] DEFAULT ((0)) NOT NULL,
	[SystemUpgrade] BIT           CONSTRAINT [DF_Hist_Transaction_SystemUpgrade] DEFAULT ((0)) NOT NULL,
	[RevertTo]      BIGINT        CONSTRAINT [DF_Hist_Transaction_RevertTo] DEFAULT ((NULL)) NULL,
    CONSTRAINT [PK_Hist_Transaction] PRIMARY KEY CLUSTERED ([TransactionId] ASC)
);

GO

CREATE NONCLUSTERED INDEX [Idx_Hist_TransactionId]
	ON [dbo].[Hist_Transaction]([InternalId] ASC)
	INCLUDE([Timestamp]);

GO