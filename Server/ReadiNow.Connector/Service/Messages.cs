// Copyright 2011-2016 Global Software Innovation Pty Ltd

namespace ReadiNow.Connector.Service
{
    /// <summary>
    /// Error messages to be returned to the caller.
    /// </summary>
    static class Messages
    {
        public const string PlatformInternalError = "E0001 Platform internal error.";

        /// <summary>
        /// E1xxx messages are for request errors.
        /// The text of the message will be returned, with a 400 Bad Request.
        /// </summary>

        public const string PropertyWasFormattedIncorrectly = "E1001 Value for '{0}' was formatted incorrectly.";

        public const string ResourceNotFoundByGuid = "E1002 No resource of the correct type matched the GUID '{0}'.";

        public const string ResourceNotFoundByField = "E1003 No resources were found that matched '{0}'.";

        public const string ResourceNotUniqueByField = "E1004 Multiple resources were found that matched '{0}'.";

        public const string IdentifierListContainedNulls = "E1005 Identifier list contained nulls.";

        public const string ExpectedArrayOfIdentities = "E1006 Expected an array of identities for '{0}'.";

        public const string CardinalityViolation = "E1007 Cardinality violation.";

        public const string FieldValidation = "E1008 Field validation rules were not met.";

        public const string EmptyMessageBody = "E1009 The message body was empty.";

        public const string MandatoryPropertyMissing = "E1010 '{0}' value is required.";


        /// <summary>
        /// E2xxx messages are for configuration errors.
        /// They should be returned as part of a ConnectorConfigException.
        /// The text of the message will be returned, with a 500 code.
        /// </summary>

        public const string MultipleApiKeysHaveSameValue = "E2001 Multiple API keys have the same value.";

        public const string MultipleApiHaveSameAddress = "E2002 Multiple APIs matching '{0}'.";

        public const string MultipleEndpointsHaveSameAddress = "E2003 Multiple APIs endpoint matching '{0}'.";

        public const string EndpointHasNoResourceMapping = "E2004 Endpoint has no resource mapping.";

        public const string ResourceMappingHasNoType = "E2005 Resource mapping has no resource type.";

        public const string ResourceMappingIdentityFieldIsWriteOnly = "E2006 Resource mapping identity field cannot be write-only.";

        public const string ResourceMappingIdentityFieldNotSet = "E2007 Resource mapping identity field was not set.";

        public const string FieldMappingHasNoField = "E2008 Field mapping '{0}' did not point to a field.";

        public const string RelationshipMappingHasNoRelationship = "E2009 Relationship mapping '{0}' did not point to a relationship.";


        /// <summary>
        /// Return just the code portion of an error message. Or null if it is not a platform coded message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        public static string GetPlatformCode( string message )
        {
            if ( message.StartsWith( "E" ) && message.Length > 6 && message [ 5 ] == ' ' )
            {
                return message.Substring( 0, 5 );
            }
            return null;
        }


        /// <summary>
        /// Return just the text portion of an error message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        public static string GetErrorText( string message )
        {
            if ( message.StartsWith( "E" ) && message.Length > 6 && message [ 5 ] == ' ' )
            {
                return message.Substring( 6 );
            }
            return message;
        }
    }
}
