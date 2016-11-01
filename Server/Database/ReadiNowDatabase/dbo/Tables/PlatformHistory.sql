-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE TABLE [dbo].[PlatformHistory] (
    [Timestamp] DATETIME CONSTRAINT [DFLT_PlatformHistory_Timestamp] DEFAULT (getutcdate()) NOT NULL,
	[TenantName] NVARCHAR (200) NULL,
	[TenantId] BIGINT NOT NULL,
	[PackageId] UNIQUEIDENTIFIER NULL,
	[PackageName] NVARCHAR (200) NULL,
	[Operation] NVARCHAR (100) NOT NULL,
	[Machine] NVARCHAR (200) NULL,
	[User] NVARCHAR (200) NULL,
	[Process] NVARCHAR (200) NULL,
	[Arguments] NVARCHAR (max) NULL,
	[Exception] NVARCHAR (max) NULL
);

GO
CREATE CLUSTERED INDEX [IX_PlatformHistory]
    ON [dbo].[PlatformHistory]([TenantId] DESC, [Timestamp] DESC);