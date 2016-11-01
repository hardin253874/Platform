-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE PROCEDURE [dbo].[spRepairApplicationReferences]
	@tenant BIGINT = 0,
	@context VARCHAR( 128 ) = NULL
AS
BEGIN
	SET NOCOUNT ON

	declare @tenantId bigint
	declare @packageId bigint
	declare @inSolution bigint
	declare @indirectInSolution bigint
	declare @inSolutionUid uniqueidentifier = '7c77c3a0-75b5-4c59-99f6-3ba9229e6a55'
	declare @indirectInSolutionUid uniqueidentifier = '54e16d01-c53f-407f-add8-4c906b6ca5dc'

	IF ( @context IS NULL )
	BEGIN
		SET @context = OBJECT_NAME(@@PROCID)
	END

	IF ( @context IS NOT NULL )
	BEGIN
		DECLARE @contextInfo VARBINARY(128) = CONVERT( VARBINARY(128), @context )
		SET CONTEXT_INFO @contextInfo
	END

	declare cur cursor forward_only for
	select Id from (
	    select 0 Id union select Id from _vTenant
	) t where t.Id = @tenant or @tenant = 0

	open cur
	fetch next from	cur into @tenantId

	while @@fetch_status = 0
	begin

		-- Working tables
		create table #corePackages (Id bigint, PackageUid uniqueidentifier, Priority int, Name nvarchar(50), primary key (Id))
		create table #repair (Id bigint, UpgradeId uniqueidentifier, CorrectPackageId bigint, CorrectPackageUid uniqueidentifier, primary key (Id, CorrectPackageId))
		create table #coreContents (EntityUid uniqueidentifier, PackageUid uniqueidentifier, CorrectPackageId bigint, primary key (EntityUid))
	
		-- Aliases
		select @packageId = dbo.fnAliasNsId( 'packageId', 'core', @tenantId )
		select @inSolution = dbo.fnAliasNsId( 'inSolution', 'core', @tenantId )
		select @indirectInSolution = dbo.fnAliasNsId( 'indirectInSolution', 'core', @tenantId )

		-- Determine the 'core packages' (with their Id in the tenant, and their Package Guid)
		-- And also include their 'priority' order for resolving conflicts
		insert into #corePackages
		select e.Id Id, g.Data PackageUid, v.Priority, v.Name
		from Entity e
			join Data_Guid g on g.TenantId = @tenantId and g.EntityId = e.Id and g.FieldId = @packageId
			join ( values
				('7062aade-2e72-4a71-a7fa-a412d20d6f01', 1, 'ReadiNow Core'),
				('34ff4d95-70c6-4ae8-8f6f-38d88546d4c4', 2, 'ReadiNow Console'),
				('abf12077-6fa5-43da-b608-b8b7514d07bb', 3, 'ReadiNow Core Data'),
				('50380499-2857-474d-92bf-6007303855f1', 4, 'Shared')
			) v(UpgradeId, Priority, Name) on e.UpgradeId = v.UpgradeId
		where e.TenantId = @tenantId

		-- Find the contents of core application, resolving priorities so each entity belongs to the highest priority app
		insert into #coreContents
		select ep.EntityUid, p2.PackageUid, p2.Id CorrectPackageId
		from (
			select EntityUid, min(Priority) MinPriority from AppEntity ae
			join #corePackages p on ae.AppVerUid = p.PackageUid
			group by EntityUid
			having count(*) > 0
		) ep
		join #corePackages p2 on p2.Priority = ep.MinPriority

		-- Locate all entities in the tenant that are not pointing to the right core app
		insert into #repair
		select distinct e.Id, c.EntityUid, c.CorrectPackageId, c.PackageUid
		from #coreContents c
			join Entity e on e.TenantId = @tenantId and e.UpgradeId = c.EntityUid
			where not exists (
				select 1 from Relationship r
					where r.TenantId = @tenantId
					and r.TypeId in (@inSolution, @indirectInSolution)
					and r.FromId = e.Id
					and r.ToId = c.CorrectPackageId
			)

		-- Locate all entities in the tenant that are pointing to the wrong app
		insert into #repair
		select distinct r.FromId Id, e.UpgradeId, c.CorrectPackageId, c.PackageUid
		from Relationship r		-- inSolution rel
			join Entity e on e.TenantId = @tenantId and e.Id = r.FromId		-- entity in question, to get UpgradeId
			join #coreContents c on e.UpgradeId = c.EntityUid				-- is entity in app library
		where 
			r.TenantId = @tenantId
			and r.TypeId in (@inSolution, @indirectInSolution)
			and r.ToId <> c.CorrectPackageId
			and not exists (
				select 1 from #repair rep
					where rep.Id = r.FromId
			)

		-- Remove any @indirectInSolution and @inSolution
		delete Relationship
		from Relationship rel
			join #repair rep
			on rel.TenantId = @tenantId
			and rel.TypeId in (@inSolution, @indirectInSolution)
			and rel.FromId = rep.Id

		-- Re-add @inSolution
		insert into Relationship (TenantId, TypeId, FromId, ToId)
		select @tenantId, @inSolution, rep.Id, rep.CorrectPackageId
		from #repair rep
			join AppRelationship ar
			on rep.CorrectPackageUid = ar.AppVerUid and rep.UpgradeId = ar.FromUid and ar.TypeUid = @inSolutionUid

		-- Re-add @indirectInSolution (which appears to be none for the core apps)
		insert into Relationship (TenantId, TypeId, FromId, ToId)
		select @tenantId, @indirectInSolution, rep.Id, rep.CorrectPackageId
		from #repair rep
			join AppRelationship ar
			on rep.CorrectPackageUid = ar.AppVerUid and rep.UpgradeId = ar.FromUid and ar.TypeUid = @indirectInSolutionUid

		-- Done
		drop table #corePackages
		drop table #repair
		drop table #coreContents

		fetch next from	cur into @tenantId
	end 

	close cur;
	deallocate cur;

END

