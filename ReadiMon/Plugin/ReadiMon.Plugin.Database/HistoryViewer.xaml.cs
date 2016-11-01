using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ReadiMon.Shared;

namespace ReadiMon.Plugin.Database
{
	/// <summary>
	/// Interaction logic for HistoryViewer.xaml
	/// </summary>
	public partial class HistoryViewer : Window
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="HistoryViewer" /> class.
		/// </summary>
		/// <param name="transactionId">The transaction identifier.</param>
		/// <param name="settings">The settings.</param>
		public HistoryViewer( long transactionId, IPluginSettings settings )
		{
			InitializeComponent( );

			var viewModel = new HistoryViewerViewModel( transactionId, settings );
			DataContext = viewModel;
		}
	}
}
