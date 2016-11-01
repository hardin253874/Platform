// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Resources;

namespace EDC.ReadiNow.Metadata.Tenants
{
	/// <summary>
	///     Represents information about registered tenants.
	/// </summary>
	[Serializable]
	public class TenantInfo
	{
	    private bool _haveName = false;
        private bool _haveDescription = false;
        private string _name = null;
	    private string _description = null;

		/// <summary>
		///     Initializes a new instance of the TenantInfo class.
		/// </summary>
		private TenantInfo( )
		{
		}

		/// <summary>
		///     Initializes a new instance of the TenantInfo class.
		/// </summary>
		public TenantInfo( long id )
			: this( )
		{
			Id = id;
        }



		/// <summary>
		///     Gets the ID associated with the tenant data.
		/// </summary>
		public long Id
		{
			get;
			set;
		}

		/// <summary>
		///     Gets the name associated with the tenant data.
		/// </summary>
		public string Name
		{
			get
			{
                if (!_haveName) 
                    GetNameDescription();

			    return _name;
			}
		    set
		    {
		        if (!_haveName && !string.IsNullOrWhiteSpace(value))
		        {
		            _name = value;
		            _haveName = true;
		        }
		    }
		}

        /// <summary>
        ///     Gets the description associated with the tenant data.
        /// </summary>
        public string Description
        {
            get
            {
                if (!_haveDescription)
                    GetNameDescription();

                return _description;
            }
        }

        /// <summary>
        ///     Have we got a name.
        /// </summary>
        public bool HaveName
        {
            get { return _haveName; }
        }

        void GetNameDescription()
        {
            if (Id == 0)
            {
                _name = SpecialStrings.GlobalTenant;
                _description = string.Empty;

                _haveName = true;
                _haveDescription = true;
            }
            else
            {
                using (new AdministratorContext())
                {
                    var tenant = Entity.Get<Tenant>(Id, Tenant.Name_Field, Tenant.Description_Field);
                    if (tenant != null)
                    {
                        _name = tenant.Name;
                        _description = tenant.Description;
                        
                    }
                    else
                    {
                        _name = string.Empty;
                        _description = string.Empty;
                    }

                    _haveName = true;
                    _haveDescription = true;
                }
            }
        }
	}
}