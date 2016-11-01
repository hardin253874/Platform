-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE TABLE [dbo].[Lic_Index] (
    [Id]        BIGINT   IDENTITY (1, 1) NOT NULL,
    [Timestamp] DATETIME CONSTRAINT [DFLT_Lic_Index_Timestamp] DEFAULT (getutcdate()) NOT NULL,
    CONSTRAINT [PK_Lic_Index] PRIMARY KEY CLUSTERED ([Id] DESC)
);


