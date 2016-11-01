// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using EDC.Core;

namespace EDC.ReadiNow.Services
{
    /// <summary>
    /// Base class for faults relating to reading and writing resource data.
    /// </summary>
    [DataContract(Namespace = Constants.DataContractNamespace)]
    public abstract class ResourceFault
    {
    }


    /// <summary>
    /// Raised when the requested resource does not exist, or if it belongs to a different tenant.
    /// Avoid raising this fault if missing resource is not the one that the contract requested.
    /// For example, if the service call requested X, which implicitly loads Y, but Y could not be found, then an internal fault should be raised.
    /// </summary>
    [DataContract(Namespace = Constants.DataContractNamespace)]
    public class NotFoundFault : ResourceFault
    {
    }


    /// <summary>
    /// Raised when a user attempts to perform some operation that they do not have permission to perform.
    /// </summary>
    [DataContract(Namespace = Constants.DataContractNamespace)]
    public class PermissionDeniedFault : ResourceFault
    {
    }


    /// <summary>
    /// Raised when a user attempts to modify a resource that is marked as read-only.
    /// </summary>
    [DataContract(Namespace = Constants.DataContractNamespace)]
    public class ReadOnlyFault : ResourceFault
    {
    }


    /// <summary>
    /// Raised if the user attempts to create a resource when a resource with the same ID already exists.
    /// Not to be confused with DuplicateKeyFault.
    /// </summary>
    [DataContract(Namespace = Constants.DataContractNamespace)]
    public class AlreadyExistsFault : ResourceFault
    {
    }


    /// <summary>
    /// Raised if the user attempts to create or update a resource in a manner that would violate one of its field constraints.
    /// </summary>
    [DataContract(Namespace = Constants.DataContractNamespace)]
    public class ConstraintViolationFault : ResourceFault
    {
    }



    /// <summary>
    /// Raised if the user attempts to create or modify a relationship in a manner that violates the cardinality of the relationship.
    /// For example, a one-to-one relationship may already have another resource pointing at the target.
    /// </summary>
    [DataContract(Namespace = Constants.DataContractNamespace)]
    public class CardinalityViolationFault : ResourceFault
    {
    }


    /// <summary>
    /// Raised if the user attempts to create or modify a resource in a way that causes it to have the same key as an existing resource.
    /// This is only thrown if merging is not supported for the resource.
    /// Not to be confused with AlreadyExistsFault.
    /// </summary>
    [DataContract(Namespace = Constants.DataContractNamespace)]
    public class DuplicateKeyFault : ResourceFault
    {
    }

    /// <summary>
    /// Raised if the user attempts to create a resource with the same name as an existing resource.
    /// </summary>
    [DataContract(Namespace = Constants.DataContractNamespace)]
    public class DuplicateNameFault : ResourceFault
    {
    }

    /// <summary>
    /// Raised if the user attempts to delete a resource with dependent or related children resource.
    /// </summary>
    [DataContract(Namespace = Constants.DataContractNamespace)]
    public class ReferencedByDependenciesFault : ResourceFault
    {

    }

    /// <summary>
    /// Raised if the user attempts to load corrupted file.
    /// </summary>
    [DataContract(Namespace = Constants.DataContractNamespace)]
    public class FileCorruptedFault : ResourceFault
    {

    }


    /// <summary>
    /// Raised if the user attempts to save data with invalid information.
    /// </summary>
    [DataContract(Namespace = Constants.DataContractNamespace)]
    public class ValidationFault : ResourceFault
    {

    }


    /// <summary>
    /// Raised if the user attempts to save a relationship that would result in a disallowed circular dependency.
    /// For example, if a structure level is moved to be under itself, or one of its descendants.
    /// </summary>
    [DataContract(Namespace = Constants.DataContractNamespace)]
    public class CircularRelationshipFault : ResourceFault
    {

    }

	/// <summary>
	/// A security demand has failed.
	/// </summary>
	[DataContract(Namespace = Constants.DataContractNamespace)]
	public class PlatformSecurityFault : ResourceFault
	{

	}

	/// <summary>
	/// A problem with the application library has been encountered.
	/// </summary>
	[DataContract( Namespace = Constants.DataContractNamespace )]
	public class ApplicationLibraryFault : ResourceFault
	{

	}
}
