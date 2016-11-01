-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE TABLE [dbo].[Lic_Record] (
    [IndexId]       BIGINT NOT NULL,
    [TenantId]      BIGINT NOT NULL,
    [TableId]       BIGINT NOT NULL,	
    [RowCount]      INT    CONSTRAINT [DFLT_Lic_Record_RowCount] DEFAULT ((0)) NOT NULL,    
	[ApplicationId] BIGINT CONSTRAINT [DFLT_Lic_Record_ApplicationId] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_Lic_Record] PRIMARY KEY CLUSTERED ([IndexId] ASC, [TenantId] ASC, [TableId] ASC, [ApplicationId] ASC),
    CONSTRAINT [FK_Lic_Record_Lic_Index] FOREIGN KEY ([IndexId]) REFERENCES [dbo].[Lic_Index] ([Id]),
    CONSTRAINT [FK_Lic_Record_Lic_Table_Name] FOREIGN KEY ([TableId]) REFERENCES [dbo].[Lic_Table_Name] ([Id])
);


