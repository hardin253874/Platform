-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE TABLE [dbo].[Lic_Object_Count] (
    [IndexId]       BIGINT         NOT NULL,
    [TenantId]      BIGINT         NOT NULL,
    [Count]         BIGINT         CONSTRAINT [DFLT_Lic_Object_Count_Count] DEFAULT ((0)) NOT NULL,
    [Attribute]     NVARCHAR (200) NULL,
    [ApplicationId] BIGINT         CONSTRAINT [DFLT_Lic_Object_Count_ApplicationId] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [FK_Lic_Object_Count_Lic_Index] FOREIGN KEY ([IndexId]) REFERENCES [dbo].[Lic_Index] ([Id])
);


GO

CREATE CLUSTERED INDEX [IX_Lic_Object_Count_TenantIdAttribute] ON [dbo].[Lic_Object_Count] ([TenantId], [Attribute])
