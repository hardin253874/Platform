// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Runtime.Serialization;
using EDC.ReadiNow.Annotations;

namespace EDC.SoftwarePlatform.WebApi.Controllers.Report
{
	/// <summary>
	///     Aggregate Detail class.
	/// </summary>
	[DataContract]
	public class AggregateDetail
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
		///     Gets or sets the type.
		/// </summary>
		/// <value>
		///     The type.
		/// </value>
		[DataMember( Name = "type", EmitDefaultValue = false, IsRequired = false )]
		public string Type
		{
			get;
			set;
		}

		/// <summary>
		///		Should the type be serializes.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializeType( )
		{
			return Type != null;
		}
	}
}