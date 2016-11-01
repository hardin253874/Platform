// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Globalization;

namespace EDC.Globalization
{
	/// <summary>
	///     Provides helper methods for interacting with cultures.
	/// </summary>
	public static class CultureHelper
	{
		/// <summary>
		///     Gets the name of the culture used by the current thread.
		/// </summary>
		/// <param name="type">
		///     An enumeration influencing the type of culture to return.
		/// </param>
		/// <returns>
		///     A string representing the name of the culture used by the current thread.
		/// </returns>
		public static string GetThreadCulture( CultureType type )
		{
			string culture = CultureInfo.InvariantCulture.Name;

			switch ( type )
			{
				case CultureType.Invariant:
					{
						// Retrieve the invariant culture
						culture = CultureInfo.InvariantCulture.Name;

						break;
					}

				case CultureType.Neutral:
					{
						// Walk the culture hierarchy to find the neutral culture
						CultureInfo currentCulture = CultureInfo.CurrentCulture;

						while ( ( !Equals( currentCulture, CultureInfo.InvariantCulture ) ) )
						{
							if ( currentCulture.IsNeutralCulture )
							{
								culture = currentCulture.Name;
								break;
							}

							currentCulture = currentCulture.Parent;
						}

						break;
					}

				default:
					{
						// Retrieve the specific culture used by the current thread (if available)
						CultureInfo currentCulture = CultureInfo.CurrentCulture;
						culture = currentCulture.Name;

						break;
					}
			}

			return culture;
		}

		/// <summary>
		///     Gets the name of the culture used by the user interface.
		/// </summary>
		/// <param name="type">
		///     An enumeration influencing the type of culture to return.
		/// </param>
		/// <returns>
		///     A string representing the name of the culture used by the user interface.
		/// </returns>
		public static string GetUiThreadCulture( CultureType type )
		{
			string culture = CultureInfo.InvariantCulture.Name;

			switch ( type )
			{
				case CultureType.Invariant:
					{
						// Retrieve the invariant culture
						culture = CultureInfo.InvariantCulture.Name;

						break;
					}

				case CultureType.Neutral:
					{
						// Walk the culture hierarchy to find the neutral culture
						CultureInfo currentCulture = CultureInfo.CurrentUICulture;

						while ( ( !Equals( currentCulture, CultureInfo.InvariantCulture ) ) )
						{
							if ( currentCulture.IsNeutralCulture )
							{
								culture = currentCulture.Name;
								break;
							}

							currentCulture = currentCulture.Parent;
						}

						break;
					}

				default:
					{
						// Retrieve the specific culture used by the user interface (if available)
						CultureInfo currentCulture = CultureInfo.CurrentUICulture;
						culture = currentCulture.Name;

						break;
					}
			}

			return culture;
		}
	}
}