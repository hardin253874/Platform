// Copyright 2011-2016 Global Software Innovation Pty Ltd

using NUnit.Framework.Constraints;

namespace EDC.SoftwarePlatform.WebApi.Test.Infrastructure
{
	/// <summary>
	///     Constraint Expression extension methods.
	/// </summary>
	public static class ConstraintExpressionExtensionMethods
	{
		/// <summary>
		///     Extends the constraint expression by adding support for deserialized JSON structures.
		/// </summary>
		/// <param name="expression">The expression.</param>
		/// <param name="name">The name.</param>
		/// <returns></returns>
		public static ResolvableConstraintExpression JsonProperty( this ConstraintExpression expression, string name )
		{
			return expression.Append( new JsonPropertyOperator( name ) );
		}
	}
}