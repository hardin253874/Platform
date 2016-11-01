// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;
using System.IO;
using EDC.IO;
using EDC.Security;
using EDC.SoftwarePlatform.Interfaces.EDC.ReadiNow.Model;

namespace EDC.SoftwarePlatform.Migration.SourceFormatters
{
    /// <summary>
    /// Image File Type Source Formatter
    /// </summary>
    internal class ImageFileTypeSourceFormatter : IEntityContainerSourceFormatter
    {
        /// <summary>
        /// Formats the specified source.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns>
        /// A collection of alias-&gt;value pairs.
        /// </returns>
        public IEnumerable<KeyValuePair<string, object>> Format(string source)
        {
            if (string.IsNullOrEmpty(source))
            {
                return null;
            }

            var tokenProvider = new Sha256FileTokenProvider();
            string hash;

            using (var stream = File.OpenRead(source))
            {
                hash = tokenProvider.ComputeToken(stream);
            }

            /////
            // Only member of interest here is the data hash.
            /////
            var members = new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>( "core:fileDataHash", hash )
            };

            return members;
        }        
    }
}
