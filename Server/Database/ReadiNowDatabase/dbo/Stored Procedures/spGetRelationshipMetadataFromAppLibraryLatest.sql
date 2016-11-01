-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE PROCEDURE [dbo].[spGetRelationshipMetadataFromAppLibraryLatest]
	@relTypeIds dbo.UniqueGuidListType READONLY
AS
BEGIN
	set nocount on;

	declare @solutionVersionString uniqueidentifier = '3b9d1ad1-f3ef-45ae-8ebb-78a5fb2f2ad9'
	declare @alias uniqueidentifier = '33f30df6-4597-4f9f-950e-621c30f8f2f8'
	declare @reverseAlias uniqueidentifier = 'b834e145-4eb3-44a9-aec7-170f2fa354d2'
	declare @cardinality uniqueidentifier = '23b14828-dcd7-432e-b8c1-d23ce6df16c0'
	declare @cloneAction uniqueidentifier = '830f4a8d-6e51-44ad-a543-7dd26a5333cd'
	declare @reverseCloneAction uniqueidentifier = 'd2cfc438-e7d6-4cd7-8eeb-eb753d094254'

	--select * from AppRelationship where FromUid = @drinks and TypeUid=@cardinality

	select
		rel.AppVerUid,	-- provided by package, primarily for diagnostics
		rel.EntityUid RelTypeId,
		alias.[Namespace] + ':' + alias.Data alias,
		reverseAlias.[Namespace] + ':' + reverseAlias.Data reverseAlias,
		cardinality.ToUid cardinality,
		cloneAction.ToUid cloneAction,
		reverseCloneAction.ToUid reverseCloneAction
	from
	(
		select
			p.AppVerUid,
			r.EntityUid,
			-- Rank packages by version number, for each relationship (EntityUid)
			row_number() over (partition by r.EntityUid order by p.Hierarchy desc) newestFirst
		from
		(
			-- Locate packages via their solution entity version strings
			select
				v.AppVerUid,
				cast( '/' + replace( dbo.fnSanitiseVersion( v.Data ), '.', '/' ) + '/' as hierarchyid ) Hierarchy
			from
				AppData_NVarChar v
			where
				FieldUid = @solutionVersionString
		) p
		-- Locate relationship types being examined
		join AppEntity r on
			r.AppVerUid = p.AppVerUid
		join @relTypeIds relTypes on
			relTypes.Id = r.EntityUid
	) rel
	-- Collect data
	left join AppData_Alias alias
		on alias.AppVerUid = rel.AppVerUid and alias.EntityUid = rel.EntityUid and alias.FieldUid = @alias

	left join AppData_Alias reverseAlias
		on reverseAlias.AppVerUid = rel.AppVerUid and reverseAlias.EntityUid = rel.EntityUid and reverseAlias.FieldUid = @reverseAlias

	left join AppRelationship cardinality
		on cardinality.AppVerUid = rel.AppVerUid and cardinality.FromUid = rel.EntityUid and cardinality.TypeUid = @cardinality

	left join AppRelationship cloneAction
		on cloneAction.AppVerUid = rel.AppVerUid and cloneAction.FromUid = rel.EntityUid and cloneAction.TypeUid = @cloneAction

	left join AppRelationship reverseCloneAction
		on reverseCloneAction.AppVerUid = rel.AppVerUid and reverseCloneAction.FromUid = rel.EntityUid and reverseCloneAction.TypeUid = @reverseCloneAction
	-- Collect from newest package
	where rel.newestFirst = 1

END


