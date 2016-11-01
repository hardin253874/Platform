-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE TABLE [dbo].[Lic_Table] (
    [IndexId]     BIGINT NOT NULL,
    [TableId]     BIGINT NOT NULL,
    [RowCount]    INT    CONSTRAINT [DFLT_Lic_Table_RowCount] DEFAULT ((0)) NOT NULL,
    [MinRowBytes] INT    CONSTRAINT [DFLT_Lic_Table_MinRowBytes] DEFAULT ((0)) NOT NULL,
    [MaxRowBytes] INT    CONSTRAINT [DFLT_Lic_Table_MaxRowBytes] DEFAULT ((0)) NOT NULL,
    [AvgRowBytes] INT    CONSTRAINT [DFLT_Lic_Table_AvgRowBytes] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_Lic_Table] PRIMARY KEY CLUSTERED ([IndexId] ASC, [TableId] ASC),
    CONSTRAINT [FK_Lic_Table_Lic_Index] FOREIGN KEY ([IndexId]) REFERENCES [dbo].[Lic_Index] ([Id]),
    CONSTRAINT [FK_Lic_Table_Lic_Table_Name] FOREIGN KEY ([TableId]) REFERENCES [dbo].[Lic_Table_Name] ([Id])
);


