// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Configuration;

namespace EDC.ReadiNow.Configuration
{
    /// <summary>
    /// Represents a file repository configuration element.
    /// </summary>
    public class FileRepositoryConfigElement : ConfigurationElement
    {
        /// <summary>
		///     Initializes a new instance of the <see cref="FileRepositoryConfigElement" /> class.
		/// </summary>
		public FileRepositoryConfigElement()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="FileRepositoryConfigElement" /> class.
        /// </summary>
        /// <param name="name">The repository name.</param>
        /// <param name="path">The repository path.</param>
        public FileRepositoryConfigElement(string name, string path) : this()
        {
            Name = name;
            Path = path;
        }

        /// <summary>
        ///     Gets or sets the file repository name.
        /// </summary>
        /// <value>
        ///     The file repository name.
        /// </value>
        [ConfigurationProperty("name", IsRequired = true, IsKey = true)]
        public string Name
        {
            get
            {
                return (string)this["name"];
            }
            set
            {
                this["name"] = value;
            }
        }

        /// <summary>
        ///     Gets or sets the path to the repository.
        /// </summary>
        /// <value>
        ///     The repository path.
        /// </value>
        [ConfigurationProperty("path", IsRequired = true, IsKey = false)]
        public string Path
        {
            get
            {
                return (string)this["path"];
            }
            set
            {
                this["path"] = value;
            }
        }        
    }
}