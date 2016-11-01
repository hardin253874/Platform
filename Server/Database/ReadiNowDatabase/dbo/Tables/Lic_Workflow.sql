-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE TABLE [dbo].[Lic_Workflow] (
    [IndexId]       BIGINT         NOT NULL,
    [TenantId]      BIGINT         NOT NULL,
    [WorkflowId]    BIGINT         NOT NULL,
    [Name]          NVARCHAR (200) NOT NULL,
    [RunCount]      BIGINT         CONSTRAINT [DFLT_Lic_Workflow_RunCount] DEFAULT ((0)) NOT NULL,
    [ApplicationId] BIGINT         CONSTRAINT [DFLT_Lic_Workflow_ApplicationId] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_Lic_Workflow] PRIMARY KEY CLUSTERED ([IndexId] ASC, [TenantId] ASC, [WorkflowId] ASC),
    CONSTRAINT [FK_Lic_Workflow_Lic_Index] FOREIGN KEY ([IndexId]) REFERENCES [dbo].[Lic_Index] ([Id])
);


