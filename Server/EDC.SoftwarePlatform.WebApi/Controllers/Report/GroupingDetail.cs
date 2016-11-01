// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;
using System.Runtime.Serialization;
using EDC.ReadiNow.Annotations;

namespace EDC.SoftwarePlatform.WebApi.Controllers.Report
{
	/// <summary>
	///     Grouping Detail class.
	/// </summary>
	[DataContract]
	public class GroupingDetail
	{
		/// <summary>
		///     Gets or sets the style.
		/// </summary>
		/// <value>
		///     The style.
		/// </value>
		[DataMember( Name = "style", EmitDefaultValue = false, IsRequired = false )]
		public string Style
		{
			get;
			set;
		}

		/// <summary>
		///		Should the style be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializeStyle( )
		{
			return Style != null;
		}

		/// <summary>
		///     Gets or sets the value.
		/// </summary>
		/// <value>
		///     The value.
		/// </value>
		[DataMember( Name = "value", EmitDefaultValue = false, IsRequired = false )]
		public string Value
		{
			get;
			set;
		}

		/// <summary>
		///		Should the value be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializeValue( )
		{
			return Value != null;
		}

		/// <summary>
		///     Gets or sets the values.
		/// </summary>
		/// <value>
		///     The values.
		/// </value>
		[DataMember( Name = "values", EmitDefaultValue = false, IsRequired = false )]
		public Dictionary<long, string> Values
		{
			get;
			set;
		}

		/// <summary>
		///		Should the values be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializeValues( )
		{
			return Values != null;
		}

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="GroupingDetail"/> is collapsed.
        /// </summary>
        /// <value>
        ///   <c>true</c> if collapsed; otherwise, <c>false</c>.
        /// </value>
        [DataMember(Name = "collapsed", EmitDefaultValue = true, IsRequired = false)]
        public bool Collapsed
        {
            get;
            set;
        }
	}
}