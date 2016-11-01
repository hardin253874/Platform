-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE TABLE [dbo].[Data_Decimal] (
    [EntityId] BIGINT           NOT NULL,
    [TenantId] BIGINT           NOT NULL,
    [FieldId]  BIGINT           NOT NULL,
    [Data]     DECIMAL (38, 10) NULL,
    CONSTRAINT [PK_Data_Decimal] PRIMARY KEY CLUSTERED ([TenantId] ASC, [EntityId] ASC, [FieldId] ASC),
    CONSTRAINT [FK_Data_Decimal_EntityId_in_Entity] FOREIGN KEY ([TenantId], [EntityId]) REFERENCES [dbo].[Entity] ([TenantId], [Id]),
    CONSTRAINT [FK_Data_Decimal_FieldId_In_Entity] FOREIGN KEY ([TenantId], [FieldId]) REFERENCES [dbo].[Entity] ([TenantId], [Id])
);





GO

CREATE NONCLUSTERED INDEX [IX_Data_Decimal_SearchField]
    ON [dbo].[Data_Decimal]([TenantId] ASC, [FieldId] ASC, [Data] ASC)
    INCLUDE([EntityId]);

GO

CREATE TRIGGER [dbo].[trgData_Decimal]
   ON  [dbo].[Data_Decimal]
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
			[Hist_Data_Decimal]
		SELECT
			@tranId,
			0,
			[EntityId],
			[TenantId],
			[FieldId],
			[Data]
		FROM
			[deleted]
    
		INSERT INTO
			[Hist_Data_Decimal]
		SELECT
			@tranId,
			1,
			[EntityId],
			[TenantId],
			[FieldId],
			[Data]
		FROM
			[inserted]
	END
END