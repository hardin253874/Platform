using System.Collections.Generic;
using System.Runtime.Serialization;

namespace UpdateCopyright
{
    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    public class CopyrightConfig
    {
        /// <summary>
        /// 
        /// </summary>
        public CopyrightConfig()
        {
            Directives = new List<CopyrightDirective>();
        }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public List<CopyrightDirective> Directives { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string OldCopyrightStartsWith { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string OldCopyrightEndsWith { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string CopyrightNoticeToApply { get; set; }
    }
}
