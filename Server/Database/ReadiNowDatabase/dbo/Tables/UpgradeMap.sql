-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE TABLE [dbo].[UpgradeMap] (
    [Namespace] NVARCHAR (100)   NOT NULL,
    [Alias]     NVARCHAR (100)   NOT NULL,
    [UpgradeId] UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT [PK_UpgradeMap] PRIMARY KEY CLUSTERED ([Namespace] ASC, [Alias] ASC)
);


GO
CREATE NONCLUSTERED INDEX [IX_UpgradeMap]
    ON [dbo].[UpgradeMap]([UpgradeId] ASC);

