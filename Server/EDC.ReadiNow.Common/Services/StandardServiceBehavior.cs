// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.Core;
using System;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Configuration;
using System.ServiceModel.Description;

namespace EDC.ReadiNow.Services
{
    /// <summary>
    /// Service behaviour that scans operations for pattern attributes, and automatically adds 
    /// commonly used faults accordingly.
    /// </summary>
    public class StandardServiceBehavior : BehaviorExtensionElement, IServiceBehavior
    {
        public void ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            foreach (ServiceEndpoint ep in serviceDescription.Endpoints)
            {
                foreach (OperationDescription operation in ep.Contract.Operations)
                {
					MethodInfo method = null;

					if ( operation.BeginMethod != null && operation.EndMethod != null )
					{
						method = operation.BeginMethod;
					}
					else if ( operation.SyncMethod != null )
					{
						method = operation.SyncMethod;
					}

                    if (method == null)
                        continue;

                    object[] attributes = method.GetCustomAttributes(false);

                    // Read
                    if (HasAttribute(attributes, typeof(ReadPatternAttribute)))
                    {
                        AddFault(operation, "NotFoundFault", typeof(NotFoundFault));
                        AddFault(operation, "PermissionDeniedFault", typeof(PermissionDeniedFault));
						AddFault(operation, "PlatformSecurityFault", typeof(PlatformSecurityFault));
                    }

                    // Create
                    if (HasAttribute(attributes, typeof(CreatePatternAttribute)))
                    {
                        AddFault(operation, "PermissionDeniedFault", typeof(PermissionDeniedFault));
                        AddFault(operation, "AlreadyExistsFault", typeof(AlreadyExistsFault));
                        AddFault(operation, "ConstraintViolationFault", typeof(ConstraintViolationFault));
                        AddFault(operation, "DuplicateKeyFault", typeof(DuplicateKeyFault));
                        AddFault(operation, "DuplicateNameFault", typeof(DuplicateNameFault));
                        AddFault(operation, "ValidationFault", typeof(ValidationFault));
						AddFault(operation, "PlatformSecurityFault", typeof(PlatformSecurityFault));
                    }
                    
					// Update
                    if (HasAttribute(attributes, typeof(UpdatePatternAttribute)))
                    {
                        AddFault(operation, "NotFoundFault", typeof(NotFoundFault));
                        AddFault(operation, "PermissionDeniedFault", typeof(PermissionDeniedFault));
                        AddFault(operation, "AlreadyExistsFault", typeof(AlreadyExistsFault));
                        AddFault(operation, "ReadOnlyFault", typeof(ReadOnlyFault));
                        AddFault(operation, "ConstraintViolationFault", typeof(ConstraintViolationFault));
                        AddFault(operation, "DuplicateKeyFault", typeof(DuplicateKeyFault));
                        AddFault(operation, "DuplicateNameFault", typeof(DuplicateNameFault));
                        AddFault(operation, "ValidationFault", typeof(ValidationFault));
						AddFault(operation, "PlatformSecurityFault", typeof(PlatformSecurityFault));
                    }
                    
					// Delete
                    if (HasAttribute(attributes, typeof(DeletePatternAttribute)))
                    {
                        AddFault(operation, "NotFoundFault", typeof(NotFoundFault));
                        AddFault(operation, "PermissionDeniedFault", typeof(PermissionDeniedFault));
                        AddFault(operation, "ReadOnlyFault", typeof(ReadOnlyFault));
                        AddFault(operation, "ResourceDeleteFault", typeof(ReferencedByDependenciesFault));
						AddFault(operation, "PlatformSecurityFault", typeof(PlatformSecurityFault));
                    }

					// Application Library
	                if ( HasAttribute( attributes, typeof ( ApplicationLibraryAttribute ) ) )
	                {
						AddFault( operation, "ApplicationLibraryFault", typeof( ApplicationLibraryFault ) );
	                }
                }
            }
        }


        /// <summary>
        /// Returns whether an attribute of the specified type is in the list.
        /// </summary>
        private static bool HasAttribute(object[] attributes, Type attribType)
        {
            return
                attributes != null &&
                attributes.Any(attrib => attrib.GetType() == attribType);
        }


        /// <summary>
        /// Adds a fault to the service operation.
        /// </summary>
        private static void AddFault(OperationDescription operation, string faultName, Type faultType)
        {
			if ( !operation.Faults.Any( fault => fault.Name == faultName ) )
			{
				FaultDescription faultDescription = new FaultDescription( string.Format( "{0}/{1}/{2}", operation.DeclaringContract.Namespace, operation.DeclaringContract.Name, operation.Name ) );
				faultDescription.Name = faultName;
				faultDescription.Namespace = Constants.DataContractNamespace;
				faultDescription.DetailType = faultType;
				operation.Faults.Add( faultDescription );
			}
        }


        /// <summary>
        /// Required by IServiceBehavior.
        /// </summary>
        public void AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, System.Collections.ObjectModel.Collection<ServiceEndpoint> endpoints, System.ServiceModel.Channels.BindingParameterCollection bindingParameters)
        {
        }


        /// <summary>
        /// Required by IServiceBehavior.
        /// </summary>
        public void Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
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
            return new StandardServiceBehavior();
        }

        /// <summary>
        /// Gets the type of behaviour.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Type"/>.
        /// </returns>
        public override Type BehaviorType
        {
            get { return typeof(StandardServiceBehavior); }
        }

        #endregion
    }
}
