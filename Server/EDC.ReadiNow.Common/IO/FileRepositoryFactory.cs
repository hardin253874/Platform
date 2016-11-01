// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Linq;
using EDC.IO;
using EDC.ReadiNow.Configuration;
using EDC.ReadiNow.Diagnostics;

namespace EDC.ReadiNow.IO
{
    /// <summary>
    /// File repository factory.
    /// </summary>
    internal static class FileRepositoryFactory
    {
        /// <summary>
        /// Creates a file repository with the given name and token provider.
        /// </summary>
        /// <param name="name">The name of the file repository.</param>
        /// <param name="tokenProvider">The token provider to use.</param>
        /// <param name="defaultPath">The default path for the repository.</param>        
        /// <param name="retentionPeriod">The retention period.</param>
        /// <returns>The file repository.</returns>
        internal static FileRepository CreateFileRepository(string name, IFileTokenProvider tokenProvider, string defaultPath, TimeSpan retentionPeriod)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException("name");
            }

            if (tokenProvider == null)
            {
                throw new ArgumentNullException("tokenProvider");
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException("defaultPath");
            }

            FileRepositoryConfiguration fileRepoConfig = ConfigurationSettings.GetFileRepositoryConfigurationSection();
            FileRepositoryConfigElement configElement = null;

            if (fileRepoConfig != null &&
                fileRepoConfig.FileRepositories != null)
            {
                configElement = fileRepoConfig.FileRepositories.Cast<FileRepositoryConfigElement>().FirstOrDefault(f => f.Name == name);
            }            

            string path = string.Empty;

            if (configElement != null)
            {
                path = configElement.Path;
            }

            if (string.IsNullOrWhiteSpace(path))
            {
                path = defaultPath;
            }

            return new FileRepository(name, path, tokenProvider, EventLog.Application, retentionPeriod);
        }
    }
}