// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace TenantDiffTool.Core
{
	/// <summary>
	///     DataGrid row double click handler.
	/// </summary>
	public sealed class RowDoubleClickHandler : FrameworkElement
	{
		/// <summary>
		/// </summary>
		public static readonly DependencyProperty MethodNameProperty = DependencyProperty.RegisterAttached(
			"MethodName",
			typeof( string ),
			typeof( RowDoubleClickHandler ),
			new PropertyMetadata( ( o, e ) =>
			{
				var dataGrid = o as DataGrid;
				if ( dataGrid != null )
				{
// ReSharper disable ObjectCreationAsStatement
					new RowDoubleClickHandler( dataGrid );
// ReSharper restore ObjectCreationAsStatement
				}
			} ) );

		public RowDoubleClickHandler( DataGrid dataGrid )
		{
			MouseButtonEventHandler handler = ( sender, args ) =>
			{
				var row = sender as DataGridRow;
				if ( row != null && row.IsSelected )
				{
					string methodName = GetMethodName( dataGrid );

					Type dataContextType = dataGrid.DataContext.GetType( );
					MethodInfo method = dataContextType.GetMethod( methodName );
					if ( method == null )
					{
						throw new MissingMethodException( methodName );
					}

					method.Invoke( dataGrid.DataContext, null );
				}
			};

			dataGrid.LoadingRow += ( s, e ) =>
			{
				e.Row.MouseDoubleClick += handler;
			};

			dataGrid.UnloadingRow += ( s, e ) =>
			{
				e.Row.MouseDoubleClick -= handler;
			};
		}

		/// <summary>
		///     Gets the name of the method.
		/// </summary>
		/// <param name="dataGrid">The data grid.</param>
		/// <returns></returns>
		public static string GetMethodName( DataGrid dataGrid )
		{
			return ( string ) dataGrid.GetValue( MethodNameProperty );
		}

		/// <summary>
		///     Sets the name of the method.
		/// </summary>
		/// <param name="dataGrid">The data grid.</param>
		/// <param name="value">The value.</param>
		public static void SetMethodName( DataGrid dataGrid, string value )
		{
			dataGrid.SetValue( MethodNameProperty, value );
		}
	}
}