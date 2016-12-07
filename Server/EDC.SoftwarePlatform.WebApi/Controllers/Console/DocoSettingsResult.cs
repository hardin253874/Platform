// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Runtime.Serialization;
using EDC.Annotations;

namespace EDC.SoftwarePlatform.WebApi.Controllers.Console
{
    [DataContract]
    public class DocoSettingsResult
    {
        /// <summary>
        ///     Gets or sets the name of the doco user.
        /// </summary>
        /// <value>
        ///     The name of the doco user.
        /// </value>
        [DataMember(Name = "documentationUserName")]
        public string DocumentationUserName { get; set; }

        /// <summary>
        ///     Gets or sets the doco user password.
        /// </summary>
        /// <value>
        ///     The doco user password.
        /// </value>
        [DataMember(Name = "documentationUserPassword")]
        public string DocumentationUserPassword { get; set; }

        /// <summary>
        ///     Gets or sets the name of the documentation url.
        /// </summary>
        /// <value>
        ///     The documentation url.
        /// </value>
        [DataMember(Name = "documentationUrl")]
        public string DocumentationUrl { get; set; }

        /// <summary>
        ///     Gets or sets the contact support url.
        /// </summary>
        /// <value>
        ///     The contact support url.
        /// </value>
        [DataMember(Name = "contactSupportUrl")]
        public string ContactSupportUrl { get; set; }

        /// <summary>
        ///     Gets or sets the release notes url.
        /// </summary>
        /// <value>
        ///     The release notes url.
        /// </value>
        [DataMember(Name = "releaseNotesUrl")]
        public string ReleaseNotesUrl { get; set; }

        /// <summary>
        ///     Gets or sets the nav header documentation url.
        /// </summary>
        /// <value>
        ///     The nav header documentation url.
        /// </value>
        [DataMember(Name = "navHeaderDocumentationUrl")]
        public string NavHeaderDocumentationUrl { get; set; }

        /// <summary>
        ///     Should the user name be serialized.
        /// </summary>
        /// <returns></returns>
        [UsedImplicitly]
        private bool ShouldSerializeDocumentationUserName()
        {
            return DocumentationUserName != null;
        }


        /// <summary>
        ///     Should the user password be serialized.
        /// </summary>
        /// <returns></returns>
        [UsedImplicitly]
        private bool ShouldSerializeDocumentationUserPassword()
        {
            return DocumentationUserPassword != null;
        }

        /// <summary>
        ///     Should the documentation url be serialized.
        /// </summary>
        /// <returns></returns>
        [UsedImplicitly]
        private bool ShouldSerializeDocumentationUrl()
        {
            return DocumentationUrl != null;
        }

        /// <summary>
        ///     Should the release notes url be serialized.
        /// </summary>
        /// <returns></returns>
        [UsedImplicitly]
        private bool ShouldSerializeReleaseNotesUrl()
        {
            return ReleaseNotesUrl != null;
        }

        /// <summary>
        ///     Should the contact support url be serialized.
        /// </summary>
        /// <returns></returns>
        [UsedImplicitly]
        private bool ShouldSerializeContactSupportUrl()
        {
            return ContactSupportUrl != null;
        }

        /// <summary>
        ///     Should the nav header documentation url be serialized.
        /// </summary>
        /// <returns></returns>
        [UsedImplicitly]
        private bool ShouldSerializeNavHeaderDocumentationUrl()
        {
            return NavHeaderDocumentationUrl != null;
        }
    }
}