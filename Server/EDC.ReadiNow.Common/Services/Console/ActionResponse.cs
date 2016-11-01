// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace EDC.ReadiNow.Services.Console
{
    /// <summary>
    /// Represents all data being returned for a response.
    /// </summary>
    [DataContract]
    public class ActionResponse
    {
        /// <summary>
        /// Indicates if the "special" category menu for creating new records should be shown.
        /// </summary>
        [DataMember(Name = "showNewMenu")]
        public bool? ShowNewMenu { get; set; }

        /// <summary>
        /// Indicates if the "special" category menu for exporting data should be shown.
        /// </summary>
        [DataMember(Name = "showExportMenu")]
        public bool? ShowExportMenu { get; set; }

        /// <summary>
        /// Indicates if the "Edit Inline" button should be shown.
        /// </summary>        
        [DataMember(Name = "showEditInlineButton")]
        public bool? ShowEditInlineButton { get; set; }

        /// <summary>
        /// Indicates if the actions requested may be configured for this host by the user.
        /// </summary>
        [DataMember(Name = "isEditable")]
        public bool? IsEditable { get; set; }

        /// <summary>
        /// Ordered list of actions.
        /// </summary>
        [DataMember(Name = "actions")]
        public IList<ActionMenuItemInfo> Actions { get; set; }
    }
}
