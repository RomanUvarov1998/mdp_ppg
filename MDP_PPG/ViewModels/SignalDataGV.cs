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
		public SignalDataGV()
		{

		}
		public void SetData(SignalData instance)
		{
			Instance = instance ?? throw new ArgumentNullException();

			double[] values = instance.Data.Select(d => 0.0 + d).ToArray();

			SignalContainer.SetData(values, 250.0);

			PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(SizeInfo)));
		}

		public SignalContainer SignalContainer = new SignalContainer();

		private Size CurrentScale;
		private Rect RectWindow;

		public void UpdatePlot(Rect rectWindow, Size currentScale)
		{
			RectWindow = rectWindow;
			CurrentScale = currentScale;

			PlotPoints = new PointCollection();
			X_Axis = new GeometryGroup();
			Y_Axis = new GeometryGroup();
			PlotGrid = new GeometryGroup();

			SignalContainer.SetContainers(PlotPoints, PlotGrid, X_Axis, Y_Axis, RectWindow, CurrentScale);

			PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(PlotPoints)));
			PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(X_Axis)));
			PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(Y_Axis)));
			PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(PlotGrid)));
		}

		public string OnMouseMove(Point mousePosition)
		{
			var p = SignalContainer.GetPoint(mousePosition, RectWindow, CurrentScale);

			return $"{p.X.ToString("G4")} с, {p.Y.ToString("G4")}";
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
		public void Change_XY_Scale(Size newScale)
		{
			CurrentScale = newScale;				

			UpdatePlot(RectWindow, CurrentScale);
		}

		public SignalData Instance { get; private set; }

		public string SizeInfo => Instance?.Data == null ? string.Empty : $"Размер {Instance.Data.Length.ToString("N")} байт";

		public PointCollection PlotPoints { get; set; } = new PointCollection();
		public GeometryGroup PlotGrid { get; set; } = new GeometryGroup();
		public GeometryGroup X_Axis { get; set; } = new GeometryGroup();
		public GeometryGroup Y_Axis { get; set; } = new GeometryGroup();


		public event PropertyChangedEventHandler PropertyChanged;
	}
}
