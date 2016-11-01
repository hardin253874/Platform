// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Collections.Generic;
using System.Runtime.Serialization;
using EDC.ReadiNow.Model;

namespace ReadiNow.Reporting
{
	/// <summary>
	/// </summary>
	[DataContract]
	public class ReportValidationResult
	{
		#region Constructors

		/// <summary>
		///     Initializes a new instance of the <see cref="ReportValidationResult" /> class.
		/// </summary>
		public ReportValidationResult( )
		{
			InvalidEntityRefs = new List<EntityRef>( );
		}

		#endregion

		#region Properties

		/// <summary>
		///     Gets or sets the invalid entity refs.
		/// </summary>
		/// <value>
		///     The invalid entity refs.
		/// </value>
		[DataMember]
		public IList<EntityRef> InvalidEntityRefs
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets a value indicating whether the report being validated is valid.
		/// </summary>
		/// <value>
		///     <c>true</c> if the report being validated is valid; otherwise, <c>false</c>.
		/// </value>
		[DataMember]
		public bool IsValid
		{
			get;
			set;
		}

		#endregion Properties
	}
}