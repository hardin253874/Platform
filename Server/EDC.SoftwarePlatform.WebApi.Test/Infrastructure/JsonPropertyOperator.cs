// Copyright 2011-2016 Global Software Innovation Pty Ltd

using NUnit.Framework.Constraints;

namespace EDC.SoftwarePlatform.WebApi.Test.Infrastructure
{
	/// <summary>
	///     JSON Property operator.
	/// </summary>
	public class JsonPropertyOperator : SelfResolvingOperator
	{
		/// <summary>
		///     The property name
		/// </summary>
		private readonly string _name;

		/// <summary>
		///     Constructs a PropOperator for a particular named property
		/// </summary>
		public JsonPropertyOperator( string name )
		{
			_name = name;
			left_precedence = right_precedence = 1;
		}

		/// <summary>
		///     Gets the name of the property to which the operator applies
		/// </summary>
		public string Name
		{
			get
			{
				return _name;
			}
		}

		/// <summary>
		///     Reduce produces a constraint from the operator and
		///     any arguments. It takes the arguments from the constraint
		///     stack and pushes the resulting constraint on it.
		/// </summary>
		/// <param name="stack" />
		public override void Reduce( ConstraintBuilder.ConstraintStack stack )
		{
			stack.Push( new JsonPropertyConstraint( _name, stack.Pop( ) ) );
		}
	}
}