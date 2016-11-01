// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

namespace EDC.ReadiNow.Model
{
	/// <summary>
	///     Entity Activator Delegate.
	/// </summary>
	/// <param name="args">The arguments.</param>
	/// <returns></returns>
	public delegate object EntityActivatorDelegate( params object[ ] args );

	/// <summary>
	///     Entity Activator
	/// </summary>
	public static class EntityActivator
	{
		/// <summary>
		///     Entity activator cache.
		/// </summary>
		private static readonly ConcurrentDictionary<Type, EntityActivatorDelegate> Activators = new ConcurrentDictionary<Type, EntityActivatorDelegate>( );

		/// <summary>
		///     Creates an instance of the specified type.
		/// </summary>
		/// <param name="type">The type.</param>
		/// <param name="activationData">The activation data.</param>
		/// <returns></returns>
		public static object CreateInstance( Type type, ActivationData activationData )
		{
			EntityActivatorDelegate activator = Activators.GetOrAdd( type, new Func<Type, EntityActivatorDelegate>( t => GetActivator( type ) ) );

			return activator( activationData );
		}

		/// <summary>
		///     Creates an instance of the specified type.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="activationData">The activation data.</param>
		/// <returns></returns>
		public static T CreateInstance<T>( ActivationData activationData )
		{
			return ( T ) CreateInstance( typeof ( T ), activationData );
		}

		/// <summary>
		///     Gets the activator.
		/// </summary>
		/// <param name="type">The type.</param>
		/// <returns></returns>
		public static EntityActivatorDelegate GetActivator( Type type )
		{
			ConstructorInfo ctor = type.GetConstructor( BindingFlags.NonPublic | BindingFlags.Instance, null, new[ ]
			{
				typeof ( ActivationData )
			}, null );

			/////
			// Get the parameters to the constructor.
			// Note: Sometimes the constructor will cast to an IActivationData so we need to dynamically create the arguments expression.
			/////
			ParameterInfo[ ] paramsInfo = ctor.GetParameters( );

			/////
			// Create the constructor arguments parameter.
			/////
			ParameterExpression param = Expression.Parameter( typeof ( object[ ] ), "args" );

			var argsExp = new Expression[paramsInfo.Length];

			/////
			// Create an expression for each argument type.
			/////
			for ( int i = 0; i < paramsInfo.Length; i++ )
			{
				Expression index = Expression.Constant( i );

				Type paramType = paramsInfo[ i ].ParameterType;

				Expression paramAccessorExp = Expression.ArrayIndex( param, index );

				Expression paramCastExp = Expression.Convert( paramAccessorExp, paramType );

				argsExp[ i ] = paramCastExp;
			}

			/////
			// Create an expression that will call the constructor.
			/////
			NewExpression newExp = Expression.New( ctor, argsExp );

			/////
			// Create a lambda that will call the constructor passing in the arguments.
			/////
			LambdaExpression lambda = Expression.Lambda( typeof ( EntityActivatorDelegate ), newExp, param );

			/////
			// Compile the lambda.
			/////
			var compiled = ( EntityActivatorDelegate ) lambda.Compile( );

			return compiled;
		}
	}
}