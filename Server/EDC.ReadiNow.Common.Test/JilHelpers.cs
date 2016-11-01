// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.ReadiNow.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EDC.ReadiNow.Test
{
    /// <summary>
    /// Helper so we can use Jil serializer.
    /// Unfortunately Jil is unsigned. So we can't reference it from our signed test assemblies.
    /// (And if we don't sign them, then we can't have them accessing the internals of our GACed signed assemblies).
    /// </summary>
    public static class JilHelpers
    {
        /// <summary>
        /// Invoke Jil.JSON.Deserialize
        /// </summary>
        public static T Deserialize<T>( string json )
        {
            MethodInfo genericMethod = GetDeserializeMethod( );
            MethodInfo specificMethod = genericMethod.MakeGenericMethod( typeof( T ) );
            object result = specificMethod.Invoke( null, new object [ ] { json, null } );
            return ( T ) result;
        }

        private static MethodInfo GetDeserializeMethod( )
        {
            if ( _genericDeserializeMethod == null )
            {
                string install = SpecialFolder.GetSpecialFolderPath( SpecialMachineFolders.Install );
                string jilPath = Path.Combine(install, @"SpApi\bin\Jil.dll");

                Assembly assembly = Assembly.LoadFrom( jilPath );

                Type jsonType = assembly.GetType( "Jil.JSON" );
                Type optionsType = assembly.GetType( "Jil.Options" );

                _genericDeserializeMethod = jsonType.GetMethod( "Deserialize",
                    BindingFlags.Static | BindingFlags.Public | BindingFlags.InvokeMethod,
                    null, new Type [ ] { typeof( string ), optionsType }, null );

                if ( _genericDeserializeMethod == null )
                    throw new Exception( "Could not access Jil.JSON.Deserialize" );
            }

            return _genericDeserializeMethod;
        }
        static MethodInfo _genericDeserializeMethod;
    }
}
