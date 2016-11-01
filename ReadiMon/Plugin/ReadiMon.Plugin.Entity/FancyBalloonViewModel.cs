// Copyright 2011-2015 Global Software Innovation Pty Ltd

using System;
using System.Windows.Input;
using ReadiMon.Shared.Core;

namespace ReadiMon.Plugin.Entity
{
	/// <summary>
	///     FancyBallon view model.
	/// </summary>
	public class FancyBalloonViewModel : ViewModelBase
	{
		/// <summary>
		///     The balloon title
		/// </summary>
		private string _balloonTitle;

		/// <summary>
		///     The description.
		/// </summary>
		private string _description;

		/// <summary>
		///     The entity identifier
		/// </summary>
		private string _entityId;

		/// <summary>
		///     The name
		/// </summary>
		private string _name;

		/// <summary>
		///     The solution
		/// </summary>
		private string _solution;

		/// <summary>
		///     The tenant
		/// </summary>
		private string _tenant;

		/// <summary>
		///     The tenant identifier
		/// </summary>
		private string _tenantId;

		/// <summary>
		///     The type
		/// </summary>
		private string _type;

		/// <summary>
		///     The upgrade identifier
		/// </summary>
		private string _upgradeId;

		/// <summary>
		///     Initializes a new instance of the <see cref="FancyBalloonViewModel" /> class.
		/// </summary>
		public FancyBalloonViewModel( )
		{
			BalloonClick = new DelegateCommand( OnClick );
		}

		/// <summary>
		///     Gets or sets the balloon click.
		/// </summary>
		/// <value>
		///     The balloon click.
		/// </value>
		public ICommand BalloonClick
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets or sets the balloon title.
		/// </summary>
		/// <value>
		///     The balloon title.
		/// </value>
		public string BalloonTitle
		{
			get
			{
				return _balloonTitle;
			}
			set
			{
				SetProperty( ref _balloonTitle, value );
			}
		}

		/// <summary>
		///     Gets or sets the description.
		/// </summary>
		/// <value>
		///     The description.
		/// </value>
		public string Description
		{
			get
			{
				return _description;
			}
			set
			{
				SetProperty( ref _description, value );
			}
		}

		/// <summary>
		///     Gets or sets the entity identifier.
		/// </summary>
		/// <value>
		///     The entity identifier.
		/// </value>
		public string EntityId
		{
			get
			{
				return _entityId;
			}
			set
			{
				SetProperty( ref _entityId, value );
			}
		}

		/// <summary>
		///     Gets or sets the name.
		/// </summary>
		/// <value>
		///     The name.
		/// </value>
		public string Name
		{
			get
			{
				return _name;
			}
			set
			{
				SetProperty( ref _name, value );
			}
		}

		/// <summary>
		///     Gets or sets the solution.
		/// </summary>
		/// <value>
		///     The solution.
		/// </value>
		public string Solution
		{
			get
			{
				return _solution;
			}
			set
			{
				SetProperty( ref _solution, value );
			}
		}

		/// <summary>
		///     Gets or sets the tenant.
		/// </summary>
		/// <value>
		///     The tenant.
		/// </value>
		public string Tenant
		{
			get
			{
				return _tenant;
			}
			set
			{
				SetProperty( ref _tenant, value );
			}
		}

		/// <summary>
		///     Gets or sets the tenant identifier.
		/// </summary>
		/// <value>
		///     The tenant identifier.
		/// </value>
		public string TenantId
		{
			get
			{
				return _tenantId;
			}
			set
			{
				SetProperty( ref _tenantId, value );
			}
		}

		/// <summary>
		///     Gets or sets the type.
		/// </summary>
		/// <value>
		///     The type.
		/// </value>
		public string Type
		{
			get
			{
				return _type;
			}
			set
			{
				SetProperty( ref _type, value );
			}
		}

		/// <summary>
		///     Gets or sets the upgrade identifier.
		/// </summary>
		/// <value>
		///     The upgrade identifier.
		/// </value>
		public string UpgradeId
		{
			get
			{
				return _upgradeId;
			}
			set
			{
				SetProperty( ref _upgradeId, value );
			}
		}

		/// <summary>
		///     Called when [click].
		/// </summary>
		private void OnClick( )
		{
			var evt = ShowEntity;

			if ( evt != null )
			{
				evt( this, EntityId );
			}
		}

		/// <summary>
		///     Occurs when [show entity].
		/// </summary>
		public event EventHandler<string> ShowEntity;
	}
}