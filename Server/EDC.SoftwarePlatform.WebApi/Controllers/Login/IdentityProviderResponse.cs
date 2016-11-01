// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Runtime.Serialization;

namespace EDC.SoftwarePlatform.WebApi.Controllers.Login
{
    /// <summary>
    /// Represents basic information about an identity provider.
    /// </summary>
    [DataContract]
    public class IdentityProviderResponse
    {
        /// <summary>
        ///     The entity id of the identity provider.
        /// </summary>
        [DataMember(Name = "id", EmitDefaultValue = false, IsRequired = true)]
        public long Id { get; set; }

        /// <summary>
        ///     The name of the identity provider.
        /// </summary>
        [DataMember(Name = "name", EmitDefaultValue = false, IsRequired = true)]
        public string Name { get; set; }

        /// <summary>
        ///     The ordinal of the identity provider.
        /// </summary>
        [DataMember(Name = "ordinal", EmitDefaultValue = false, IsRequired = false)]
        public int Ordinal { get; set; }

        /// <summary>
        ///     The type alias of the identity provider.
        /// </summary>
        [DataMember(Name = "typeAlias", EmitDefaultValue = false, IsRequired = true)]
        public string TypeAlias { get; set; }        
    }
}