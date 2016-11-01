// Copyright 2011-2016 Global Software Innovation Pty Ltd

namespace EDC.ReadiNow.IO
{
    /// <summary>
    /// File details class.
    /// </summary>
    public class FileDetails
    {
        /// <summary>
        /// The name of the file.
        /// </summary>
        public string Filename { get; internal set; }

        /// <summary>
        /// The content type.
        /// </summary>
        public string ContentType { get; internal set; }
    }
}