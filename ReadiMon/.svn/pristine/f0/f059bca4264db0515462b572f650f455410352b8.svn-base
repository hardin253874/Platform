// Copyright 2011-2014 Global Software Innovation Pty Ltd

using System.AddIn;
using System.Windows;
using System.Windows.Controls;
using ReadiMon.AddinView;

namespace ReadiMon.Plugin.Entity
{
	/// <summary>
	///     Dummy Plugin
	/// </summary>
	public class DummyPlugin : PluginBase
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="DummyPlugin" /> class.
		/// </summary>
		public DummyPlugin( )
		{
			SectionName = "Entity";
			EntryName = GetName( );
			HasOptionsUserInterface = true;
			HasUserInterface = true;
		}

		/// <summary>
		///     Gets the name.
		/// </summary>
		/// <returns></returns>
		private string GetName( )
		{
			object[ ] customAttributes = GetType( ).GetCustomAttributes( typeof ( AddInAttribute ), true );

			if ( customAttributes.Length > 0 )
			{
				var addin = customAttributes[ 0 ] as AddInAttribute;

				if ( addin != null )
				{
					return addin.Name;
				}
			}

			return GetType( ).Name;
		}

		/// <summary>
		///     Gets the options user interface.
		/// </summary>
		/// <returns></returns>
		public override FrameworkElement GetOptionsUserInterface( )
		{
			var button = new Button
			{
				Content = EntryName,
				Height = 100,
				Width = 100
			};

			return button;
		}

		/// <summary>
		///     Gets the user interface.
		/// </summary>
		/// <returns></returns>
		public override FrameworkElement GetUserInterface( )
		{
			var button = new Button
			{
				Content = EntryName,
				Height = 100,
				Width = 100
			};

			return button;
		}
	}

	/*
	[AddIn( "Entity Plugin 19", Version = "1.0.0.0" )]
	public class EntityPlugin19 : DummyPlugin, IPlugin
	{
	}

	[AddIn( "Entity Plugin 18", Version = "1.0.0.0" )]
	public class EntityPlugin18 : DummyPlugin, IPlugin
	{
	}

	[AddIn( "Entity Plugin 17", Version = "1.0.0.0" )]
	public class EntityPlugin17 : DummyPlugin, IPlugin
	{
	}

	[AddIn( "Entity Plugin 16", Version = "1.0.0.0" )]
	public class EntityPlugin16 : DummyPlugin, IPlugin
	{
	}

	[AddIn( "Entity Plugin 15", Version = "1.0.0.0" )]
	public class EntityPlugin15 : DummyPlugin, IPlugin
	{
	}

	[AddIn( "Entity Plugin 14", Version = "1.0.0.0" )]
	public class EntityPlugin14 : DummyPlugin, IPlugin
	{
	}

	[AddIn( "Entity Plugin 13", Version = "1.0.0.0" )]
	public class EntityPlugin13 : DummyPlugin, IPlugin
	{
	}

	[AddIn( "Entity Plugin 12", Version = "1.0.0.0" )]
	public class EntityPlugin12 : DummyPlugin, IPlugin
	{
	}

	[AddIn( "Entity Plugin 11", Version = "1.0.0.0" )]
	public class EntityPlugin11 : DummyPlugin, IPlugin
	{
	}

	[AddIn( "Entity Plugin 10", Version = "1.0.0.0" )]
	public class EntityPlugin10 : DummyPlugin, IPlugin
	{
	}

	[AddIn( "Entity Plugin 9", Version = "1.0.0.0" )]
	public class EntityPlugin9 : DummyPlugin, IPlugin
	{
	}

	[AddIn( "Entity Plugin 8", Version = "1.0.0.0" )]
	public class EntityPlugin8 : DummyPlugin, IPlugin
	{
	}

	[AddIn( "Entity Plugin 7", Version = "1.0.0.0" )]
	public class EntityPlugin7 : DummyPlugin, IPlugin
	{
	}

	[AddIn( "Entity Plugin 6", Version = "1.0.0.0" )]
	public class EntityPlugin6 : DummyPlugin, IPlugin
	{
	}

	[AddIn( "Entity Plugin 5", Version = "1.0.0.0" )]
	public class EntityPlugin5 : DummyPlugin, IPlugin
	{
	}

	[AddIn( "Entity Plugin 4", Version = "1.0.0.0" )]
	public class EntityPlugin4 : DummyPlugin, IPlugin
	{
	}

	[AddIn( "Entity Plugin 3", Version = "1.0.0.0" )]
	public class EntityPlugin3 : DummyPlugin, IPlugin
	{
	}

	[AddIn( "Entity Plugin 2", Version = "1.0.0.0" )]
	public class EntityPlugin2 : DummyPlugin, IPlugin
	{
	}
	*/
}