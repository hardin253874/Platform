// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Globalization;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Security;
using ReadiNow.Connector.Interfaces;

namespace ReadiNow.Connector.Service
{
	/// <summary>
	/// Generate a URI that a caller can use to refer back to a resource.
	/// </summary>
	/// <returns>The label. E.g. a guid, or some other string.</returns>
    class ResourceUriGenerator : IResourceUriGenerator
    {
        public string CreateResourceUri( IEntity instance, ConnectorRequest request, ApiResourceMapping mapping )
        {
            if ( instance == null )
                throw new ArgumentNullException( "instance" );
            if ( request == null )
                throw new ArgumentNullException( "request" );
            if ( mapping == null )
                throw new ArgumentNullException( "mapping" );

            if ( request.ApiPath.Length < 2 )
                throw new InvalidOperationException( "Cannot create a path to a resource that was not accessed using the api/endpoint pattern" );

            string resourceId = GetResourceIdentifier( instance, mapping );

            string uri = string.Concat(
                request.ControllerRootPath,
                request.TenantName,
                "/", request.ApiPath [ 0 ],
                "/", request.ApiPath [ 1 ],
                "/", resourceId );

            return uri;
        }

        /// <summary>
        /// Generate the label that can be used to identify the resource, within a type/mapping context.
        /// </summary>
        /// <param name="instance">The resource to identify.</param>
        /// <param name="mapping">The mapping context.</param>
        /// <returns>The label. E.g. a guid, or some other string.</returns>
        private static string GetResourceIdentifier( IEntity instance, ApiResourceMapping mapping )
        {
            // Get field info
            Field field;
            bool isWriteOnly;
            using ( new SecurityBypassContext( ) )
            {
                field = mapping.ResourceMappingIdentityField;
                isWriteOnly = field != null && field.IsFieldWriteOnly == true;
            }

            string result;

            if ( field == null )
            {
                Guid entityGuid = instance.UpgradeId;
                if ( entityGuid == Guid.Empty )
                    throw new Exception( "Entity GUID was empty" ); // assert false.

                result = entityGuid.ToString( "D" );
            }
            else if ( isWriteOnly )
            {
                throw new ConnectorConfigException( Messages.ResourceMappingIdentityFieldIsWriteOnly );
            }
            else
            {
                object value = instance.GetField( field );
                if ( value == null )
                {
                    // May be a config exception, or a passed-in data exception
                    throw new ConnectorConfigException( Messages.ResourceMappingIdentityFieldNotSet );
                }

                result = ConvertToJsonFormat( field, value );
            }

            return result;
        }

        private static string ConvertToJsonFormat( Field field, object value )
        {
            // TODO: Fix me
            string result = string.Format( CultureInfo.InvariantCulture, "{0}", value );
            return result;
        }
    }
}
