-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE TABLE [dbo].[AppDeploy_Relationship] (
    [Id]        BIGINT           IDENTITY (1, 1) NOT NULL,
    [AppVerUid] UNIQUEIDENTIFIER NOT NULL,
    [TenantId]  BIGINT           NOT NULL,
    [TypeUid]   UNIQUEIDENTIFIER NOT NULL,
    [FromUid]   UNIQUEIDENTIFIER NOT NULL,
    [ToUid]     UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT [PK_AppDeploy_Relationship] PRIMARY KEY NONCLUSTERED ([AppVerUid] ASC, [TenantId] ASC, [TypeUid] ASC, [FromUid] ASC, [ToUid] ASC)
);




GO
CREATE CLUSTERED INDEX [IDX_AppDeployRelationship]
    ON [dbo].[AppDeploy_Relationship]([Id] ASC);


GO
CREATE NONCLUSTERED INDEX [IDX_AppDeployRelationship_AppVerUid]
    ON [dbo].[AppDeploy_Relationship]([AppVerUid] ASC, [TypeUid] ASC, [FromUid] ASC, [ToUid] ASC);
GO
CREATE NONCLUSTERED INDEX [IDX_AppRelationship_TenantId]
    ON [dbo].[AppDeploy_Relationship]([TenantId] ASC, [AppVerUid] ASC);

