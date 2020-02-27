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

			CurrentScale = new Size(1000, 1);
			SampleWidthStr = "1000";
			Y_ScaleStr = "1";

			Min_X = 0;
			Max_X = 100;

			Min_Y = 0;
			Max_Y = 100;

			UpdatePlotClip();
		}


		//--------------------------------------- Gui props -------------------------------------------
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
		public RectangleGeometry PlotClip
		{
			get => plotClip;
			set
			{
				plotClip = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PlotClip)));
			}
		}
		public double Min_X
		{
			get => min_X;
			set
			{
				min_X = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Min_X)));
			}
		}
		public double Max_X
		{
			get => max_X;
			set
			{
				max_X = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Max_X)));
			}
		}
		public double ScrollValue_X
		{
			get => scrollValue_X;
			set
			{
				scrollValue_X = value;
				if (value < Min_X) scrollValue_X = Min_X;
				if (value > Max_X) scrollValue_X = Max_X;

				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ScrollValue_X)));
			}
		}
		public double Min_Y
		{
			get => min_Y;
			set
			{
				min_Y = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Min_Y)));
			}
		}
		public double Max_Y
		{
			get => max_Y;
			set
			{
				max_Y = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Max_Y)));
			}
		}
		public double ScrollValue_Y
		{
			get => scrollValue_Y;
			set
			{
				scrollValue_Y = value;
				if (value < Min_Y) scrollValue_Y = Min_Y;
				if (value > Max_Y) scrollValue_Y = Max_Y;

				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ScrollValue_Y)));
			}
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
					CurrentScale.Height = v;
					Plot?.SetYScale(CurrentScale.Height);
					UpdateScrollBars(false);
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
					CurrentScale.Width = v;
					Plot?.SetXScale(CurrentScale.Width);
					UpdateScrollBars(false);
				}

				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SampleWidthStr)));
			}
		}
		public SignalDataGV Plot
		{
			get => plot;
			set
			{
				plot = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Plot)));
			}
		}
		public Cursor PlotCursor => IsMouseRightButtonDown ? Cursors.ScrollAll : Cursors.Cross;


		//--------------------------------------- API ---------------------------------------------
		public double PixelsPerDip = 1.25;
		public void Freeze() => IsLoadingData = true;
		public async Task LoadData(ModelBase recording)
		{
			if (recording == null)
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
				Dispatcher.Invoke(delegate
				{
					Plot = new SignalDataGV(PixelsPerDip);
					Plot.SetData(sd);
					UpdateScrollBars(true);
					Plot.UpdatePlot(RectWindow, CurrentScale);
					UpdatePlotClip();
					Plot.ClearHighlightedPoint();
				});
			}

			IsLoadingData = false;
		}
		public void OnWindowResized()
		{
			UpdatePlotClip();
			UpdateScrollBars(false);
			Plot?.UpdatePlot(RectWindow, CurrentScale);
			Plot?.ClearHighlightedPoint();
		}


		//--------------------------------------- Events handlers ---------------------------------
		private void Plot_MouseMove(object sender, MouseEventArgs e)
		{
			Point currentMousePosition = e.GetPosition(plotGrid);

			Plot?.HighLightPointNearestTo(currentMousePosition);

			if (Mouse.RightButton == MouseButtonState.Released)
				IsMouseRightButtonDown = false;

			if (IsMouseRightButtonDown)
			{
				ScrollValue_X += MouseRightButtonDownPoint.X - currentMousePosition.X;
				ScrollValue_Y += MouseRightButtonDownPoint.Y - currentMousePosition.Y;

				MouseRightButtonDownPoint = currentMousePosition;

				Plot?.UpdatePlot(RectWindow, CurrentScale);
			}
		}
		private void Plot_MouseLeave(object sender, MouseEventArgs e)
		{
			IsMouseRightButtonDown = false;
			Plot?.ClearHighlightedPoint();
		}
		private bool IsMouseRightButtonDown
		{
			get => isMouseRightButtonDown;
			set
			{
				isMouseRightButtonDown = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PlotCursor)));
			}
		}
		private Point MouseRightButtonDownPoint;
		private void Plot_MouseDown(object sender, MouseButtonEventArgs e)
		{
			if (Mouse.RightButton == MouseButtonState.Pressed)
			{
				IsMouseRightButtonDown = true;
				MouseRightButtonDownPoint = e.GetPosition(plotGrid);
			}
		}
		private void Plot_MouseUp(object sender, MouseButtonEventArgs e)
		{
			if (Mouse.RightButton == MouseButtonState.Released)
				IsMouseRightButtonDown = false;
		}

		private void Plot_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
		{
			if (Plot == null) return;
			if (IsMouseRightButtonDown) return;

			if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
			{
				double deltaScale = 1.0 + Math.Sign(e.Delta) * 0.1;

				CurrentScale = new Size(CurrentScale.Width * deltaScale, CurrentScale.Height * deltaScale);

				Plot.Change_XY_Scale(CurrentScale);

				sampleWidthStr = CurrentScale.Width.ToString();
				y_ScaleStr = CurrentScale.Height.ToString();

				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Y_ScaleStr)));
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SampleWidthStr)));
			}
			else if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
			{
				if (e.Delta > 0)
					ScrollValue_X += Max_X * 0.05;
				else
					ScrollValue_X -= Max_X * 0.05;

				e.Handled = true;
			}
			else
			{
				if (e.Delta > 0)
					ScrollValue_Y -= Max_Y * 0.05;
				else
					ScrollValue_Y += Max_Y * 0.05;

				e.Handled = true;
			}

			Plot.UpdatePlot(RectWindow, CurrentScale);
			Plot.RefreshHighlightedPoint();
		}
		private void SbX_Scroll(object sender, ScrollEventArgs e)
		{
			Plot?.UpdatePlot(RectWindow, CurrentScale);
			Plot?.RefreshHighlightedPoint();
		}
		private void SbY_Scroll(object sender, ScrollEventArgs e)
		{
			Plot?.UpdatePlot(RectWindow, CurrentScale);
			Plot?.RefreshHighlightedPoint();
		}


		//--------------------------------------- Refreshing visual -------------------------------
		private void UpdatePlotClip()
		{
			PlotClip = new RectangleGeometry(new Rect(0.0, 0.0, plotGrid.ActualWidth, plotGrid.ActualHeight));
		}
		private void UpdateScrollBars(bool reset)
		{
			if (Plot == null) return;

			double prevMaxX = Max_X;
			double prevMaxY = Max_Y;

			Max_X = Math.Max(Plot.X_Range * CurrentScale.Width - RectWindow.Width, RectWindow.Width);
			Max_Y = Math.Max(Plot.Y_Range * CurrentScale.Height - RectWindow.Height, RectWindow.Height);

			if (reset)
			{
				ScrollValue_X = Min_X;
				ScrollValue_Y = Max_Y;
			}
			else
			{
				ScrollValue_X *= (prevMaxX != 0) ? Max_X / prevMaxX : 0;
				ScrollValue_Y *= (prevMaxY != 0) ? Max_Y / prevMaxY : 0;
			}
		}


		//--------------------------------------- Visual Info ---------------------------------------

		private Size CurrentScale;
		private Rect RectWindow => new Rect(
			ScrollValue_X,
			Max_Y - ScrollValue_Y,
			plotGrid.ActualWidth,
			plotGrid.ActualHeight);


		public event PropertyChangedEventHandler PropertyChanged;


		private bool isLoadingData;
		private SignalDataGV plot;
		private string sampleWidthStr;
		private string y_ScaleStr;
		private double min_X;
		private double max_X;
		private double min_Y;
		private double max_Y;
		private RectangleGeometry plotClip = new RectangleGeometry();
		private double scrollValue_X;
		private double scrollValue_Y;
		private bool isMouseRightButtonDown = false;
	}
}
