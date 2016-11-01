-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE TABLE [dbo].[Data_Alias] (
    [EntityId]      BIGINT         NOT NULL,
    [TenantId]      BIGINT         NOT NULL,
    [FieldId]       BIGINT         NOT NULL,
    [Data]          NVARCHAR (100) NOT NULL,
    [Namespace]     NVARCHAR (100) NOT NULL,
    [AliasMarkerId] INT            NOT NULL,
    CONSTRAINT [PK_Data_Alias] PRIMARY KEY CLUSTERED ([TenantId] ASC, [EntityId] ASC, [FieldId] ASC),
    CONSTRAINT [FK_Data_Alias_EntityId_in_Entity] FOREIGN KEY ([TenantId], [EntityId]) REFERENCES [dbo].[Entity] ([TenantId], [Id]),
    CONSTRAINT [FK_Data_Alias_FieldId_In_Entity] FOREIGN KEY ([TenantId], [FieldId]) REFERENCES [dbo].[Entity] ([TenantId], [Id])
);







GO

CREATE NONCLUSTERED INDEX [FKIX_Data_Alias_FieldId]
    ON [dbo].[Data_Alias]([TenantId] ASC, [FieldId] ASC);
	
GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_Data_Alias_SearchAlias]
    ON [dbo].[Data_Alias]([TenantId] ASC, [Data] ASC, [Namespace] ASC, [AliasMarkerId] ASC, [EntityId] ASC )
    INCLUDE([FieldId]);

GO

CREATE NONCLUSTERED INDEX [IX_Data_Alias_SearchEntityAlias]
    ON [dbo].[Data_Alias]([EntityId] ASC, [Data] ASC, [Namespace] ASC, [AliasMarkerId] ASC);

GO

CREATE NONCLUSTERED INDEX [IX_Data_Alias_Search]
    ON [dbo].[Data_Alias]([Data] ASC, [Namespace] ASC, [AliasMarkerId] ASC);

GO

CREATE TRIGGER [dbo].[trgData_Alias]
   ON  [dbo].[Data_Alias]
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
			[Hist_Data_Alias]
		SELECT
			@tranId,
			0,
			[EntityId],
			[TenantId],
			[FieldId],
			[Data],
			[Namespace],
			[AliasMarkerId]
		FROM
			[deleted]
    
		INSERT INTO
			[Hist_Data_Alias]
		SELECT
			@tranId,
			1,
			[EntityId],
			[TenantId],
			[FieldId],
			[Data],
			[Namespace],
			[AliasMarkerId]
		FROM
			[inserted]
	END
END