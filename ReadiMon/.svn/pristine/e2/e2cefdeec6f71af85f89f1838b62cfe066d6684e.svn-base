// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using ReadiMon.Shared.Core;

namespace ReadiMon.Plugin.Entity
{
	/// <summary>
	///     Relationship Info
	/// </summary>
	/// <seealso cref="ReadiMon.Shared.Core.ViewModelBase" />
	public class RelationshipInfo : ViewModelBase
	{
		/// <summary>
		///     The selected value.
		/// </summary>
		private bool _selected;

		/// <summary>
		/// Initializes a new instance of the <see cref="RelationshipInfo" /> class.
		/// </summary>
		/// <param name="id">The identifier.</param>
		/// <param name="name">The name.</param>
		/// <param name="description">The description.</param>
		/// <param name="fromName">From name.</param>
		/// <param name="toName">To name.</param>
		/// <param name="cardinality">The cardinality.</param>
		/// <param name="solution">The solution.</param>
		/// <param name="selectedAction">The selected action.</param>
		public RelationshipInfo( long id, string name, string description, string fromName, string toName, string cardinality, string solution, Action selectedAction )
		{
			Id = id;
			Name = name;
			Description = description;
			FromName = fromName;
			ToName = toName;
			Cardinality = cardinality;
			Solution = solution;
			SelectedAction = selectedAction;
		}

		/// <summary>
		/// Gets or sets the selected action.
		/// </summary>
		/// <value>
		/// The selected action.
		/// </value>
		private Action SelectedAction
		{
			get;
			set;
		}

		/// <summary>
		///     Gets the cardinality.
		/// </summary>
		/// <value>
		///     The cardinality.
		/// </value>
		public string Cardinality
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the description.
		/// </summary>
		/// <value>
		///     The description.
		/// </value>
		public string Description
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets from name.
		/// </summary>
		/// <value>
		///     From name.
		/// </value>
		public string FromName
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the identifier.
		/// </summary>
		/// <value>
		///     The identifier.
		/// </value>
		public long Id
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the name.
		/// </summary>
		/// <value>
		///     The name.
		/// </value>
		public string Name
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets or sets a value indicating whether this <see cref="RelationshipInfo" /> is selected.
		/// </summary>
		/// <value>
		///     <c>true</c> if selected; otherwise, <c>false</c>.
		/// </value>
		public bool Selected
		{
			get
			{
				return _selected;
			}
			set
			{
				SetProperty( ref _selected, value );

				if ( SelectedAction != null )
				{
					SelectedAction( );
				}
			}
		}

		/// <summary>
		///     Gets the solution.
		/// </summary>
		/// <value>
		///     The solution.
		/// </value>
		public string Solution
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets to name.
		/// </summary>
		/// <value>
		///     To name.
		/// </value>
		public string ToName
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the tool tip.
		/// </summary>
		/// <value>
		///     The tool tip.
		/// </value>
		public string Tooltip
		{
			get
			{
				return string.Format( @"Name:		{0}
Description:	{1}
Id:		{2}
From Name:	{3}
To Name:	{4}
Cardinality:	{5}
Solution Name:	{6}", Name, Description, Id, FromName, ToName, Cardinality, Solution );
			}
		}
	}
}