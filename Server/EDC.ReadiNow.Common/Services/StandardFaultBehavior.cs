// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.ServiceModel.Configuration;
using System.ServiceModel.Description;
using System.ServiceModel;
using System.ServiceModel.Dispatcher;

namespace EDC.ReadiNow.Services
{
	/// <summary>
	/// Standard Fault Behaviour that attaches an IErrorHandler to each endpoint defined.
	/// </summary>
	public class StandardFaultBehavior : BehaviorExtensionElement, IServiceBehavior
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
		/// Applies a custom error handler to each endpoint defined within the service.
		/// </summary>
		/// <param name="serviceDescription">
		/// Service Description.
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
					/////
					// Apply the standard fault error handler to each endpoint.
					/////
					endpointDispatcher.ChannelDispatcher.ErrorHandlers.Add( new StandardFaultErrorHandler( ) );
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
            return new StandardFaultBehavior();
	    }

	    /// <summary>
	    /// Gets the type of behaviour.
	    /// </summary>
	    /// <returns>
	    /// A <see cref="T:System.Type"/>.
	    /// </returns>
	    public override Type BehaviorType
	    {
            get { return typeof(StandardFaultBehavior); }
	    }

	    #endregion
	}
}
