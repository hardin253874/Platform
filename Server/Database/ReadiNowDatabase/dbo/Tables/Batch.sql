-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE TABLE [dbo].[Batch] (
    [BatchId]     BIGINT           IDENTITY (1, 1) NOT NULL,
    [BatchGuid]   UNIQUEIDENTIFIER CONSTRAINT [DF_Batch_BatchGuid] DEFAULT (newid()) NOT NULL,
    [CreatedDate] DATETIME         CONSTRAINT [DF_Batch_CreatedDate] DEFAULT (getdate()) NOT NULL,
    CONSTRAINT [PK_Batch] PRIMARY KEY CLUSTERED ([BatchId] ASC),
    CONSTRAINT [IX_BatchGuid] UNIQUE NONCLUSTERED ([BatchGuid] ASC)
);

