// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Concurrent;
using EDC.ReadiNow.IO;

namespace EDC.ReadiNow.Model
{
    /// <summary>
    /// Helper class to make the resolution of alises to IDs really fast for a few frequently used aliases.
    /// </summary>
    public class WellKnownAliases
    {
        /// <summary>
        /// Cache of all wellknown alias lookups
        /// </summary>
        private static readonly ConcurrentDictionary<long, WellKnownAliases> _cache = new ConcurrentDictionary<long, WellKnownAliases>( );

        /// <summary>
        /// Wellknown instance for current tenant.
        /// </summary>
        [ThreadStatic]
        private static WellKnownAliases _current;

        /// <summary>
        /// Tenant that it applies to.
        /// </summary>
        private readonly long _tenantId;

        /// <summary>
        /// Private constructor
        /// </summary>
        /// <param name="tenantId">Tenant that these aliases apply to</param>
        private WellKnownAliases( long tenantId )
        {
            _tenantId = tenantId;
        }

        /// <summary>
        /// Return an instance that relates to the current tenant.
        /// </summary>
        public static WellKnownAliases CurrentTenant
        {
            get
            {
                long currentTenant = RequestContext.TenantId;

                if ( _current == null || _current._tenantId != currentTenant )
                {
                    _current = _cache.GetOrAdd( currentTenant, tenant => new WellKnownAliases( tenant ) );
                }

                return _current;
            }
        }

        private long GetId( ref long stored, string ns, string alias )
        {
            if ( stored == 0 )
            {
                // (Unfortunately) presently the fastest way to get an ID.
                EntityAlias ea = new EntityAlias( ns, alias, true );
                stored = EntityIdentificationCache.GetId( ea );
            }
            return stored;
        }

        /// <summary>
        /// alias alias ID.
        /// </summary>
        public long Alias => GetId(ref _alias, "core", "alias" );
        private long _alias;

        /// <summary>
        /// autoNumberField alias ID.
        /// </summary>
        public long AutoNumberField => GetId( ref _autoNumberField, "core", "autoNumberField" );
        private long _autoNumberField;

        /// <summary>
        /// autoNumberValue alias ID.
        /// </summary>
        public long AutoNumberValue => GetId( ref _autoNumberValue, "core", "autoNumberValue" );
        private long _autoNumberValue;

        /// <summary>
        /// allowDisplay alias ID.
        /// </summary>
        public long AllowDisplay => GetId( ref _allowDisplay, "core", "allowDisplay" );
        private long _allowDisplay;

        /// <summary>
        /// dbFieldTable alias ID.
        /// </summary>
        public long DbFieldTable => GetId( ref _dbFieldTable, "core", "dbFieldTable" );
        private long _dbFieldTable;

        /// <summary>
        /// dbType alias ID.
        /// </summary>
        public long DbType => GetId( ref _dbType, "core", "dbType" );
        private long _dbType;

        /// <summary>
        /// enumOrder alias ID.
        /// </summary>
        public long EnumOrder => GetId(ref _enumOrder, "core", "enumOrder");
        private long _enumOrder;

        /// <summary>
        /// everyoneRole alias ID.
        /// </summary>
        public long EveryoneRole => GetId( ref _everyoneRole, "core", "everyoneRole" );
        private long _everyoneRole;

        /// <summary>
        /// fieldCalculation alias ID.
        /// </summary>
        public long FieldCalculation => GetId(ref _fieldCalculation, "core", "fieldCalculation");
        private long _fieldCalculation;

        /// <summary>
        /// fieldIsOnType alias ID.
        /// </summary>
        public long FieldIsOnType => GetId( ref _fieldIsOnType, "core", "fieldIsOnType" );
        private long _fieldIsOnType;

        /// <summary>
        /// includesRoles alias ID.
        /// </summary>
        public long IncludesRoles => GetId( ref _includesRoles, "core", "includesRoles" );
        private long _includesRoles;

        /// <summary>
        /// includedByRoles alias ID.
        /// </summary>
        public long IncludedByRoles => GetId( ref _includedByRoles, "core", "includedByRoles" );
        private long _includedByRoles;

