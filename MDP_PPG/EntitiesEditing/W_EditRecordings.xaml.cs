using Microsoft.Win32;
using PPG_Database;
using PPG_Database.KeepingModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Globalization;
using System.IO;
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

			DataContext = this;

			CheckForExistingData();
		}

		private async void CheckForExistingData()
		{
			int c;

			using (var context = new PPG_Context())
			{
				c = await context.SignalDatas.CountAsync(d => d.RecordingId == Instance.Id);
			}

			if (c == 0)
			{
				DataLoadingMessage = "Нет загруженного файла записи";
				DataFileIsLoaded = false;
			}
			else
			{
				DataLoadingMessage = "Есть файл записи";
				DataFileIsLoaded = true;
			}

			IsLoadingData = false;
		}


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
		public string DataLoadingMessage
		{
			get => dataLoadingMessage;
			set
			{
				dataLoadingMessage = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DataLoadingMessage)));
			}
		}
		private void Btn_LoadRecording_Click(object sender, RoutedEventArgs e)
		{
			if (DataFileIsLoaded)
			{
				var res = MessageBox.Show("Файл записи уже загружен, загрузить другой?", "Файл записи", MessageBoxButton.YesNo, MessageBoxImage.Question);
				if (res == MessageBoxResult.No) return;
			}

			OpenFileDialog openFileDialog = new OpenFileDialog();

			string path;

			if (openFileDialog.ShowDialog() == true)
				path = openFileDialog.FileName;
			else return;

			byte[] data;

			try
			{
				data = File.ReadAllBytes(path);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, ex.Source);
				DataLoadingMessage = "Ошибка чтения файла";
				DataFileIsLoaded = false;
				return;
			}

			Instance.SignalData = new SignalData(data, Instance.Id);
			DataLoadingMessage = "Файл успешно заружен";
			DataFileIsLoaded = true;
		}


		public bool BtnOkActive => vtbRecordingDateTime.AllowsSavingInput &&
															 DataFileIsLoaded;
		private void SetBtnOkEnabled()
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BtnOkActive)));
		}


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


		public event PropertyChangedEventHandler PropertyChanged;


		private bool dataFileIsLoaded = false;
		private string dataLoadingMessage = "Проверка на существование файла записи...";
		private bool isLoadingData = true;
	}
}
