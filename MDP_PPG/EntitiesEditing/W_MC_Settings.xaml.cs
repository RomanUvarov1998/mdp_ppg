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
using System.Windows.Shapes;
using PPG_Database.KeepingModels;
using PPG_Database;
using System.ComponentModel;

namespace MDP_PPG.EntitiesEditing
{
	/// <summary>
	/// Логика взаимодействия для W_MC_Settings.xaml
	/// </summary>
	public partial class W_MC_Settings : Window, INotifyPropertyChanged
	{
		public W_MC_Settings()
		{
			InitializeComponent();

			DataContext = this;

			ChannelsList = new List<SignalChannel>()
			{
				new SignalChannel("ЭКГ", 0) { IsInUse = true },
				new SignalChannel("ФПГ ИК", 1) { IsInUse = true },
				new SignalChannel("ФПГ К", 2) { IsInUse = true },
			};
		}

		public List<SignalChannel> ChannelsList
		{
			get => channelsList;
			set
			{
				channelsList = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ChannelsList)));
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		private List<SignalChannel> channelsList;

		private void Btn_Cansel_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = false;
		}

		private void Btn_Save_Click(object sender, RoutedEventArgs e)
		{
			//using ()
			DialogResult = true;
		}
	}
}
