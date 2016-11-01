// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;

namespace EDC.SoftwarePlatform.Migration.Processing
{
    /// <summary>
    ///     UpgradeIds for various well known aliases.
    /// </summary>
    public static class Guids
    {
        public static readonly Guid CloneEntities = new Guid( "c84720b5-f038-49ba-a601-4f4cbe6cdb7c" );

        public static readonly Guid CloneReferences = new Guid( "4c0e20fc-88ee-47b6-babb-6f444484a5f3" );

        public static readonly Guid Drop = new Guid( "8f116490-19b0-4d22-b999-5818569d49e7" );

        public static readonly Guid InSolution = new Guid( "7c77c3a0-75b5-4c59-99f6-3ba9229e6a55" );

        public static readonly Guid IndirectInSolution = new Guid( "54e16d01-c53f-407f-add8-4c906b6ca5dc" );

        public static readonly Guid Name = new Guid( "f8def406-90a1-4580-94f4-1b08beac87af" );

        public static readonly Guid Description = new Guid( "a6907c5a-19db-48ca-b9be-85d81cd9081f" );

        public static readonly Guid ReverseAlias = new Guid( "b834e145-4eb3-44a9-aec7-170f2fa354d2" );

        public static readonly Guid IsOfType = new Guid( "e1afc9e2-a526-4dc6-b90f-e2271e130f24" );
    }
}
