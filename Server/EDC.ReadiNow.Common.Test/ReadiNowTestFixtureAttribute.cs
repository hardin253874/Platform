// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Configuration;
using NUnit.Framework;

namespace EDC.ReadiNow.Test
{
	/// <summary>
	///     The ReadiNow test fixture class.
	/// </summary>
	/// <seealso cref="NUnit.Framework.TestFixtureAttribute" />
	/// <remarks>
	///     Intended to be used in test fixtures where no other [ReadiNowTest] attribute is present.
	/// </remarks>
	public class ReadiNowTestFixtureAttribute : TestFixtureAttribute
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="ReadiNowTestFixtureAttribute" /> class.
		/// </summary>
		public ReadiNowTestFixtureAttribute( )
		{
			/////
			// This configuration manager call is required to get work around the .NET 4.5.1
			// runtime change documented at https://msdn.microsoft.com/en-us/library/dn458353(v=vs.110).aspx
			/////
			ConfigurationManager.GetSection( "system.xml/xmlReader" );
		}
	}
}