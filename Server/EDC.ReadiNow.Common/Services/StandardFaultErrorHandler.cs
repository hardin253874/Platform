// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Channels;
using System.ServiceModel;

namespace EDC.ReadiNow.Services
{
	/// <summary>
	/// Custom error handler that is responsible for freeing the request context on the thread when an exception is thrown.
	/// </summary>
	public class StandardFaultErrorHandler : IErrorHandler
	{
		/// <summary>
		/// Called once the response message has been sent.
		/// </summary>
		/// <param name="error">
		/// Exception that was raised.
		/// </param>
		/// <returns>
		/// True if the session should not be aborted, False otherwise.
		/// </returns>
		public bool HandleError( Exception error )
		{
			return true;
		}

		/// <summary>
		/// Called prior to the response message being sent.
		/// </summary>
		/// <param name="error">
		/// Exception that was raised.
		/// </param>
		/// <param name="version">
		/// Message version.
		/// </param>
		/// <param name="fault">
		/// Fault.
		/// </param>
		public void ProvideFault( Exception error, MessageVersion version, ref Message fault )
		{
			if ( fault == null && error != null )
			{
				FaultException faultException = ServicesHelper.RetrieveAsFault( error );

				fault = Message.CreateMessage( version, faultException.CreateMessageFault( ), faultException.Action );
			}

			if ( fault != null )
			{
				/////
				// Ensure HTTP 200 OK is returned to stop various browsers processing the non-200 code.
				/////
				fault.Properties [ HttpResponseMessageProperty.Name ] = new HttpResponseMessageProperty { StatusCode = System.Net.HttpStatusCode.OK };
			}
		}
	}
}