        /// <summary>
        /// inSolution alias ID.
        /// </summary>
        public long InSolution => GetId( ref _inSolution, "core", "inSolution" );
        private long _inSolution;

        /// <summary>
        /// isOfType alias ID.
        /// </summary>
        public long IsOfType => GetId( ref _isOfType, "core", "isOfType" );
        private long _isOfType;

        /// <summary>
        /// isPrivatelyOwned alias ID.
        /// </summary>
        public long IsPrivatelyOwned => GetId( ref _isPrivatelyOwned, "core", "isPrivatelyOwned" );
        private long _isPrivatelyOwned;

        /// <summary>
        /// manyToMany alias ID.
        /// </summary>
        public long ManyToMany => GetId( ref _manyToMany, "core", "manyToMany" );
        private long _manyToMany;

        /// <summary>
        /// manyToOne alias ID.
        /// </summary>
        public long ManyToOne => GetId( ref _manyToOne, "core", "manyToOne" );
        private long _manyToOne;

        /// <summary>
        /// name alias ID.
        /// </summary>
        public long Name => GetId( ref _name, "core", "name" );
        private long _name;

        /// <summary>
        /// oneToMany alias ID.
        /// </summary>
        public long OneToMany => GetId( ref _oneToMany, "core", "oneToMany" );
        private long _oneToMany;

        /// <summary>
        /// oneToOne alias ID.
        /// </summary>
        public long OneToOne => GetId( ref _oneToOne, "core", "oneToOne" );
        private long _oneToOne;

        /// <summary>
        /// createdDate alias ID.
        /// </summary>
        public long CreatedDate => GetId(ref _createdDate, "core", "createdDate");
        private long _createdDate;

        /// <summary>
        /// createdBy alias ID.
        /// </summary>
        public long CreatedBy => GetId(ref _createdBy, "core", "createdBy");
        private long _createdBy;

        /// <summary>
        /// field alias ID.
        /// </summary>
        public long Field => GetId( ref _field, "core", "field" );
        private long _field;

        /// <summary>
        /// modifiedDate alias ID.
        /// </summary>
        public long ModifiedDate => GetId(ref _modifiedDate, "core", "modifiedDate");
        private long _modifiedDate;

        /// <summary>
        /// lastModifiedBy alias ID.
        /// </summary>
        public long LastModifiedBy => GetId(ref _lastModifiedBy, "core", "lastModifiedBy");
        private long _lastModifiedBy;

        /// <summary>
        /// relationship alias ID.
        /// </summary>
        public long Relationship => GetId( ref _relationship, "core", "relationship" );
        private long _relationship;

        /// <summary>
        /// resource alias ID.
        /// </summary>
        public long Resource => GetId( ref _resource, "core", "resource" );
        private long _resource;

        /// <summary>
        /// resourceKeyDataHashAppliesToResourceKey alias ID.
        /// </summary>
        public long ResourceKeyDataHashAppliesToResourceKey => GetId( ref _resourceKeyDataHashAppliesToResourceKey, "core", "resourceKeyDataHashAppliesToResourceKey" );
        private long _resourceKeyDataHashAppliesToResourceKey;

        /// <summary>
        /// reverseAlias alias ID.
        /// </summary>
        public long ReverseAlias => GetId(ref _reverseAlias, "core", "reverseAlias");
        private long _reverseAlias;

        /// <summary>
        /// roleMembers alias ID.
        /// </summary>
        public long RoleMembers => GetId( ref _roleMembers, "core", "roleMembers" );
        private long _roleMembers;

        /// <summary>
        /// securesFrom alias ID.
        /// </summary>
        public long SecuresFrom => GetId( ref _securesFrom, "core", "securesFrom" );
        private long _securesFrom;

        /// <summary>
        /// securesFromReadOnly alias ID.
        /// </summary>
        public long SecuresFromReadOnly => GetId( ref _securesFromReadOnly, "core", "securesFromReadOnly" );
        private long _securesFromReadOnly;

        /// <summary>
        /// securesTo alias ID.
        /// </summary>
        public long SecuresTo => GetId( ref _securesTo, "core", "securesTo" );
        private long _securesTo;

