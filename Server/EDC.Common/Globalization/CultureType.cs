// Copyright 2011-2016 Global Software Innovation Pty Ltd
namespace EDC.Globalization
{
	/// <summary>
	///     Defines the culture types.
	/// </summary>
	public enum CultureType
	{
		/// <summary>
		///     Represents a culture that is not associated with either a language or a country/region.
		/// </summary>
		Invariant,

		/// <summary>
		///     Represents a culture that is associated with a language but not with a country/region.
		/// </summary>
		Neutral,

		/// <summary>
		///     Represents a culture that is associated with a language and a country/region.
		/// </summary>
		Specific
	}
}