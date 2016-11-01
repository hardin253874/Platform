// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Dispatcher;
using EDC.ReadiNow.IO;

namespace EDC.ReadiNow.Services
{
	/// <summary>
	/// Custom parameter inspector that is used to inject request context data into each call.
	/// </summary>
	public class RequestContextParameterInspector : IParameterInspector
	{
		/// <summary>
		/// Injects the call to 'ServicesHelper.SetRequestContextData( )' prior to the WCF method being invoked.
		/// </summary>
		/// <param name="operationName">
		/// Name of the WCF method being called.
		/// </param>
		/// <param name="inputs">
		/// Parameters being passed to the method.
		/// </param>
		/// <returns>
		/// Correlation state.
		/// </returns>
		public object BeforeCall( string operationName, object [ ] inputs )
		{
			if ( ServicesHelper.IsRequestContextDataSet )
			{
				ServicesHelper.SetRequestContextData( );
			}

			return null;
		}

		/// <summary>
		/// Injects a call to 'ServicesHelper.FreeRequestContextData( )' after the WCF method call has completed.
		/// </summary>
		/// <param name="operationName">
		/// Name of the WCF method being called.
		/// </param>
		/// <param name="outputs">
		/// Parameters being passed out of the method.
		/// </param>
		/// <param name="returnValue">
		/// Methods return value.
		/// </param>
		/// <param name="correlationState">
		/// Correlation state returned by the BeforeCall method.
		/// </param>
		public void AfterCall( string operationName, object [ ] outputs, object returnValue, object correlationState )
		{
			ServicesHelper.FreeRequestContextData( );
		}
	}
}
