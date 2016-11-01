// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.ComponentModel;

namespace TenantDiffTool.SupportClasses.Diff
{
	/// <summary>
	///     Relationship.
	/// </summary>
	public class Relationship : DiffBase
	{
		/// <summary>
		///     The From name
		/// </summary>
		private string _fromName;

		/// <summary>
		///     The To name
		/// </summary>
		private string _toName;

		/// <summary>
		///     The Type name
		/// </summary>
		private string _typeName;

		/// <summary>
		///     Gets or sets from name.
		/// </summary>
		/// <value>
		///     From name.
		/// </value>
		[Browsable( true )]
		[DisplayName( @"From Name" )]
		[Category( "Properties" )]
		public string FromName
		{
			get
			{
				if ( string.IsNullOrEmpty( _fromName ) )
				{
					string name;

					if ( UpgradeIdCache.Instance.TryGetValue( FromUpgradeId, out name ) )
					{
						_fromName = name;
					}
				}

				return _fromName;
			}
			set
			{
				_fromName = value;
			}
		}

		/// <summary>
		///     Gets or sets from upgrade id.
		/// </summary>
		/// <value>
		///     From upgrade id.
		/// </value>
		[Browsable( true )]
		[DisplayName( @"From Upgrade Id" )]
		[Category( "Properties" )]
		public Guid FromUpgradeId
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets to name.
		/// </summary>
		/// <value>
		///     To name.
		/// </value>
		[Browsable( true )]
		[DisplayName( @"To Name" )]
		[Category( "Properties" )]
		public string ToName
		{
			get
			{
				if ( string.IsNullOrEmpty( _toName ) )
				{
					string name;

					if ( UpgradeIdCache.Instance.TryGetValue( ToUpgradeId, out name ) )
					{
						_toName = name;
					}
				}

				return _toName;
			}
			set
			{
				_toName = value;
			}
		}

		/// <summary>
		///     Gets or sets to upgrade id.
		/// </summary>
		/// <value>
		///     To upgrade id.
		/// </value>
		[Browsable( true )]
		[DisplayName( @"To Upgrade Id" )]
		[Category( "Properties" )]
		public Guid ToUpgradeId
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the name of the type.
		/// </summary>
		/// <value>
		///     The name of the type.
		/// </value>
		[Browsable( true )]
		[DisplayName( @"Type Name" )]
		[Category( "Properties" )]
		public string TypeName
		{
			get
			{
				if ( string.IsNullOrEmpty( _typeName ) )
				{
					string name;

					if ( UpgradeIdCache.Instance.TryGetValue( TypeUpgradeId, out name ) )
					{
						_typeName = name;
					}
				}

				return _typeName;
			}
			set
			{
				_typeName = value;
			}
		}

		/// <summary>
		///     Gets or sets the type upgrade id.
		/// </summary>
		/// <value>
		///     The type upgrade id.
		/// </value>
		[Browsable( true )]
		[DisplayName( @"Type Upgrade Id" )]
		[Category( "Properties" )]
		public Guid TypeUpgradeId
		{
			get;
			set;
		}
	}
}