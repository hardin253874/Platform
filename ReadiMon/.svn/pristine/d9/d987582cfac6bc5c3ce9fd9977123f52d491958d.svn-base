// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using ReadiMon.Shared.Controls.TreeListView;
using ReadiMon.Shared.Core;

namespace ReadiMon.Plugin.Redis.Profiling
{
	/// <summary>
	///     ProfilerTrace class.
	/// </summary>
	/// <seealso cref="ViewModelBase" />
	/// <seealso cref="IExpandable" />
	public class ProfilerTrace : ViewModelBase, IExpandable
	{
		private string _durationMilliseconds;

		private string _fullName;
		private Guid _id;

		private string _image;
		private bool _isLeaf;
		private string _name;
		private string _startMilliseconds;

		/// <summary>
		///     Initializes a new instance of the <see cref="ProfilerTrace" /> class.
		/// </summary>
		/// <param name="properties">The properties.</param>
		/// <param name="image">The image.</param>
		public ProfilerTrace( IDictionary<string, object> properties, string image = "" )
		{
			object value;

			if ( properties.TryGetValue( "Id", out value ) && value != null )
			{
				Id = new Guid( value.ToString( ) );
			}

			if ( properties.TryGetValue( "Name", out value ) && value != null )
			{
				var name = value.ToString( );

				int splitPosition = name.IndexOf( " - ", StringComparison.Ordinal );

				if ( splitPosition >= 0 )
				{
					Name = name.Substring( 0, splitPosition );
				}
				else
				{
					Name = name;
				}

				FullName = name;
			}

			if ( properties.TryGetValue( "DurationMilliseconds", out value ) && value != null )
			{
				DurationMilliseconds = value.ToString( );
			}

			if ( properties.TryGetValue( "StartMilliseconds", out value ) && value != null )
			{
				StartMilliseconds = value.ToString( );
			}

			if ( !string.IsNullOrEmpty( image ) )
			{
				Image = image;
			}
			else
			{
				if ( Name.StartsWith( "http" ) )
				{
					Image = "web.png";
				}
				else
				{
					Image = "call.png";
				}
			}
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="ProfilerTrace" /> class.
		/// </summary>
		protected ProfilerTrace( )
		{
		}

		/// <summary>
		///     Gets or sets the duration milliseconds.
		/// </summary>
		/// <value>
		///     The duration milliseconds.
		/// </value>
		public string DurationMilliseconds
		{
			get
			{
				return _durationMilliseconds;
			}
			set
			{
				SetProperty( ref _durationMilliseconds, value );
			}
		}

		/// <summary>
		///     Gets or sets the full name.
		/// </summary>
		/// <value>
		///     The full name.
		/// </value>
		public string FullName
		{
			get
			{
				return _fullName;
			}
			set
			{
				SetProperty( ref _fullName, value );
			}
		}

		/// <summary>
		///     Gets or sets the identifier.
		/// </summary>
		/// <value>
		///     The identifier.
		/// </value>
		public Guid Id
		{
			get
			{
				return _id;
			}
			set
			{
				SetProperty( ref _id, value );
			}
		}

		/// <summary>
		///     Gets or sets the image.
		/// </summary>
		/// <value>
		///     The image.
		/// </value>
		public string Image
		{
			get
			{
				return _image;
			}
			set
			{
				SetProperty( ref _image, value );
			}
		}

		/// <summary>
		///     Gets or sets a value indicating whether this instance is leaf.
		/// </summary>
		/// <value>
		///     <c>true</c> if this instance is leaf; otherwise, <c>false</c>.
		/// </value>
		public bool IsLeaf
		{
			get
			{
				return _isLeaf;
			}
			set
			{
				SetProperty( ref _isLeaf, value );
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
		///     Gets or sets the start milliseconds.
		/// </summary>
		/// <value>
		///     The start milliseconds.
		/// </value>
		public string StartMilliseconds
		{
			get
			{
				return _startMilliseconds;
			}
			set
			{
				SetProperty( ref _startMilliseconds, value );
			}
		}

		/// <summary>
		///     Gets the tooltip.
		/// </summary>
		/// <value>
		///     The tooltip.
		/// </value>
		public virtual string Tooltip
		{
			get
			{
				return FullName;
			}
		}

		/// <summary>
		///     Called when collapsing.
		/// </summary>
		public void OnCollapse( )
		{
		}

		/// <summary>
		///     Called when expanding.
		/// </summary>
		public void OnExpand( )
		{
		}
	}
}