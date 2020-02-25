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
using System.Windows.Controls.Primitives;
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

			SampleWidthStr = "10";
			Y_ScaleStr = "1";
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
				svX.ScrollToLeftEnd();
				svY.ScrollToBottom();
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
				Dispatcher.Invoke(delegate { Plot.SetData(sd, SampleWidthGlobal, Y_ScaleGlobal, 0.0, svPlot.ActualWidth); });
			}

			IsLoadingData = false;
		}

		public void OnWindowResized()
		{
			TryUpdatePlot();
		}
		private void TryUpdatePlot()
		{
			if (Plot != null)
			{
				double leftBorder = svPlot.HorizontalOffset;
				double rightBorder = svPlot.HorizontalOffset + svPlot.ActualWidth;

				svY.ScrollChanged -= svY_ScrollChanged;
				svX.ScrollChanged -= svX_ScrollChanged;

				Plot.UpdatePlot(leftBorder, rightBorder);
				var w = plotGraph.ActualWidth;

				svY.ScrollChanged += svY_ScrollChanged;
				svX.ScrollChanged += svX_ScrollChanged;
			}
		}

		private void Sv_Plot_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
		{
			if (Plot == null) return;

			if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
			{
				Plot.Change_XY_Scale(e.Delta);

				//Y_ScaleStr = Plot.Y_Scale.ToString();
				//SampleWidthStr = Plot.SampleWidth.ToString();
			}
			else if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
			{
				if (e.Delta > 0)
					svX.LineLeft();
				else
					svX.LineRight();

				e.Handled = true;
			}
			else
			{
				if (e.Delta > 0)
					svY.LineUp();
				else
					svY.LineDown();

				e.Handled = true;
			}
		}
		private void svY_ScrollChanged(object sender, ScrollChangedEventArgs e)
		{
			svPlot.ScrollToVerticalOffset(e.VerticalOffset);
		}
		private void svX_ScrollChanged(object sender, ScrollChangedEventArgs e)
		{
			TryUpdatePlot();
			svPlot.ScrollToHorizontalOffset(e.HorizontalOffset);
		}

		public string Y_ScaleStr
		{
			get => y_ScaleStr;
			set
			{
				y_ScaleStr = value;

				double v = -1.0;
				if (double.TryParse(value, out v) && v > 0)
				{
					Y_ScaleGlobal = v;
					if (Plot != null)
						Plot.SetYScale(Y_ScaleGlobal);
				}

				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Y_ScaleStr)));
			}
		}
		public string SampleWidthStr
		{
			get => sampleWidthStr;
			set
			{
				sampleWidthStr = value;

				double v = -1.0;
				if (double.TryParse(value, out v) && v > 0)
				{
					SampleWidthGlobal = v;
					if (Plot != null)
						Plot.SetXScale(SampleWidthGlobal);
				}

				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SampleWidthStr)));
			}
		}

		public string MousePos
		{
			get => mousePos;
			set
			{
				mousePos = value;
				PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(MousePos)));
			}
		}
		private void Plot_MouseMove(object sender, MouseEventArgs e)
		{
			if (Plot == null) return;

			var p = e.GetPosition(plotGrid);

			MousePos = Plot.OnMouseMove(p);
		}
		private void Plot_MouseLeave(object sender, MouseEventArgs e)
		{
			MousePos = string.Empty;
		}


		public event PropertyChangedEventHandler PropertyChanged;


		private bool isLoadingData;
		private SignalDataGV plot;
		private Recording recording;
		private string sampleWidthStr;
		private string y_ScaleStr;
		private double SampleWidthGlobal;
		private double Y_ScaleGlobal;
		private string mousePos;
	}
}
