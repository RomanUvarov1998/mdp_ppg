using PPG_Database.KeepingModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MDP_PPG.ViewModels
{
	public class SignalDataGV : INotifyPropertyChanged
	{
		public SignalDataGV()
		{

		}
		public void SetData(SignalData instance, double sampleWidth, double y_scale,
			double leftBorder, double rightBorder)
		{
			Instance = instance ?? throw new ArgumentNullException();

			SetSignalValues(Instance.Data);

			SamplingFrequency = 250.0;
			SamplingPeriod = 1.0 / SamplingFrequency;

			this.sampleWidth = sampleWidth;
			y_Scale = y_scale;

			UpdatePlot(leftBorder, rightBorder);

			PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(SizeInfo)));
		}

		private double SamplingFrequency;
		private double SamplingPeriod;

		private void SetSignalValues(byte[] data)
		{
			SignalValues = data.Select(_byte => (double)_byte).ToArray();
			MinSignalValue = SignalValues.Min();
			MaxSignalValue = SignalValues.Max();

			MaxT = SignalValues.Length;
		}
		private double[] SignalValues;

		private double LeftBorder;
		private double RightBorder;

		private double MinSignalValue;
		private double MaxSignalValue;

		private int MaxT;

		private double MinBarsDist = 30.0;
		private double PlotMargin = 10.0;

		private double X_Min;
		private double X_Max;
		private double Y_Min;
		private double Y_Max;
		private double Y_0;

		public void UpdatePlot(double leftBorder, double rightBorder)
		{
			LeftBorder = leftBorder;
			RightBorder = rightBorder;

			X_Min = 0.0;
			X_Max = PlotMargin * 2 + MaxT * sampleWidth;
			Y_Min = MinSignalValue * y_Scale - PlotMargin;
			Y_Max = MaxSignalValue * y_Scale + PlotMargin;
			Y_0 = Y_Max - PlotMargin + MinSignalValue * y_Scale;

			Refresh_Y_Axis();
			Refresh_X_Axis();
			Refresh_Grid();
			Refresh_PlotPoints();
		}
		private void Refresh_PlotPoints()
		{
			PlotPoints = new PointCollection();

			double left = LeftBorder, right = RightBorder;
			double xmin = X_Min;
			double ymax = Y_Max;
			double xCount = SignalValues.Length;
			double plotMargin = PlotMargin;
			double smin = MinSignalValue;
			double yScale = y_Scale;
			double sampWidth = sampleWidth;

			for (int x = 0; x < xCount; ++x)
			{
				double pointX = xmin + plotMargin + x * sampWidth;

				if (pointX >= left && pointX <= right)
				{
					var ps = new Point(
						pointX,
						ymax - plotMargin - (SignalValues[x] - smin) * yScale);
					PlotPoints.Add(ps);
				}
			}

			PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(PlotPoints)));
		}
		private void Refresh_X_Axis()
		{
			X_Axis = new GeometryGroup();
			VertBarsPoss.Clear();

			double yA = PlotMargin * 0.5;
			double yTop = 0.0;
			double yBottom = PlotMargin;

			//X axis
			var p1 = new Point(X_Min, yA);
			var p2 = new Point(X_Max, yA);
			X_Axis.Children.Add(new LineGeometry(p1, p2));

			int deltaT_inMs = (int)(1.0 / SamplingFrequency * 1000.0);
			TimeSpan t1 = new TimeSpan(0);
			TimeSpan deltaT = new TimeSpan(0, 0, 0, 0, deltaT_inMs);

			//X bars
			double verticalBarsCurDistance = 0.0;
			double xCount = SignalValues.Length;
			double xmin = X_Min;
			double plotMargin = PlotMargin;
			double deltaX = sampleWidth;
			double minBarsDist = MinBarsDist;
			double left = LeftBorder, right = RightBorder;
			double zeroBarDist = X_Min + PlotMargin + deltaX;
			for (int x = 0; x <= xCount; ++x)
			{
				double pointX = xmin + plotMargin + x * deltaX;
				if (pointX >= left && pointX <= right)
					if (verticalBarsCurDistance > minBarsDist || pointX < zeroBarDist)
					{
						verticalBarsCurDistance = 0.0;

						var dp1 = new Point(pointX, yTop);
						var dp2 = new Point(pointX, yBottom);
						X_Axis.Children.Add(new LineGeometry(dp1, dp2));

						var text = GetTextField(t1.TotalSeconds);
						text.MaxTextWidth = minBarsDist;
						text.SetFontWeight(FontWeights.UltraLight);
						var tg = text.BuildGeometry(new Point(pointX, yBottom));
						X_Axis.Children.Add(tg);

						VertBarsPoss.Add(pointX);
					}

				verticalBarsCurDistance += deltaX;
				t1 = t1 + deltaT;
			}

			PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(X_Axis)));
		}
		private List<double> VertBarsPoss = new List<double>();
		private void Refresh_Y_Axis()
		{
			Y_Axis = new GeometryGroup();
			HorzBarsPoss.Clear();

			double ymin = Math.Min(Y_0, Y_Min);
			double ymax = Math.Max(Y_0, Y_Max);

			int textsCount = (int)((ymax - ymin) / MinBarsDist);

			FormattedText[] texts = new FormattedText[textsCount];
			double[] textsYs = new double[textsCount];

			//Y bars
			for (int i = 0; i < textsCount; ++i)
			{
				double s = ymin + PlotMargin + i * MinBarsDist;

				var text = GetTextField((Y_0 - s) / y_Scale);
				text.SetFontWeight(FontWeights.UltraLight);

				textsYs[i] = s;
				texts[i] = text;

				HorzBarsPoss.Add(s);
			}

			double maxTextWidth = texts.Length > 0 ? texts.Max(t => t.Width) : 0.0;
			double x0 = PlotMargin + maxTextWidth + PlotMargin * 0.5;

			//Y axis
			var p1 = new Point(x0, ymin);
			var p2 = new Point(x0, ymax);
			Y_Axis.Children.Add(new LineGeometry(p1, p2));

			for (int i = 0; i < textsCount; ++i)
			{
				var textOrigin = new Point(x0 - texts[i].Width - PlotMargin * 0.5, textsYs[i]);
				var tg = texts[i].BuildGeometry(textOrigin);

				var dp1 = new Point(x0 - PlotMargin * 0.5, textsYs[i]);
				var dp2 = new Point(x0 + PlotMargin * 0.5, textsYs[i]);
				Y_Axis.Children.Add(new LineGeometry(dp1, dp2));

				Y_Axis.Children.Add(tg);
			}

			PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(Y_Axis)));
		}
		private List<double> HorzBarsPoss = new List<double>();
		private void Refresh_Grid()
		{
			PlotGrid = new GeometryGroup();

			double ymin = Math.Min(Y_0, Y_Min);
			double ymax = Math.Max(Y_0, Y_Max);
			double xmin = X_Min;
			double xmax = X_Max;

			//horizontal grid
			foreach (var y in HorzBarsPoss)
			{
				var p1 = new Point(xmin, y);
				var p2 = new Point(xmax, y);
				PlotGrid.Children.Add(new LineGeometry(p1, p2));
			}

			//vertical grid
			foreach (var x in VertBarsPoss)
			{
				var p1 = new Point(x, ymin);
				var p2 = new Point(x, ymax);
				PlotGrid.Children.Add(new LineGeometry(p1, p2));
			}

			PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(PlotGrid)));
		}
		public string OnMouseMove(Point mousePosition)
		{
			double tValue = (mousePosition.X - X_Min) / sampleWidth * SamplingPeriod;
			double sValue = (Y_0 - mousePosition.Y) / y_Scale;

			return $"{tValue.ToString("G4")} с, {sValue.ToString("G4")}";
		}

		private string MyDoubleToString(double d) => d.ToString("G04");
		private FormattedText GetTextField(double value) => GetTextField(MyDoubleToString(value));
		private FormattedText GetTextField(string text) =>
			new FormattedText(
				text,
				CultureInfo.CurrentCulture,
				FlowDirection.LeftToRight,
				new Typeface("Verdana"),
				10,
				Brushes.Gray,
				1.25);

		public void SetYScale(double newValue)
		{
			y_Scale = newValue;
			UpdatePlot(LeftBorder, RightBorder);
		}
		public void SetXScale(double newValue)
		{
			sampleWidth = newValue;
			UpdatePlot(LeftBorder, RightBorder);
		}
		public void Change_XY_Scale(int factor)
		{
			double deltaScale = 1.0 + Math.Sign(factor) * 0.1;

			sampleWidth *= deltaScale;
			y_Scale *= deltaScale;

			UpdatePlot(LeftBorder, RightBorder);
		}

		public SignalData Instance { get; private set; }

		public string SizeInfo => Instance?.Data == null ? string.Empty : $"Размер {Instance.Data.Length.ToString("N")} байт";

		public PointCollection PlotPoints { get; set; } = new PointCollection();
		public GeometryGroup PlotGrid { get; set; } = new GeometryGroup();
		public GeometryGroup X_Axis { get; set; } = new GeometryGroup();
		public GeometryGroup Y_Axis { get; set; } = new GeometryGroup();


		public event PropertyChangedEventHandler PropertyChanged;

		private double sampleWidth;
		private double y_Scale;
	}
}
