-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE TABLE [dbo].[AppDeploy_Field] (
    [Id]        BIGINT           IDENTITY (1, 1) NOT NULL,
    [AppVerUid] UNIQUEIDENTIFIER NOT NULL,
    [TenantId]  BIGINT           NOT NULL,
    [EntityUid] UNIQUEIDENTIFIER NOT NULL,
    [FieldUid]  UNIQUEIDENTIFIER NOT NULL,
    [Data]      NVARCHAR (MAX)   NULL,
    [Type]      NVARCHAR (50)    NULL,
    CONSTRAINT [PK_AppDeploy_Field] PRIMARY KEY NONCLUSTERED ([AppVerUid] ASC, [TenantId] ASC, [EntityUid] ASC, [FieldUid] ASC)
);




GO
CREATE UNIQUE CLUSTERED INDEX [IX_AppDeploy_Field_Id]
    ON [dbo].[AppDeploy_Field]([Id] ASC);	