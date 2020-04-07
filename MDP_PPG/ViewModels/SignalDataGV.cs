using MDP_PPG.Helpers;
using PPG_Database.KeepingModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace MDP_PPG.ViewModels
{
	public class SignalDataGV : INotifyPropertyChanged
	{
		public SignalDataGV(double pixelsPerDip)
		{
			PixelsPerDip = pixelsPerDip;
			PlotValuesCounter = new PlotValuesCounter();
		}
		private double PixelsPerDip;

		public void SetData(Recording instance, double scaleX)
		{
			Instance = instance ?? throw new ArgumentNullException();

			PlotValuesCounter.SetData(instance, 250.0, PixelsPerDip);

			ScaleX = scaleX;

			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SizeInfo)));
		}

		private PlotValuesCounter PlotValuesCounter;

		private double ScaleX;
		private Rect RectWindow;
		private Point MousePos;

		public Size AllPlotSize_Scaled
		{
			get
			{
				Size notScaledRect = PlotValuesCounter.AllPlotSize;

				double scaledWidth = notScaledRect.Width * ScaleX;

				double scaledHeight = PlotValuesCounter.PlotDataKeepers.Select(pl => pl.ScaleY * pl.Max_Y).Max();

				return new Size(scaledWidth, scaledHeight);
			}
		}

		public void UpdatePlot(Rect rectWindow, double scaleX)
		{
			RectWindow = rectWindow;
			ScaleX = scaleX;

			plotSelectedByUser = false;
			if (selectedPlot == null || !PlotValuesCounter.PlotDataKeepers.Contains(selectedPlot))
				selectedPlot = PlotValuesCounter.PlotDataKeepers.FirstOrDefault();

			if (selectedPlot != null && PlotValuesCounter.PlotDataKeepers != null)
				foreach (var pl in PlotValuesCounter.PlotDataKeepers)
					pl.IsSelected = (pl == selectedPlot);
			
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Plots)));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedPlot)));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedPlotScaleYStr)));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ScaleY_IsEnabled)));

			X_Axis = new GeometryGroup();
			Y_Axis = new GeometryGroup();
			PlotGrid = new GeometryGroup();

			PlotValuesCounter.SetContainers(
				PlotGrid,
				X_Axis,
				Y_Axis,
				RectWindow, scaleX);

			plotSelectedByUser = true;
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(X_Axis)));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Y_Axis)));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PlotGrid)));
		}

		public void HighLightPointNearestTo(Point mousePosition)
		{
			HighLightedPoint = new GeometryGroup();
			HighLightedPointText = new GeometryGroup();
			MousePos = mousePosition;

			PlotValuesCounter.HighLightPointNearestTo(mousePosition, RectWindow, HighLightedPoint, HighLightedPointText);

			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HighLightedPoint)));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HighLightedPointText)));
		}
		public void RefreshHighlightedPoint()
		{
			HighLightPointNearestTo(MousePos);
		}
		public void ClearHighlightedPoint()
		{
			HighLightedPoint = new GeometryGroup();
			HighLightedPointText = new GeometryGroup();
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HighLightedPoint)));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HighLightedPointText)));
		}

		public void SetXScale(double scaleX)
		{
			ScaleX = scaleX;
			UpdatePlot(RectWindow, ScaleX);
		}
		//public void Change_XY_Scale(Rect rectWindow, Size newScale)
		//{
		//	RectWindow = rectWindow;
		//	CurrentScale = newScale;

		//	UpdatePlot(RectWindow, CurrentScale);
		//}

		public Recording Instance { get; private set; }

		public string SizeInfo => Instance == null ? string.Empty : $"Размер {Instance.SignalDatas.Sum(sd => sd.Data.Length).ToString("N")} байт";

		private bool plotSelectedByUser = true;
		//public List<PlotDataKeeper> Plots
		//{
		//	get => plots;
		//	set
		//	{
		//		plots = value;
		//		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Plots)));
		//	}
		//}
		public PlotDataKeeper SelectedPlot
		{
			get => selectedPlot;
			set
			{
				if (plotSelectedByUser)
				{
					selectedPlot = value;

					if (selectedPlot != null && PlotValuesCounter.PlotDataKeepers != null)
						foreach (var pl in PlotValuesCounter.PlotDataKeepers)
							pl.IsSelected = (pl == selectedPlot);

					if (selectedPlot != null)
						UpdatePlot(RectWindow, ScaleX);
				}

				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedPlot)));
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedPlotScaleYStr)));
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ScaleY_IsEnabled)));
			}
		}
		public string SelectedPlotScaleYStr
		{
			get => SelectedPlot != null ? SelectedPlot.ScaleY.ToString() : string.Empty;
			set
			{
				double v = -1.0;
				if (double.TryParse(value, out v) && v > 0)
				{
					SelectedPlot.ScaleY = v;
					UpdatePlot(RectWindow, ScaleX);
				}
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedPlotScaleYStr)));
			}
		}
		public bool ScaleY_IsEnabled => SelectedPlot != null;
		public List<PlotDataKeeper> Plots => PlotValuesCounter.PlotDataKeepers;
		public GeometryGroup PlotGrid { get; set; } = new GeometryGroup();
		public GeometryGroup X_Axis { get; set; } = new GeometryGroup();
		public GeometryGroup Y_Axis { get; set; } = new GeometryGroup();
		public GeometryGroup HighLightedPoint { get; set; } = new GeometryGroup();
		public GeometryGroup HighLightedPointText { get; set; } = new GeometryGroup();


		public event PropertyChangedEventHandler PropertyChanged;


		//private List<PlotDataKeeper> plots = new List<PlotDataKeeper>();
		private PlotDataKeeper selectedPlot;
		private PlotValuesCounter plotValuesCounter;
	}
}
