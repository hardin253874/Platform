-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE TABLE [dbo].[Lic_Tenant]
(
	[IndexId] [bigint] NOT NULL,
	[TenantId] [bigint] NOT NULL,
	[Name] [nvarchar](200) NOT NULL,
	[Disabled] BIT NULL, 
    CONSTRAINT [PK_Lic_Tenant] PRIMARY KEY ([IndexId], [TenantId])	, 
    CONSTRAINT [FK_Lic_Tenant_Lic_Index] FOREIGN KEY ([IndexId]) REFERENCES [Lic_Index]([Id])
)
