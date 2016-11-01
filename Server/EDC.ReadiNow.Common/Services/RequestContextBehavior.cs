// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.ServiceModel;
using System.ServiceModel.Configuration;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace EDC.ReadiNow.Services
{
	/// <summary>
	/// Request Context Behaviour that automatically injects a 'ServicesHelper.SetRequestContextData( )' call prior
	/// to each WCF method invocation and a call to 'ServicesHelper.FreeRequestContextData( )' once the call is complete.
	/// </summary>
    public class RequestContextBehavior : BehaviorExtensionElement, IServiceBehavior
	{
		/// <summary>
		/// Allows for custom data to be passed to binding elements.
		/// </summary>
		/// <param name="serviceDescription">
		/// Service description.
		/// </param>
		/// <param name="serviceHostBase">
		/// Service Host Base.
		/// </param>
		/// <param name="endpoints">
		/// Endpoints at which the custom data can be set.
		/// </param>
		/// <param name="bindingParameters">
		/// Custom objects to be passed.
		/// </param>
		public void AddBindingParameters( ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, System.Collections.ObjectModel.Collection<ServiceEndpoint> endpoints, System.ServiceModel.Channels.BindingParameterCollection bindingParameters )
		{

		}

		/// <summary>
		/// Applies custom operations to the service.
		/// </summary>
		/// <param name="serviceDescription">
		/// Service description.
		/// </param>
		/// <param name="serviceHostBase">
		/// Service Host Base.
		/// </param>
		public void ApplyDispatchBehavior( ServiceDescription serviceDescription, ServiceHostBase serviceHostBase )
		{
			foreach ( ChannelDispatcher channelDispatcher in serviceHostBase.ChannelDispatchers )
			{
				foreach ( EndpointDispatcher endpointDispatcher in channelDispatcher.Endpoints )
				{
					foreach ( DispatchOperation operation in endpointDispatcher.DispatchRuntime.Operations )
					{
						operation.ParameterInspectors.Add( new RequestContextParameterInspector( ) );
					}

					/////
					// Since the parameter inspector will not be called when an exception is raised,
					// the custom error handler is used to free the context data.
					/////
					endpointDispatcher.ChannelDispatcher.ErrorHandlers.Add( new RequestContextErrorHandler( ) );
				}
			}
		}

		/// <summary>
		/// Validates the service endpoint.
		/// </summary>
		/// <param name="serviceDescription">
		/// Service description.
		/// </param>
		/// <param name="serviceHostBase">
		/// Service host base.
		/// </param>
		public void Validate( ServiceDescription serviceDescription, ServiceHostBase serviceHostBase )
		{

		}

	    #region Overrides of BehaviorExtensionElement

	    /// <summary>
	    /// Creates a behaviour extension based on the current configuration settings.
	    /// </summary>
	    /// <returns>
	    /// The behaviour extension.
	    /// </returns>
	    protected override object CreateBehavior()
	    {
            return new RequestContextBehavior();
        }

	    /// <summary>
	    /// Gets the type of behaviour.
	    /// </summary>
	    /// <returns>
	    /// A <see cref="T:System.Type"/>.
	    /// </returns>
	    public override Type BehaviorType
	    {
            get { return typeof(RequestContextBehavior); }
        }

	    #endregion
	}
}
