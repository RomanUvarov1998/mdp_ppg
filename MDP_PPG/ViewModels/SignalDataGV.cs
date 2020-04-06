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
		}
		private double PixelsPerDip;

		public void SetData(Recording instance, Size currentScale)
		{
			Instance = instance ?? throw new ArgumentNullException();

			SignalContainer.SetData(instance, 250.0, PixelsPerDip);

			CurrentScale = currentScale;

			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SizeInfo)));
		}

		private PlotValuesCounter SignalContainer = new PlotValuesCounter();

		private Size CurrentScale;
		private Rect RectWindow;
		private Point MousePos;

		public Size AllPlotSize_Scaled
		{
			get
			{
				var notScaledRect = SignalContainer.AllPlotSize;

				return new Size(
					notScaledRect.Width * CurrentScale.Width,
					notScaledRect.Height * CurrentScale.Height);
			}
		}

		public void UpdatePlot(Rect rectWindow, Size currentScale)
		{
			RectWindow = rectWindow;
			CurrentScale = currentScale;

			Plots = null;
			X_Axis = new GeometryGroup();
			Y_Axis = new GeometryGroup();
			PlotGrid = new GeometryGroup();

			SignalContainer.SetContainers(
				PlotGrid,
				X_Axis,
				Y_Axis,
				RectWindow, CurrentScale);

			Plots = SignalContainer.PlotDataKeepers;
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(X_Axis)));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Y_Axis)));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PlotGrid)));
		}

		public void HighLightPointNearestTo(Point mousePosition)
		{
			HighLightedPoint = new GeometryGroup();
			HighLightedPointText = new GeometryGroup();
			MousePos = mousePosition;

			SignalContainer.HighLightPointNearestTo(mousePosition, RectWindow, HighLightedPoint, HighLightedPointText);

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

		public void SetYScale(double newValue)
		{
			CurrentScale.Height = newValue;
			UpdatePlot(RectWindow, CurrentScale);
		}
		public void SetXScale(double newValue)
		{
			CurrentScale.Width = newValue;
			UpdatePlot(RectWindow, CurrentScale);
		}
		public void Change_XY_Scale(Rect rectWindow, Size newScale)
		{
			RectWindow = rectWindow;
			CurrentScale = newScale;

			UpdatePlot(RectWindow, CurrentScale);
		}

		public Recording Instance { get; private set; }

		public string SizeInfo => Instance == null ? string.Empty : $"Размер {Instance.SignalDatas.Sum(sd => sd.Data.Length).ToString("N")} байт";

		public List<PlotDataKeeper> Plots 
		{ 
			get => plots;
			set
			{
				plots = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Plots)));
			}
		}
		public GeometryGroup PlotGrid { get; set; } = new GeometryGroup();
		public GeometryGroup X_Axis { get; set; } = new GeometryGroup();
		public GeometryGroup Y_Axis { get; set; } = new GeometryGroup();
		public GeometryGroup HighLightedPoint { get; set; } = new GeometryGroup();
		public GeometryGroup HighLightedPointText { get; set; } = new GeometryGroup();


		public event PropertyChangedEventHandler PropertyChanged;


		private List<PlotDataKeeper> plots = new List<PlotDataKeeper>();
	}
}
