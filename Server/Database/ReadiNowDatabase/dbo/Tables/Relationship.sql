-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE TABLE [dbo].[Relationship] (
    [Id]       BIGINT IDENTITY (1, 1) NOT NULL,
    [TenantId] BIGINT NOT NULL,
    [TypeId]   BIGINT NOT NULL,
    [FromId]   BIGINT NOT NULL,
    [ToId]     BIGINT NOT NULL,
    CONSTRAINT [PK_Relationships] PRIMARY KEY NONCLUSTERED ([TenantId] ASC, [TypeId] ASC, [FromId] ASC, [ToId] ASC),
    CONSTRAINT [FK_RelationshipFrom_Entity] FOREIGN KEY ([TenantId], [FromId]) REFERENCES [dbo].[Entity] ([TenantId], [Id]),
    CONSTRAINT [FK_RelationshipTo_Entity] FOREIGN KEY ([TenantId], [ToId]) REFERENCES [dbo].[Entity] ([TenantId], [Id])
);







GO

CREATE UNIQUE CLUSTERED INDEX [IX_Relationship_Id]
    ON [dbo].[Relationship]([Id] ASC);

GO

CREATE NONCLUSTERED INDEX [FKIX_Relationship_FromId]
    ON [dbo].[Relationship]([TenantId] ASC, [FromId] ASC)
    INCLUDE([TypeId], [ToId]);

GO

CREATE NONCLUSTERED INDEX [FKIX_Relationship_ToId]
    ON [dbo].[Relationship]([TenantId] ASC, [ToId] ASC)
    INCLUDE([TypeId], [FromId]);

GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_Relationship_TypeToFrom]
    ON [dbo].[Relationship]([TenantId] ASC, [TypeId] ASC, [ToId] ASC, [FromId] ASC);

GO

CREATE TRIGGER [dbo].[trgRelationship]
   ON  [dbo].[Relationship]
   AFTER INSERT,DELETE,UPDATE
AS 
BEGIN
	IF @@ROWCOUNT = 0
		RETURN

	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	DECLARE @tenantId BIGINT

	SELECT
		@tenantId = MAX( TenantId )
	FROM (
		SELECT
			TenantId
		FROM
			[inserted]

		UNION ALL

		SELECT
			TenantId
		FROM
			[deleted]
	) a

	IF ( @tenantId IS NOT NULL )
	BEGIN
		DECLARE @tranId BIGINT
		
		EXEC spCreateRestorePoint @tenantId, DEFAULT, DEFAULT, DEFAULT, DEFAULT, @tranId OUTPUT

		INSERT INTO
			[Hist_Relationship]
		SELECT
			@tranId,
			0,
			[Id],
			[TenantId],
			[TypeId],
			[FromId],
			[ToId]
		FROM
			[deleted]
    
		INSERT INTO
			[Hist_Relationship]
		SELECT
			@tranId,
			1,
			[Id],
			[TenantId],
			[TypeId],
			[FromId],
			[ToId]
		FROM
			[inserted]
	END
END