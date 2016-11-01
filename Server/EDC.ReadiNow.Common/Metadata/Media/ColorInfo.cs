// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Runtime.Serialization;
using EDC.Core;

namespace EDC.ReadiNow.Metadata.Media
{
	/// <summary>
	///     Defines a color. This class is intended to be transmitted via WCF.
	/// </summary>
	[DataContract( Namespace = Constants.DataContractNamespace )]
	public class ColorInfo
	{
		/// <summary>
		///     Gets or sets the sRGB alpha channel value of the color.
		/// </summary>
		[DataMember]
		public byte A
		{
			get;
			set;
		}


		/// <summary>
		///     Gets or sets the sRGB blue channel value of the color.
		/// </summary>
		[DataMember]
		public byte B
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the sRGB green channel value of the color.
		/// </summary>
		[DataMember]
		public byte G
		{
			get;
			set;
		}

		/// Gets or sets the sRGB red channel value of the color.
		[DataMember]
		public byte R
		{
			get;
			set;
		}

        /// <summary>
        /// Returns a hex string rrggbb.
        /// </summary>
	    public string GetRgbHex()
	    {
	        return string.Format("{0:x2}{1:x2}{2:x2}", R, G, B);
	    }

        /// <summary>
        /// Returns a hex string aarrggbb.
        /// </summary>
        public string GetArgbHex()
        {
            return string.Format("{0:x2}{1:x2}{2:x2}{3:x2}", A, R, G, B);
        }
	}
}