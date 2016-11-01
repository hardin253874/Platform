-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE TABLE [dbo].[Lic_User]
(
	[IndexId] [bigint] NOT NULL,
	[TenantId] [bigint] NOT NULL, 
	[UserId] [bigint] NOT NULL,
	[Name] [nvarchar](200) NOT NULL,
	[Status] [nvarchar](200) NOT NULL,
	CONSTRAINT [PK_Lic_User] PRIMARY KEY CLUSTERED ([IndexId], [TenantId], [UserId]), 
    CONSTRAINT [FK_Lic_User_Lic_Index] FOREIGN KEY ([IndexId]) REFERENCES [Lic_Index]([Id])
)

GO

CREATE INDEX [IX_Lic_User_Name] ON [dbo].[Lic_User] ([Name])

GO

CREATE INDEX [IX_Lic_User_Status] ON [dbo].[Lic_User] ([Status])
