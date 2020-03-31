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
using MDP_PPG.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MDP_PPG.EntitiesEditing
{
	/// <summary>
	/// Логика взаимодействия для W_EditRecordings.xaml
	/// </summary>
	public partial class W_EditRecordings : Window, INotifyPropertyChanged
	{
		public W_EditRecordings(int recordingId, int patientId)
		{
			InitializeComponent();

			MyContext = new PPG_Context();

			DataContext = this;

			LoadDataFromDB(recordingId, patientId);
		}


		//---------------------------- GUI Props ----------------------------------------
		public SignalFileReader F_SignalReader
		{
			get => f_SignalReader;
			set
			{
				f_SignalReader = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(F_SignalReader)));
			}
		}
		public SignalPortReader MC_SignalReader
		{
			get => mC_SignalReader;
			set
			{
				mC_SignalReader = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MC_SignalReader)));
			}
		}
		public RecordingTVM InstanceVM
		{
			get => instanceVM;
			set
			{
				instanceVM = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(InstanceVM)));
			}
		}

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
			Instance.RecordingDateTime = ParsedDateTime.Value;
			try { MyContext.SaveChanges(); }
			catch (Exception ex) { MessageBox.Show(ex.Message, ex.Source); }
			finally { MyContext.Dispose(); }
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

			Instance.RecordingDateTime = ParsedDateTime.Value;

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

		private async void LoadDataFromDB(int recordingId, int patientId)
		{
			IsLoadingData = true;

			if (recordingId != 0)
			{
				Instance = await MyContext.Recordings
					.Include(r => r.SignalDatas.Select(sd => sd.SignalChannel))
					.FirstOrDefaultAsync(r => r.Id == recordingId);
			}
			else
			{
				Instance = new Recording(patientId);
				MyContext.Recordings.Add(Instance);
			}

			if (Instance == null)
			{
				var mbRes = MessageBox.Show("Запись не найдена в БД, создать новую?", "Ошибка", MessageBoxButton.OKCancel);
				if (mbRes != MessageBoxResult.OK)
				{
					Dispatcher.Invoke(delegate
					{
						DialogResult = false;
					});
					return;
				}

				Instance = new Recording(patientId);
				MyContext.Recordings.Add(Instance);
			}

			InstanceVM = new RecordingTVM(Instance);
			string recordingDateTimeStr =
				Instance.Id == 0 ?
				DateTime.Now.ToString(Recording.DATE_TIME_FORMAT) :
				Instance.RecordingDateTime.ToString(Recording.DATE_TIME_FORMAT);
			Dispatcher.Invoke(delegate
			{
				vtbRecordingDateTime.SetValues(recordingDateTimeStr, ValidateRecordingDateTime, s => s, SetBtnOkEnabled);
			});

			SignalChannels = await MyContext.SignalChannels.ToListAsync();

			F_SignalReader = new SignalFileReader(
				(s, b) => { FileMessage = s; DataFileIsLoaded = b; },
				CheckSignalData,
				arg => IsLoadingData = arg,
				SignalChannels,
				Instance);
			MC_SignalReader = new SignalPortReader(
				(s, b) => { PortMessage = s; DataFileIsLoaded = b; },
				CheckSignalData,
				arg => IsLoadingData = arg,
				SignalChannels,
				Instance);

			DataFileIsLoaded = Instance.SignalDatas.Count > 0;

			IsLoadingData = false;
		}

		private PPG_Context MyContext;

		private void NotifyGettingSignalStatus(string msg, bool isSuccess)
		{
			FileMessage = msg;
			DataFileIsLoaded = isSuccess;
			InstanceVM.UpdateViewOnModelUpdated(Instance);
		}
		private void CheckSignalData()
		{
			if (Instance.SignalDatas.Any(sd => sd.Data.Length <= 0))
			{
				MessageBox.Show("Сигнал содержит нуль отсчетов, такой сигнал невозможно привязать к пациенту", "Сигнал пуст", MessageBoxButton.OK, MessageBoxImage.Error);
				NotifyGettingSignalStatus("Данный сигнал пуст, попробуйте загрузить другой сигнал", false);
				return;
			}
		}


		private Recording Instance { get; set; }
		private List<SignalChannel> SignalChannels;

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
		private string fileMessage = string.Empty;
		private string portMessage = string.Empty;
		private bool isLoadingData = true;
		private RecordingTVM instanceVM;
		private SignalFileReader f_SignalReader;
		private SignalPortReader mC_SignalReader;
	}
}
