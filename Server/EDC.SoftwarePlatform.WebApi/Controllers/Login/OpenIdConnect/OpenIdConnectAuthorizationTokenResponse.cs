// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Runtime.Serialization;

namespace EDC.SoftwarePlatform.WebApi.Controllers.Login.OpenIdConnect
{
    /// <summary>
    ///     Represents an Open Id Connect authorization response, recieved from an identity provider.
    /// </summary>
    [DataContract]
    public class OpenIdConnectAuthorizationTokenResponse
    {        
        /// <summary>
        ///     The token type.
        /// </summary>
        [DataMember(Name = "token_type", EmitDefaultValue = false, IsRequired = true)]
        public string TokenType { get; set; }

        /// <summary>
        ///     The id token.
        /// </summary>
        [DataMember(Name = "id_token", EmitDefaultValue = false, IsRequired = true)]
        public string IdToken { get; set; }        
    }
}