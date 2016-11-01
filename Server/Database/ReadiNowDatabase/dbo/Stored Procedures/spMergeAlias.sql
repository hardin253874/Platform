-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE PROCEDURE [dbo].[spMergeAlias]
	@data dbo.Data_AliasType READONLY
AS
BEGIN

	SET NOCOUNT ON;

	MERGE
		dbo.Data_Alias dest
	USING
		@data AS src
	ON
		dest.EntityId = src.EntityId AND
		dest.TenantId = src.TenantId AND
		dest.FieldId = src.FieldId
	WHEN NOT MATCHED THEN
		INSERT (
			EntityId,
			TenantId,
			FieldId,
			Data,
			[Namespace],
			AliasMarkerId )
		VALUES (
			src.EntityId,
			src.TenantId,
			src.FieldId,
			src.Data,
			src.[Namespace],
			src.AliasMarkerId )
	WHEN MATCHED AND EXISTS (
		SELECT
			dest.Data, dest.Namespace, dest.AliasMarkerId
		EXCEPT
		SELECT
			src.Data, src.Namespace, src.AliasMarkerId ) THEN
		UPDATE SET
			dest.Data = src.Data,
			dest.[Namespace] = src.[Namespace],
			dest.AliasMarkerId = src.AliasMarkerId;
END