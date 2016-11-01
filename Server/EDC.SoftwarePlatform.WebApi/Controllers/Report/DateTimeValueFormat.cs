// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Runtime.Serialization;
using EDC.ReadiNow.Annotations;

namespace EDC.SoftwarePlatform.WebApi.Controllers.Report
{
	/// <summary>
	///     DateTime Value Format class.
	/// </summary>
	[DataContract]
	public class DateTimeValueFormat
	{
		/// <summary>
		///     Gets or sets the ordinal.
		/// </summary>
		/// <value>
		///     The ordinal.
		/// </value>
		[DataMember( Name = "ord", EmitDefaultValue = false, IsRequired = false )]
		public long Ordinal
		{
			get;
			set;
		}

		/// <summary>
		///		Should the ordinal be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializeOrdinal( )
		{
			return Ordinal != 0;
		}

		/// <summary>
		///     Gets or sets the name of the enumerated.
		/// </summary>
		/// <value>
		///     The name of the enumerated.
		/// </value>
		[DataMember( Name = "enum", EmitDefaultValue = false, IsRequired = true )]
		public string EnumeratedName
		{
			get;
			set;
		}

		/// <summary>
		///		Should the enumerated name be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializeEnumeratedName( )
		{
			return EnumeratedName != null;
		}

		/// <summary>
		///     Gets or sets the display name.
		/// </summary>
		/// <value>
		///     The display name.
		/// </value>
		[DataMember( Name = "name", EmitDefaultValue = false, IsRequired = false )]
		public string DisplayName
		{
			get;
			set;
		}

		/// <summary>
		///		Should the display name be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializeDisplayName( )
		{
			return DisplayName != null;
		}
	}
}