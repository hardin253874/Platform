-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE TABLE [dbo].[AppRelationship] (
    [Id]        BIGINT           IDENTITY (1, 1) NOT NULL,
    [AppVerUid] UNIQUEIDENTIFIER NOT NULL,
    [TypeUid]   UNIQUEIDENTIFIER NOT NULL,
    [FromUid]   UNIQUEIDENTIFIER NOT NULL,
    [ToUid]     UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT [PK_AppRelationship] PRIMARY KEY NONCLUSTERED ([AppVerUid] ASC, [TypeUid] ASC, [FromUid] ASC, [ToUid] ASC)
);






GO
CREATE CLUSTERED INDEX [IDX_AppRelationship]
    ON [dbo].[AppRelationship]([Id] ASC);


GO
CREATE NONCLUSTERED INDEX [IDX_AppRelationship_AppTypeTo]
	ON [dbo].[AppRelationship] ([AppVerUid],[TypeUid],[ToUid])
	INCLUDE ([FromUid]);


GO

