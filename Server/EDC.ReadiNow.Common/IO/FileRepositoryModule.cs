// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using Autofac;
using EDC.IO;

namespace EDC.ReadiNow.IO
{
    /// <summary>
    ///     Autofac dependency injection module for the file repository module.
    /// </summary>
    public class FileRepositoryModule : Module
    {
        private const string ApplicationLibraryFileRepositoryConfigName = "Application Library";
        private const string TemporaryFileRepositoryConfigName = "Temporary";
        private const string BinaryFileRepositoryConfigName = "Binary";
        private const string DocumentFileRepositoryConfigName = "Document";

        public const string ApplicationLibraryFileRepositoryName = ApplicationLibraryFileRepositoryConfigName + " File Repository";
        public const string TemporaryFileRepositoryName = TemporaryFileRepositoryConfigName + " File Repository";
        public const string BinaryFileRepositoryName = BinaryFileRepositoryConfigName + " File Repository";
        public const string DocumentFileRepositoryName = DocumentFileRepositoryConfigName + "  File Repository";


        /// <summary>
        ///     Perform any registrations
        /// </summary>
        /// <param name="builder">The autofac container builder.</param>
        protected override void Load(ContainerBuilder builder)
        {
            var sha256TokenProvider = new Sha256FileTokenProvider();
            var randomTokenProvider = new RandomFileTokenProvider();

            // Register file repositories            
            builder.Register(c => FileRepositoryFactory.CreateFileRepository(ApplicationLibraryFileRepositoryConfigName, sha256TokenProvider, @"C:\PlatformFileRepos\AppLibrary", TimeSpan.MaxValue))
                .Named<FileRepository>( ApplicationLibraryFileRepositoryName )
                .Named<IFileRepository>( ApplicationLibraryFileRepositoryName )
                .As<IFileRepository>()
                .SingleInstance();

            // Only keep files for 1 day
            builder.Register(c => FileRepositoryFactory.CreateFileRepository(TemporaryFileRepositoryConfigName, randomTokenProvider, @"C:\PlatformFileRepos\Temporary", TimeSpan.FromDays(1)))
                .Named<FileRepository>( TemporaryFileRepositoryName )
                .Named<IFileRepository>( TemporaryFileRepositoryName )
                .As<IFileRepository>()
                .SingleInstance();

            builder.Register(c => FileRepositoryFactory.CreateFileRepository(BinaryFileRepositoryConfigName, sha256TokenProvider, @"C:\PlatformFileRepos\Binary", TimeSpan.MaxValue))
                .Named<FileRepository>( BinaryFileRepositoryName )
                .Named<IFileRepository>( BinaryFileRepositoryName )
                .As<IFileRepository>()
                .SingleInstance();

            builder.Register(c => FileRepositoryFactory.CreateFileRepository(DocumentFileRepositoryConfigName, sha256TokenProvider, @"C:\PlatformFileRepos\Document", TimeSpan.MaxValue))
                .Named<FileRepository>( DocumentFileRepositoryName )
                .Named<IFileRepository>( DocumentFileRepositoryName )
                .As<IFileRepository>()
                .SingleInstance();
        }
    }
}