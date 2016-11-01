-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE TABLE [dbo].[Lic_Application]
(	
	[IndexId] [bigint] NOT NULL,
	[AppId] [bigint] NOT NULL,
	[TenantId] [bigint] NOT NULL, 
	[Name] [nvarchar](200) NOT NULL,
	[Version] [nvarchar](200) NULL,
	[Publisher] [nvarchar](200) NULL,
	[Released] [datetime] NULL,
	[PackageId] [uniqueidentifier] NOT NULL,
    CONSTRAINT [PK_Lic_Application] PRIMARY KEY CLUSTERED ([IndexId], [AppId], [TenantId]), 
    CONSTRAINT [FK_Lic_Application_Lic_Index] FOREIGN KEY ([IndexId]) REFERENCES [Lic_Index]([Id])
)
