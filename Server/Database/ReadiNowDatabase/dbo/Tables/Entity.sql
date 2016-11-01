-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE TABLE [dbo].[Entity] (
    [Id]        BIGINT           IDENTITY (1, 1) NOT NULL,
    [TenantId]  BIGINT           NOT NULL,
    [UpgradeId] UNIQUEIDENTIFIER CONSTRAINT [DF_Entity_UpgradeId] DEFAULT (newid()) NOT NULL,
    CONSTRAINT [PK_Entity] PRIMARY KEY CLUSTERED ([TenantId] ASC, [Id] ASC),
    CONSTRAINT [IX_Entity] UNIQUE NONCLUSTERED ([TenantId] ASC, [UpgradeId] ASC)
);














GO
CREATE NONCLUSTERED INDEX [IDX_Entity_Id]
    ON [dbo].[Entity]([Id] ASC);


GO

CREATE TRIGGER [dbo].[trgEntity]
   ON  [dbo].[Entity]
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
			[Hist_Entity]
		SELECT
			@tranId,
			0,
			[Id],
			[TenantId],
			[UpgradeId]
		FROM
			[deleted]
    
		INSERT INTO
			[Hist_Entity]
		SELECT
			@tranId,
			1,
			[Id],
			[TenantId],
			[UpgradeId]
		FROM
			[inserted]
	END
END