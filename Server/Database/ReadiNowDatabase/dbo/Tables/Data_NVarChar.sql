-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE TABLE [dbo].[Data_NVarChar] (
    [Id]              BIGINT         IDENTITY (1, 1) NOT NULL,
    [EntityId]        BIGINT         NOT NULL,
    [TenantId]        BIGINT         NOT NULL,
    [FieldId]         BIGINT         NOT NULL,
    [Data]            NVARCHAR (MAX) COLLATE Latin1_General_CI_AI NULL,
    [Data_StartsWith] AS             (CONVERT([nvarchar](100),left([Data],(100)))) PERSISTED,
    CONSTRAINT [PK_Data_NVarChar] PRIMARY KEY CLUSTERED ([TenantId] ASC, [EntityId] ASC, [FieldId] ASC),
    CONSTRAINT [FK_Data_NVarChar_EntityId_in_Entity] FOREIGN KEY ([TenantId], [EntityId]) REFERENCES [dbo].[Entity] ([TenantId], [Id]),
    CONSTRAINT [FK_Data_NVarChar_FieldId_In_Entity] FOREIGN KEY ([TenantId], [FieldId]) REFERENCES [dbo].[Entity] ([TenantId], [Id])
);











GO

CREATE UNIQUE NONCLUSTERED INDEX [IDX_Data_NVarChar]
    ON [dbo].[Data_NVarChar]([Id] ASC);

GO

CREATE NONCLUSTERED INDEX [IDX_Data_NVarChar_SearchField]
    ON [dbo].[Data_NVarChar]([TenantId] ASC, [FieldId] ASC, [Data_StartsWith] ASC)
    INCLUDE([EntityId], [Data]);

GO

CREATE TRIGGER [dbo].[trgData_NVarChar]
   ON  [dbo].[Data_NVarChar]
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
			[Hist_Data_NVarChar]
		SELECT
			@tranId,
			0,
			[Id],
			[EntityId],
			[TenantId],
			[FieldId],
			[Data]
		FROM
			[deleted]
    
		INSERT INTO
			[Hist_Data_NVarChar]
		SELECT
			@tranId,
			1,
			[Id],
			[EntityId],
			[TenantId],
			[FieldId],
			[Data]
		FROM
			[inserted]
	END
END