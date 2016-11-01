// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Collections.Generic;
using System.Runtime.Serialization;
using EDC.SoftwarePlatform.Services.ApplicationManager;

namespace EDC.ReadiNow.CAST.Contracts
{
    /// <summary>
    /// Packages information about the presently installed applications.
    /// </summary>
    [DataContract]
    public class ApplicationInfoResponse : CastResponse
    {
        /// <summary>
        /// A list of the installed apps.
        /// </summary>
        [DataMember(Name = "installed")]
        public IList<InstalledApplication> Installed { get; set; }
    }
}
