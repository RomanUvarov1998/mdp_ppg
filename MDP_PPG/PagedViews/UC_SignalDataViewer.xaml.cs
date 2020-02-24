using MDP_PPG.ViewModels;
using PPG_Database;
using PPG_Database.KeepingModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
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

namespace MDP_PPG.PagedViews
{
	/// <summary>
	/// Логика взаимодействия для UC_SignalDataViewer.xaml
	/// </summary>
	public partial class UC_SignalDataViewer : UserControl, INotifyPropertyChanged
	{
		public UC_SignalDataViewer()
		{
			InitializeComponent();

			DataContext = this;
		}

		public bool IsLoadingData
		{
			get => isLoadingData;
			set
			{
				isLoadingData = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsLoadingData)));
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsInterfaceEnabled)));
			}
		}
		public bool IsInterfaceEnabled => !isLoadingData;

		public SignalDataGV Plot
		{
			get => plot;
			set
			{
				plot = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Plot)));
			}
		}
		public Recording Recording
		{
			get => recording;
			set
			{
				recording = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Recording)));
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RecordingIsNotNull)));
			}
		}
		public bool RecordingIsNotNull => Recording != null;

		public void Freeze()
		{
			IsLoadingData = true;
		}

		public async Task LoadData(ModelBase recording)
		{
			Recording = (Recording)recording ?? null;

			if (Recording == null)
			{
				Plot = null;
				IsLoadingData = false;
				return;
			}

			SignalData sd;

			using (var context = new PPG_Context())
			{
				sd = await context.SignalDatas.FirstOrDefaultAsync(d => d.RecordingId == recording.Id);
			}

			if (sd != null)
			{
				Plot = new SignalDataGV();
				Plot.SetData(sd, sampleWidth, y_Scale);
			}

			IsLoadingData = false;
		}


		private void ScrollViewer_MouseWheel(object sender, MouseWheelEventArgs e)
		{
			ScrollViewer sv = sender as ScrollViewer;
			if (sv == null) return;

			if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
			{
				Plot.Change_XY_Scale(e.Delta);
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Y_Scale)));
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SampleWidth)));
			}
			else if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
			{
				if (e.Delta > 0)
					sv.LineLeft();
				else
					sv.LineRight();

				e.Handled = true;
			}
		}

		public string Y_Scale
		{
			get => y_Scale.ToString();
			set
			{
				double v = -1.0;
				if (double.TryParse(value, out v) && v > 0)
				{
					y_Scale = v;
					if (Plot != null)
						Plot.Y_Scale = v;
				}

				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Y_Scale)));
			}
		}
		public string SampleWidth
		{
			get => sampleWidth.ToString();
			set
			{
				double v = -1.0;
				if (double.TryParse(value, out v) && v > 0)
				{
					sampleWidth = v;
					if (Plot != null)
						Plot.SampleWidth = v;
				}

				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SampleWidth)));
			}
		}


		public event PropertyChangedEventHandler PropertyChanged;


		private bool isLoadingData;
		private SignalDataGV plot;
		private Recording recording;
		private double sampleWidth = 10;
		private double y_Scale = 1;
	}
}
