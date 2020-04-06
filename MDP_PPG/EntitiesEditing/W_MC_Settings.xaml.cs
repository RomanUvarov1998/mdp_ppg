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
using System.Data.Entity;
using MDP_PPG.EntitiesEditing;

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

			_myContext = new PPG_Context();

			LoadDataFromDB();
		}

		//------------------------------- GUI ------------------------------------
		public bool IsLoadingData
		{
			get => _isLoadingData;
			set
			{
				_isLoadingData = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsLoadingData)));
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsGUIEnabled)));
			}
		}
		public bool IsGUIEnabled => !IsLoadingData;
		public List<SignalChannel> ChannelsList
		{
			get => _channelsList;
			set
			{
				_channelsList = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ChannelsList)));
			}
		}
		public string StateMessage
		{
			get => _stateMessage;
			set
			{
				_stateMessage = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StateMessage)));
			}
		}
		public SerialPortConnector SerialPortConnector
		{
			get => signalPortConnector;
			set
			{
				signalPortConnector = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SerialPortConnector)));
			}
		}

		//------------------------------- Event handlers -------------------------
		private void Btn_Cansel_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = false;
		}
		private async void Btn_Save_Click(object sender, RoutedEventArgs e)
		{
			bool res = await SaveToDB();

			if (!res) return;

			SaveToMC();
		}
		private void Btn_refreshPortsList_Click(object sender, RoutedEventArgs e)
		{
			SerialPortConnector.RefreshPortsList();
		}
		private void Window_Closing(object sender, CancelEventArgs e)
		{
			_myContext.Dispose();
		}

		//------------------------------- Other -------------------------

		private async void LoadDataFromDB()
		{
			IsLoadingData = true;

			ChannelsList = await _myContext.SignalChannels.ToListAsync();

			SerialPortConnector = new SerialPortConnector(
				(s, b) => { StateMessage = s; _savedToMC = b; },
				arg => IsLoadingData = arg);
			SerialPortConnector.RefreshPortsList();
			SerialPortConnector.SelectedPort = SerialPortConnector.AvailablePorts.FirstOrDefault();

			IsLoadingData = false;
		}
		private async Task<bool> SaveToDB()
		{
			IsLoadingData = true;

			bool res;
			StateMessage = "Сохранение настроек на ПК...";
			try
			{
				await _myContext.SaveChangesAsync();
				StateMessage = "Настройки сохранены на ПК";
				res = true;
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, ex.Source);
				res = false;
			}
			finally
			{
				IsLoadingData = false;
			}

			return res;
		}
		private void SaveToMC()
		{
			StateMessage = "Сохранение настроек на МК...";
			SerialPortConnector.SaveSettings(ChannelsList);
		}

		public event PropertyChangedEventHandler PropertyChanged;

		private List<SignalChannel> _channelsList;
		private PPG_Context _myContext;
		private bool _isLoadingData;
		private string _stateMessage;
		private bool _savedToMC = false;
		private SerialPortConnector signalPortConnector;
	}
}
