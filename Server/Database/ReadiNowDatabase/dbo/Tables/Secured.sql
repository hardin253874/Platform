-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE TABLE [dbo].[Secured] (
    [TenantId] BIGINT           NOT NULL,
    [Context]  NVARCHAR (100)   NOT NULL,
    [SecureId] UNIQUEIDENTIFIER NOT NULL,
    [Data]     VARBINARY (MAX)  NULL,
    CONSTRAINT [PK_Secured] PRIMARY KEY CLUSTERED ([TenantId] ASC, [SecureId] ASC)
);


GO
CREATE NONCLUSTERED INDEX [IX_Secured]
    ON [dbo].[Secured]([TenantId] ASC, [Context] ASC);

