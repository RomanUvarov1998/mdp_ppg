using Microsoft.Win32;
using PPG_Database;
using PPG_Database.KeepingModels;
using System;
using System.ComponentModel;
using System.Data.Entity;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using MDP_PPG.Helpers;
using System.IO.Ports;

namespace MDP_PPG.EntitiesEditing
{
	/// <summary>
	/// Логика взаимодействия для W_EditRecordings.xaml
	/// </summary>
	public partial class W_EditRecordings : Window, INotifyPropertyChanged
	{
		public W_EditRecordings(Recording instance)
		{
			Instance = instance;

			InitializeComponent();

			string recordingDateTimeStr =
				instance.Id == 0 ?
				DateTime.Now.ToString(Recording.DATE_TIME_FORMAT) :
				instance.RecordingDateTime.ToString(Recording.DATE_TIME_FORMAT);

			vtbRecordingDateTime.SetValues(recordingDateTimeStr, ValidateRecordingDateTime, s => s, SetBtnOkEnabled);

			F_SignalReader = new SignalFileReader(
				(s, b) => { FileMessage = s; DataFileIsLoaded = b; }, CheckSignalData, arg => IsLoadingData = arg);
			MC_SignalReader = new SignalPortReader(
				(s, b) => { PortMessage = s; DataFileIsLoaded = b; }, CheckSignalData, arg => IsLoadingData = arg);

			DataContext = this;

			CheckForExistingData();
		}


		//---------------------------- GUI Props ----------------------------------------

		public string FileMessage
		{
			get => fileMessage;
			set
			{
				fileMessage = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FileMessage)));
			}
		}
		public string PortMessage
		{
			get => portMessage;
			set
			{
				portMessage = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PortMessage)));
			}
		}

		public bool BtnOkActive => vtbRecordingDateTime.AllowsSavingInput &&
															 DataFileIsLoaded;
		public bool IsLoadingData
		{
			get => isLoadingData;
			set
			{
				isLoadingData = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsLoadingData)));
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsGUIEnabled)));
			}
		}
		public bool IsGUIEnabled => !IsLoadingData;

		//---------------------------- GUI Event handlers -------------------------------

		private void Btn_LoadRecording_FromFile_Click(object sender, RoutedEventArgs e)
		{
			F_SignalReader.TryUploadSignal(Instance);
		}
		private void Btn_LoadRecording_FromMC_Click(object sender, RoutedEventArgs e)
		{
			if (MC_SignalReader.SelectedPort == null)
			{
				PortMessage = "Выберите порт для передачи сигнала с микроконтроллера";
				return;
			}

			if (!MC_SignalReader.CheckSelectedPortExists())
			{
				PortMessage = "Данный порт больше не существует, выберите другой порт";
				return;
			}

			MC_SignalReader.TryUploadSignal(Instance);
		}


		private void Btn_refreshPortsList_Click(object sender, RoutedEventArgs e)
		{
			MC_SignalReader.RefreshPortsList();
		}
		

		private void ButtonOk_OnClick(object _sender, RoutedEventArgs _e)
		{
			DialogResult = true;
		}
		private void ButtonCancel_OnClick(object _sender, RoutedEventArgs _e)
		{
			DialogResult = false;
		}
		private void Window_Closing(object sender, CancelEventArgs e)
		{
			MessageBoxResult res = MessageBoxResult.None;

			if (DialogResult == null && BtnOkActive)
				res = MessageBox.Show("Сохранить изменения?", "Сохранение изменений",
					MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

			F_SignalReader.TurnOff();
			MC_SignalReader.TurnOff();

			switch (res)
			{
				case MessageBoxResult.Yes:
					e.Cancel = true;
					ButtonOk_OnClick(null, null);
					break;
				case MessageBoxResult.Cancel:
					e.Cancel = true;
					return;
				case MessageBoxResult.No:
					DialogResult = false;
					return;
			}
		}

		//---------------------------- Other --------------------------------------------

		private async void CheckForExistingData()
		{
			int c;

			using (var context = new PPG_Context())
			{
				c = await context.SignalDatas.CountAsync(d => d.RecordingId == Instance.Id);
			}

			if (c == 0)
			{
				FileMessage = "Нет загруженного файла записи";
				PortMessage = "Нет загруженного файла записи";
				DataFileIsLoaded = false;
			}
			else
			{
				FileMessage = "Есть файл записи";
				PortMessage = "Есть файл записи";
				DataFileIsLoaded = true;
			}

			IsLoadingData = false;
		}

		private void NotifyGettingSignalStatus(string msg, bool isSuccess)
		{
			FileMessage = msg;
			DataFileIsLoaded = isSuccess;
		}
		private void CheckSignalData()
		{
			if (Instance.SignalData.Data.Length <= 0)
			{
				MessageBox.Show("Сигнал содержит нуль отсчетов, такой сигнал невозможно привязать к пациенту", "Сигнал пуст", MessageBoxButton.OK, MessageBoxImage.Error);
				NotifyGettingSignalStatus("Данный сигнал пуст, попробуйте загрузить другой сигнал", false);
				return;
			}
		}


		public SignalFileReader F_SignalReader { get; set; }
		public SignalPortReader MC_SignalReader { get; set; }


		private Recording Instance { get; set; }
		public Recording GetRecording()
		{
			Instance.RecordingDateTime = ParsedDateTime.Value;
			return Instance;
		}

		private bool ValidateRecordingDateTime(string val)
		{
			bool res = DateTime.TryParseExact(val, Recording.DATE_TIME_FORMAT, CultureInfo.CurrentCulture, DateTimeStyles.None, out DateTime dt);
			if (res) ParsedDateTime = dt;
			return res;
		}
		private DateTime? ParsedDateTime;

		private bool DataFileIsLoaded
		{
			get => dataFileIsLoaded;
			set
			{
				dataFileIsLoaded = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DataFileIsLoaded)));
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BtnOkActive)));
			}
		}

		private void SetBtnOkEnabled()
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BtnOkActive)));
		}


		public event PropertyChangedEventHandler PropertyChanged;


		private bool dataFileIsLoaded = false;
		private string fileMessage = "Проверка на существование файла записи...";
		private string portMessage = "Проверка на существование файла записи...";
		private bool isLoadingData = true;
	}
}
