// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Runtime.Serialization;
using EDC.ReadiNow.Annotations;

namespace EDC.SoftwarePlatform.WebApi.Controllers.Report
{
	/// <summary>
	///     Class ReportColumnValueFormat.
	/// </summary>
	[DataContract]
	public class ReportColumnValueFormat
	{
		/// <summary>
		///     Gets or sets a value indicating whether the hide display value text.
		/// </summary>
		/// <value><c>true</c> if [hide display value]; otherwise, <c>false</c>.</value>
		[DataMember( Name = "hideval", EmitDefaultValue = false, IsRequired = false )]
		public bool HideDisplayValue
		{
			get;
			set;
		}

        /// <summary>
		///     Gets or sets a value indicating whether use default choice field formatting value.
		/// </summary>
		/// <value><c>true</c> if [hide display value]; otherwise, <c>false</c>.</value>
		[DataMember(Name = "disabledefft", EmitDefaultValue = false, IsRequired = false)]
        public bool DisableDefaultFormat
        {
            get;
            set;
        }

        /// <summary>
        ///		Should the hide display value be serialized.
        /// </summary>
        /// <returns></returns>
        [UsedImplicitly]
		private bool ShouldSerializeHideDisplayValue( )
		{
			return HideDisplayValue;
		}


		/// <summary>
		///     Gets or sets the value text alignment.
		/// </summary>
		/// <value>the value text alignment.</value>
		[DataMember( Name = "align", EmitDefaultValue = false, IsRequired = false )]
		public string Alignment
		{
			get;
			set;
		}

		/// <summary>
		///		Should the alignment be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializeAlignment( )
		{
			return Alignment != null;
		}

		/// <summary>
		///     Gets or sets the prefix.
		/// </summary>
		/// <value>The prefix.</value>
		[DataMember( Name = "prefix", EmitDefaultValue = false, IsRequired = false )]
		public string Prefix
		{
			get;
			set;
		}

		/// <summary>
		///		Should the serialize prefix.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializePrefix( )
		{
			return Prefix != null;
		}

		/// <summary>
		///     Gets or sets the suffix.
		/// </summary>
		/// <value>The suffix.</value>
		[DataMember( Name = "suffix", EmitDefaultValue = false, IsRequired = false )]
		public string Suffix
		{
			get;
			set;
		}

		/// <summary>
		///		Should the suffix be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializeSuffix( )
		{
			return Suffix != null;
		}

		/// <summary>
		///     Gets or sets the decimal places.
		/// </summary>
		/// <value>The decimal places.</value>
		[DataMember( Name = "places", EmitDefaultValue = false, IsRequired = false )]
		public long? DecimalPlaces
		{
			get;
			set;
		}

		/// <summary>
		///		Should the decimal places be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializeDecimalPlaces( )
		{
			return DecimalPlaces != null;
		}

		/// <summary>
		///     Gets or sets the date time format.
		/// </summary>
		/// <value>The date time format.</value>
		[DataMember( Name = "datetimefmt", EmitDefaultValue = false, IsRequired = false )]
		public string DateTimeFormat
		{
			get;
			set;
		}

		/// <summary>
		///		Should the date time format be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializeDateTimeFormat( )
		{
			return DateTimeFormat != null;
		}

		/// <summary>
		///		Gets or sets the number of lines.
		/// </summary>
		/// <value>
		/// The number of lines.
		/// </value>
		[DataMember( Name = "lines", EmitDefaultValue = false, IsRequired = false )]
		public long NumberOfLines
		{
			get;
			set;
		}

		/// <summary>
		///		Should the number of lines be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializeNumberOfLines( )
		{
			return NumberOfLines != 0;
		}

		// Image Handling
		/// <summary>
		///     Gets or sets the image scale unique identifier.
		/// </summary>
		/// <value>The image scale unique identifier.</value>
		[DataMember( Name = "scaleid", EmitDefaultValue = false, IsRequired = false )]
		public long? ImageScaleId
		{
			get;
			set;
		}

		/// <summary>
		///		Should the image scale identifier be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializeImageScaleId( )
		{
			return ImageScaleId != null;
		}

		/// <summary>
		///     Gets or sets the image size unique identifier.
		/// </summary>
		/// <value>The image size unique identifier.</value>
		[DataMember( Name = "sizeid", EmitDefaultValue = false, IsRequired = false )]
		public long? ImageSizeId
		{
			get;
			set;
		}

		/// <summary>
		///		Should the image size identifier be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializeImageSizeId( )
		{
			return ImageSizeId != null;
		}

		/// <summary>
		///     Gets or sets the height of the image.
		/// </summary>
		/// <value>The height of the image in pixels.</value>
		[DataMember( Name = "imgh", EmitDefaultValue = false, IsRequired = false )]
		public long? ImageHeight
		{
			get;
			set;
		}

		/// <summary>
		///		Should the height of the image be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializeImageHeight( )
		{
			return ImageHeight != null;
		}

		/// <summary>
		///     Gets or sets the width of the image.
		/// </summary>
		/// <value>The width of the image in pixels.</value>
		[DataMember( Name = "imgw", EmitDefaultValue = false, IsRequired = false )]
		public long? ImageWidth
		{
			get;
			set;
		}

		/// <summary>
		///		Should the width of the image be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializeImageWidth( )
		{
			return ImageWidth != null;
		}

        /// <summary>
		///     Gets or sets the entity list format.
		/// </summary>
		/// <value>The show list as format.</value>
		[DataMember(Name = "entitylistcolfmt", EmitDefaultValue = false, IsRequired = false)]
        public string EntityListColumnFormat
        {
            get;
            set;
        }

        /// <summary>
        ///		Should the entity list format be serialized.
        /// </summary>
        /// <returns></returns>
        [UsedImplicitly]
        private bool ShouldSerializeEntityListColumnFormat()
        {
            return EntityListColumnFormat != null;
        }
    }
}