// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Reflection;

/////
// ReSharper disable CheckNamespace
/////

namespace EDC.ReadiNow.Model
{
	public static class ClassHelper
	{
		/// <summary>
		///     Activates an instance of this class.
		/// </summary>
		public static object Activate( this Class classEntity )
		{
            if (classEntity == null)
                throw new ArgumentNullException("classEntity");
            
			Type type = GetClass( classEntity );
			object result = Activator.CreateInstance( type );
			return result;
		}

		/// <summary>
		///     Activates an instance of this class.
		/// </summary>
		/// <typeparam name="T">A parent type that this type is presumed to be or derive from.</typeparam>
        public static T Activate<T>(this Class classEntity)
		{
            if (classEntity == null)
                throw new ArgumentNullException("classEntity");

            Type type = GetClass(classEntity);
			object oresult = Activator.CreateInstance( type );
			var result = ( T ) oresult;
			return result;
		}

		/// <summary>
		///     Gets the type represented by this class.
		/// </summary>
        public static Type GetClass(this Class classEntity)
		{
            if (classEntity == null)
                throw new ArgumentNullException("classEntity");

            if (string.IsNullOrEmpty(classEntity.AssemblyName))
			{
				throw new Exception( "AssemblyName is not specified." );
			}
            if (string.IsNullOrEmpty(classEntity.TypeName))
			{
				throw new Exception( "TypeName is not specified." );
			}

			Assembly asm = Assembly.Load( classEntity.AssemblyName );
			Type result = asm.GetType( classEntity.TypeName, true );

			return result;
		}
	}
}

/////
// ReSharper restore CheckNamespace
/////