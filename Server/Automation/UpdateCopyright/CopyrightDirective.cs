using System.Collections.Generic;
using System.Runtime.Serialization;

namespace UpdateCopyright
{
    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    public class CopyrightDirective
    {
        /// <summary>
        /// 
        /// </summary>
        public CopyrightDirective()
        {
            ExcludeSubPaths = new List<string>();
            ExcludeFiles = new List<string>();
        }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string Path { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public List<string> ExcludeSubPaths { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public List<string> ExcludeFiles { get; set; }
    }
}
