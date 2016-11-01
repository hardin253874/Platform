// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Configuration;
using NUnit.Framework;

namespace EDC.ReadiNow.Test
{
	/// <summary>
	///     The ReadiNow test attribute class.
	/// </summary>
	/// <seealso cref="NUnit.Framework.TestActionAttribute" />
	/// <seealso cref="NUnit.Framework.ITestAction" />
	public class ReadiNowTestAttribute : TestActionAttribute
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="ReadiNowTestAttribute" /> class.
		/// </summary>
		public ReadiNowTestAttribute( )
		{
			/////
			// This configuration manager call is required to get work around the .NET 4.5.1
			// runtime change documented at https://msdn.microsoft.com/en-us/library/dn458353(v=vs.110).aspx
			/////
			ConfigurationManager.GetSection( "system.xml/xmlReader" );
		}
	}
}