        /// <summary>
        /// securesToReadOnly alias ID.
        /// </summary>
        public long SecuresToReadOnly => GetId( ref _securesToReadOnly, "core", "securesToReadOnly" );
        private long _securesToReadOnly;

        /// <summary>
        /// securityOwner alias ID.
        /// </summary>
        public long SecurityOwner => GetId( ref _securityOwner, "core", "securityOwner" );
        private long _securityOwner;

        /// <summary>
        /// solution alias ID.
        /// </summary>
        public long Solution => GetId( ref _solution, "core", "solution" );
        private long _solution;

        /// <summary>
        /// timeField alias ID.
        /// </summary>
        public long TimeField => GetId( ref _timeField, "core", "timeField" );
        private long _timeField;

        /// <summary>
        /// topMenu alias ID.
        /// </summary>
        public long TopMenu => GetId( ref _topMenu, "console", "topMenu" );
        private long _topMenu;

        /// <summary>
        /// type alias ID.
        /// </summary>
        public long Type => GetId( ref _type, "core", "type" );
        private long _type;

        /// <summary>
        /// userHasRole alias ID.
        /// </summary>
        public long UserHasRole => GetId( ref _userHasRole, "core", "userHasRole" );
        private long _userHasRole;

        /// <summary>
        /// workflowBeingRun alias ID.
        /// </summary>
        public long WorkflowBeingRun => GetId( ref _workflowBeingRun, "core", "workflowBeingRun" );
        private long _workflowBeingRun;


        /// <summary>
        /// isMetadata alias ID.
        /// </summary>
        public long IsMetadata => GetId(ref _isMetedata, "core", "isMetadata");
        private long _isMetedata;


        /// <summary>
        /// featureSwitch alias ID.
        /// </summary>
        public long FeatureSwitch => GetId(ref _featureSwitch, "core", "featureSwitch");
        private long _featureSwitch;

        /// <summary>
        /// featureSwitchType alias ID.
        /// </summary>
        public long FeatureSwitchType => GetId(ref _featureSwitchType, "core", "featureSwitchType");
        private long _featureSwitchType;

        /// <summary>
        /// lastLogon alias ID.
        /// </summary>
        public long LastLogon => GetId( ref _lastLogon, "core", "lastLogon" );
        private long _lastLogon;

        /// <summary>
        /// inherits alias ID.
        /// </summary>
        public long Inherits => GetId( ref _inherits, "core", "inherits" );
	    private long _inherits;

        /// <summary>
        /// readiNowIdentityProviderInstance alias ID.
        /// </summary>
        public long ReadiNowIdentityProviderInstance => GetId(ref _readiNowIdentityProviderInstance, "core", "readiNowIdentityProviderInstance");
        private long _readiNowIdentityProviderInstance;

        /// <summary>
        /// active alias ID.
        /// </summary>
        public long ActiveAccountStatus => GetId(ref _activeAccountStatus, "core", "active");
        private long _activeAccountStatus;

        /// <summary>
        /// Administrator role ID.
        /// </summary>
        public long AdministratorRole => GetId(ref _administratorRole, "core", "administratorRole");
        private long _administratorRole;

        /// <summary>
        /// Visibility calculation ID.
        /// </summary>
        public long VisibilityCalculation => GetId(ref _visibilityCalculation, "console", "visibilityCalculation");
        private long _visibilityCalculation;

        /// <summary>
        /// Control on form ID.
        /// </summary>
        public long ControlOnForm => GetId(ref _controlOnForm, "console", "controlOnForm");
        private long _controlOnForm;

        /// <summary>
        /// Type to edit with form ID.
        /// </summary>
        public long TypeToEditWithForm => GetId(ref _typeToEditWithForm, "console", "typeToEditWithForm");
        private long _typeToEditWithForm;

        /// <summary>
        /// The general settings for the tenant
        /// </summary>
        public long TenantGeneralSettingsInstance
        {
            get
            {
                return GetId(ref _tenantGeneralSettingsInstance, "core", "tenantGeneralSettingsInstance");
            }
        }

        private long _tenantGeneralSettingsInstance;
        
    }
}
