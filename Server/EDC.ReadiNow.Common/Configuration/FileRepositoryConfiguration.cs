// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Configuration;

namespace EDC.ReadiNow.Configuration
{
    /// <summary>
	///     Defines the file repository configuration section
	/// </summary>
    public class FileRepositoryConfiguration : ConfigurationSection
    {
        /// <summary>
		///     Gets or sets individual file repository settings.
		/// </summary>
		/// <value>
		///     The individual file repository settings.
		/// </value>
		[ConfigurationProperty("fileRepositories", IsDefaultCollection = false)]
        [ConfigurationCollection(typeof(FileRepositoryConfigElement))]
        public FileRepositoryConfigElementCollection FileRepositories
        {
            get
            {
                return ((FileRepositoryConfigElementCollection)this["fileRepositories"]);
            }

            set
            {
                this["fileRepositories"] = value;
            }
        }
    }
}
