// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using EDC.Core;
using EDC.ReadiNow.Annotations;
using EventLog = EDC.ReadiNow.Diagnostics.EventLog;

namespace EDC.ReadiNow.Model
{
	/// <summary>
	///     Encapsulates any possible way of referring to an entity.
	/// </summary>
	/// <remarks>
	///     Note: All properties should be marked with XmlIgnore, as XML serialization is handled via the special
	///     XmlSerializationText and XmlSerializationEntityRefAttribute properties.
	/// </remarks>
	[DataContract( Namespace = Constants.DataContractNamespace, IsReference = true )]
	[Serializable]
	[XmlType( TypeName = "entityRef" )]
    [DebuggerDisplay("EntityRef {ToString()}")]
    public class EntityRef : IEntityRef, IEquatable<EntityRef>
	{
		private IEntity _entity;
		private long _id;
		private bool _serializing;

        bool _haveNamespace;
        bool _haveAlias;
        private string _namespace;
        private string _alias;


		#region Constructors

		/// <summary>
		///     Default constructor
		/// </summary>
		[DebuggerStepThrough]
		public EntityRef( )
		{
		}

		/// <summary>
		///     Creates a new EntityRef from an Int64 Id.
		/// </summary>
		[DebuggerStepThrough]
		public EntityRef( long id )
		{
			_id = id;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="EntityRef"/> class.
		/// </summary>
		/// <param name="entityRef">The entity reference.</param>
		/// <exception cref="System.ArgumentNullException">entityRef</exception>
		public EntityRef( IEntityRef entityRef )
		{
			if ( entityRef == null )
			{
				throw new ArgumentNullException( "entityRef" );
			}

			if ( entityRef.HasEntity )
			{
				_entity = entityRef.Entity;
            }
            else
            {
                Namespace = entityRef.Namespace;   // we do not fetch the namespace and alias from entities as it may force a field fetch.
                Alias = entityRef.Alias;
            }
            
			Id = entityRef.Id;
		}

		/// <summary>
		///     Creates a new EntityRef from a string alias.
		///     Can be namespace:alias, or just alias if alias is in the core namespace.
		/// </summary>
		public EntityRef( string alias )
		{
			int i = alias.IndexOf( ':' );
            long id;
			if ( i >= 0 )
			{
				Namespace = alias.Substring( 0, i );
				Alias = alias.Substring( i + 1 );
			}
			else if (long.TryParse(alias, out id))
            {
                Id = id;
            }
            else
			{
				Namespace = "core";
				Alias = alias;
			}
		}

		/// <summary>
		///     Creates a new EntityRef from a string alias and namespace.
		/// </summary>
		[DebuggerStepThrough]
		public EntityRef( string ns, string alias )
		{
			Namespace = ns;
			Alias = alias;
		}

		/// <summary>
		///     Creates a new EntityRef from an entity.
		/// </summary>
		[DebuggerStepThrough]
		public EntityRef( IEntity entity )
		{
			_entity = entity;
			Id = entity.Id;
		}

        /// <summary>
        ///     Creates a new EntityRef from an entity, but also set the ns and alias.
        /// </summary>
        [DebuggerStepThrough]
        public EntityRef(IEntity entity, string nsAlias)
        {
            _entity = entity;
            Id = entity.Id;
            if (nsAlias != null)
            {
                var parts = nsAlias.Split(':');
                Namespace = parts[0];
                Alias = parts[1];
            }
        }


		#endregion

		/// <summary>
		///     Returns true if this EntityRef doesn't point to anything.
		/// </summary>
		[XmlIgnore]
		public bool IsEmpty
		{
			get
			{
				return _id == 0 && string.IsNullOrEmpty( Alias );
			}
		}

		/// <summary>
		///     Gets/sets an entity alias. This only returns the set value, it does not automatically resolve on demand.
		/// </summary>
		[DataMember( EmitDefaultValue = false )]
		[XmlIgnore]
		public string Alias
		{
            get
            {
                if (!_haveAlias && _entity != null)
                    Alias = _entity.Alias;

                return _alias;
            }
            set {
                _haveAlias = true;
                _alias = value;
            }
        }

		/// <summary>
		///		Should the alias be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeAlias( )
	    {
		    return Alias != null;
	    }

		/// <summary>
		///     Returns the entity referenced by this EntityRef. Resolves a read-only copy if necessary.
		/// </summary>
		[XmlIgnore]
		public IEntity Entity
		{
			get
			{
				if ( _entity == null )
				{
					_entity = Model.Entity.Get( Id );
#if DEBUG
					if ( _entity == null )
					{
						EventLog.Application.WriteWarning( "EntityRef.Entity returning null for id {0}\n{1}", Id, Environment.StackTrace );
					}
#endif
				}
				return _entity;
			}
		}

		/// <summary>
		///     Returns the entity referenced by this EntityRef, if set.
		/// </summary>
        [XmlIgnore]
        public IEntity EntityPeek
        {
            get
            {
                return _entity;
            }
        }

		/// <summary>
		///     Gets a value indicating whether this instance has entity.
		/// </summary>
		/// <value>
		///     <c>true</c> if this instance has entity; otherwise, <c>false</c>.
		/// </value>
		[XmlIgnore]
		public bool HasEntity
		{
			get
			{
				return _entity != null;
			}
		}

		/// <summary>
		///     Gets a value indicating whether this instance has a valid id.
		/// </summary>
		/// <value>
		///     <c>true</c> if this instance has id; otherwise, <c>false</c>.
		/// </value>
		[XmlIgnore]
		public bool HasId
		{
			get
			{
				return _id != 0;
			}
		}

		/// <summary>
		///     Gets/sets the Id used to reference an Entity.
		///     If the entity reference was specified in some other manner, the Id will be resolved and returned.
		/// </summary>
		[DataMember( EmitDefaultValue = false )]
		[XmlIgnore]
		public long Id
		{
			[DebuggerStepThrough]
			get
			{
				// If the object is being serialized do not resolve the id as the resolution will
				// happen after the RequestContext is Disposed.
				ResolveId( );

				return _id;
			}
			[DebuggerStepThrough]
			set
			{
				_id = value;
			}
		}

		/// <summary>
		///		Should the identifier be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeId( )
	    {
		    return Id != 0;
	    }

		/// <summary>
		/// Resolves the identifier.
		/// </summary>
		public long ResolveId( )
		{
			if ( _id == 0 && !_serializing )
			{
				if ( _entity != null )
				{
					_id = _entity.Id;
				}
				else if ( Alias != null )
				{
                    EntityAlias ea = new EntityAlias( Namespace ?? "core", Alias, true );
                    _id = EntityIdentificationCache.GetId( ea );
				}
				else
				{
					return 0;
				}
			}

			return _id;
		}


		/// <summary>
        ///     Gets/sets an entity alias namespace. This only returns the set value, it does not automatically resolve on demand.
        /// </summary>
        [DataMember( EmitDefaultValue = false )]
		[XmlIgnore]
		public string Namespace
		{
			get
            {
                if (!_haveNamespace && _entity != null)
                    Namespace = _entity.Namespace;

                return _namespace;
            }
            set
            {
                _haveNamespace = true;
                _namespace = value;
            }
		}

		/// <summary>
		///		Should the namespace be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeNamespace( )
	    {
		    return Namespace != null;
	    }

		/// <summary>
		///     Returns a <see cref="System.String" /> that represents this entity reference.
		/// </summary>
		/// <remarks>Primarily to assist debugging.</remarks>
		public override string ToString( )
		{
			string result = _id.ToString( CultureInfo.InvariantCulture );
			if ( Alias != null )
			{
				result += " ";
				if ( Namespace != null )
				{
					result += Namespace + ":";
				}
				result += Alias;
			}

			return result;
		}

		/// <summary>
		/// To the safe string.
		/// </summary>
		/// <returns></returns>
		public string ToSafeString( )
		{
			string result = string.Empty;

			if ( HasId )
			{
				result = _id.ToString( );
			}

			if ( _haveAlias && !string.IsNullOrEmpty( _alias ) )
			{
				result += $" ({_alias})";
			}

			return result;
		}

		#region XML Serialization

		/// <summary>
		///     This entityRef=true attribute is used to mark the parent property element so that various utilities, such as import/export, are
		///     able to perform transforms on any elements that are entity refs without needing to know the specific element names.
		/// </summary>
		[XmlAttribute( AttributeName = "entityRef" )]
		public string XmlSerializationEntityRefAttribute
		{
			/////
			// ReSharper disable ValueParameterNotUsed
			/////
			get
			{
				return "true";
			}
			set
			{
				/////
				// Required for xml serialization
				/////
			}
			/////
			// ReSharper restore ValueParameterNotUsed
			/////
		}

		/// <summary>
		///     All XML serialization of EntityRef is done via this property, so that it appears as an inline text (for tidy XML).
		/// </summary>
		[XmlText]
		public string XmlSerializationText
		{
			get
			{
				return ToXmlString( this );
			}
			set
			{
				FromXmlString( value, this );
			}
		}

        /// <summary>
        ///     Parses an XML string that contains an EntityRef.
        /// </summary>
        public static EntityRef FromXmlString(string value)
	    {
	        var result = new EntityRef();
	        FromXmlString(value, result);
            return result;
	    }


	    /// <summary>
		///     Parses an XML string that contains an EntityRef.
		/// </summary>
		private static void FromXmlString( string value, EntityRef target )
		{
			// Empty
			if ( string.IsNullOrEmpty( value ) )
			{
				return;
			}

			// Id
			long id;
			if ( long.TryParse( value, out id ) )
			{
				target.Id = id;
				return;
			}

			// Alias
			int i = value.IndexOf( ":", StringComparison.OrdinalIgnoreCase );
			if ( i == -1 )
			{
				target.Alias = value;
			}
			else
			{
				target.Namespace = value.Substring( 0, i );
				target.Alias = value.Substring( i + 1 );
			}
		}

		/// <summary>
		///     Called when serialization is complete.
		/// </summary>
		/// <param name="c">The c.</param>
		[OnSerialized]
		private void OnSerialized( StreamingContext c )
		{
			// Restore automatic evaluation of 'ID' if it has not been explicitly specified.
			_serializing = false;
		}

		/// <summary>
		///     Called when serializing entity refs.
		/// </summary>
		/// <param name="c">The c.</param>
		[OnSerializing]
		private void OnSerializing( StreamingContext c )
		{
			// Suppress automatic evaluation of 'ID' if it has not been explicitly specified.
			_serializing = true;
		}

		/// <summary>
		///     Formats an EntityRef for serialization into an XML document.
		/// </summary>
		private static string ToXmlString( EntityRef entity )
		{
			// Empty
			if ( entity == null )
			{
				return string.Empty;
			}

			// Alias
			if ( !string.IsNullOrEmpty( entity.Alias ) )
			{
				if ( !string.IsNullOrEmpty( entity.Namespace ) )
				{
					return entity.Namespace + ":" + entity.Alias;
				}

				return entity.Alias;
			}

			// Id
			return entity.Id.ToString( CultureInfo.InvariantCulture );
		}

		#endregion

		#region Conversions

		/// <summary>
		///     Performs an implicit conversion from an Int64 Id to an EntityRef.
		/// </summary>
		/// <param name="id">The id.</param>
		[DebuggerStepThrough]
		public static implicit operator EntityRef( long id )
		{
			return new EntityRef( id );
		}

		/// <summary>
		///     Performs an implicit conversion from a string alias to an EntityRef. Assumed to be in the 'core' namespace.
		/// </summary>
		/// <param name="alias">The alias.</param>
		[DebuggerStepThrough]
		public static implicit operator EntityRef( string alias )
		{
			return EntityRefHelper.ConvertAliasWithNamespace( alias );
		}

		/// <summary>
		///     Performs an implicit conversion from an entity to an EntityRef.
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <returns></returns>
		[DebuggerStepThrough]
		public static implicit operator EntityRef( Entity entity )
		{
             return entity != null ? new EntityRef( entity ) : null;
		}

		#endregion Conversations


        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        public override int GetHashCode()
        {
            long id;

            // Ensure GetHashCode doesn't throw
            try
            {
                id = Id;
            }
            catch (Exception ex)
            {
                id = 0;
                EventLog.Application.WriteTrace("An error occured getting the EntityRef hashcode. Error {0}", ex.ToString());
            }

            return id.GetHashCode();
        }


        /// <summary>
        /// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
        /// </summary>
        /// <param name="right">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object right)
        {
            return Equals(right as EntityRef);
        }


        /// <summary>
        /// Determines whether the specified entity ref is equal to this instance.
        /// </summary>
        /// <param name="right">The entity ref to compare with this instance.</param>
        /// <returns></returns>
        public bool Equals(EntityRef right)
        {
            bool result;

            try
            {
                result = right != null &&
                         right.Id == Id;
            }
            catch (Exception ex)
            {
                result = false;
                EventLog.Application.WriteTrace("An error occured comparing EntityRefs. Error {0}", ex.ToString());
            }

            return result;
        }
    }
}