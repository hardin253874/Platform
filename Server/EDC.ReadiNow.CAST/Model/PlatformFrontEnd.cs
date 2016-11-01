// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using EDC.ReadiNow.CAST.Template.Model;
using EDC.ReadiNow.Model;

namespace EDC.ReadiNow.CAST.Model
{
    /// <summary>
    /// Holds information about a frontend connected to a <see cref="ManagedPlatform"/>.
    /// </summary>
    [Serializable]
    [ModelClass(PlatformFrontEndSchema.PlatformFrontEndType)]
    public class PlatformFrontEnd : StrongEntity, IPlatformFrontEnd
    {
        #region Entity Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PlatformFrontEnd" /> class.
        /// </summary>
        public PlatformFrontEnd() : base(typeof(PlatformFrontEnd)) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="PlatformFrontEnd" /> class.
        /// </summary>
        /// <param name="activationData">The activation data.</param>
        internal PlatformFrontEnd(IActivationData activationData) : base(activationData) { }

        #endregion

        /// <summary>
        /// The name given to the entity.
        /// </summary>
        public string Name
        {
            get { return (string)GetField(PlatformFrontEndSchema.NameField); }
            set { SetField(PlatformFrontEndSchema.NameField, value); }
        }

        /// <summary>
        /// The domain that the frontend is a member of.
        /// </summary>
        public string Domain
        {
            get { return (string)GetField(PlatformFrontEndSchema.DomainField); }
            set { SetField(PlatformFrontEndSchema.DomainField, value); }
        }

        /// <summary>
        /// The host name that the frontend installation is running on.
        /// </summary>
        public string Host
        {
            get { return (string)GetField(PlatformFrontEndSchema.HostField); }
            set { SetField(PlatformFrontEndSchema.HostField, value); }
        }

        /// <summary>
        /// The last time that the frontend with this information was registered.
        /// </summary>
        public DateTime LastContact
        {
            get { return (DateTime)GetField(PlatformFrontEndSchema.LastContactField); }
            set { SetField(PlatformFrontEndSchema.LastContactField, value); }
        }

        #region Internals

        internal static string PlatformFrontEndPreloadQuery
        {
            get
            {
                return "alias,name,isOfType.{alias,name}," +
                        PlatformFrontEndSchema.DomainField + "," +
                        PlatformFrontEndSchema.HostField + "," +
                        PlatformFrontEndSchema.LastContactField;
            }
        }

        #endregion
    }
}
