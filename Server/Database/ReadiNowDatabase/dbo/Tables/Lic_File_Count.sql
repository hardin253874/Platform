-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE TABLE [dbo].[Lic_File_Count] (
    [IndexId]       BIGINT         NOT NULL,
    [TenantId]      BIGINT         NOT NULL,
    [Count]         BIGINT         CONSTRAINT [DFLT_Lic_File_Count_Count] DEFAULT ((0)) NOT NULL,
    [Size]          BIGINT         CONSTRAINT [DFLT_Lic_File_Count_Size] DEFAULT ((0)) NOT NULL,
    [Attribute]     NVARCHAR (200) NOT NULL,
    [ApplicationId] BIGINT         CONSTRAINT [DFLT_Lic_File_Count_ApplicationId] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [FK_Lic_File_Count_Lic_Index] FOREIGN KEY ([IndexId]) REFERENCES [dbo].[Lic_Index] ([Id])
);


GO

CREATE CLUSTERED INDEX [IX_Lic_File_Count_TenantIdAttribute] ON [dbo].[Lic_File_Count] ([TenantId], [Attribute])
