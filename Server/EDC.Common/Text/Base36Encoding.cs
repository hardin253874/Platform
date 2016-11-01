// Copyright 2011-2016 Global Software Innovation Pty Ltd
namespace EDC.Text
{
	/// <summary>
	///     Base 36 Encoding.
	/// </summary>
	public class Base36Encoding : RadixEncoding
	{
		/// <summary>
		///     Base 36 alphabet.
		/// </summary>
		private const string Base36Alphabet = "0123456789abcdefghijklmnopqrstuvwxyz";

		/// <summary>
		///     Initializes a new instance of the <see cref="Base36Encoding" /> class.
		/// </summary>
		public Base36Encoding( )
			: base( Base36Alphabet, 36 )
		{
		}
	}
}