// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace EDC.ReadiNow.Services.Console
{
    /// <summary>
    /// Contains information about the current state of action menu items defined for an entity.
    /// </summary>
    [DataContract]
    public class ActionMenuState
    {
        /// <summary>
        /// The identifier of the report that owns the menu.
        /// </summary>
        [DataMember(Name = "reportId")]
        public long ReportId { get; set; }

        /// <summary>
        /// Where on the page, the report is being hosted.
        /// </summary>
        [DataMember(Name = "hostIds")]
        public List<long> HostIds { get; set; }

        /// <summary>
        /// The ids of the entity types that the host resource implements.
        /// </summary>
        [DataMember(Name = "hostTypeIds")]
        public List<long> HostTypeIds { get; set; }

        /// <summary>
        /// The list of actions menu items related to the entity.
        /// </summary>
        [DataMember(Name = "actions")]
        public List<ActionMenuItemInfo> Actions { get; set; }


        /// <summary>
        /// An informational message that originated at the server.
        /// </summary>
        [DataMember(Name = "message")]
        public string Message { get; set; }
    }
}