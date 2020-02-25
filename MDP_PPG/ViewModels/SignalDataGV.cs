using PPG_Database.KeepingModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
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
		public void SetData(SignalData instance, double sampleWidth, double y_scale)
		{
			Instance = instance ?? throw new ArgumentNullException();

			SetSignalValues(Instance.Data);

			SamplingFrequency = 250.0;
			this.sampleWidth = sampleWidth;
			y_Scale = y_scale;

			UpdatePlot();
		}

		private double SamplingFrequency;

		private void SetSignalValues(byte[] data)
		{
			SignalValues = data.Select(_byte => (double)_byte).ToArray();
			MinSignalValue = SignalValues.Min();
			MaxSignalValue = SignalValues.Max();

			MaxT = SignalValues.Length;
		}
		private double[] SignalValues;
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

		private void UpdatePlot()
		{
			X_Min = 0.0;
			X_Max = PlotMargin * 2 + MaxT * sampleWidth;
			Y_Min = MinSignalValue * y_Scale - PlotMargin;
			Y_Max = MaxSignalValue * y_Scale + PlotMargin;
			Y_0 = Y_Max - PlotMargin + MinSignalValue * y_Scale;

			Refresh_PlotPoints();
			Refresh_Grid();
			Refresh_Y_Axis();
			Refresh_X_Axis();
		}
		private void Refresh_PlotPoints()
		{
			PlotPoints = new PointCollection();

			for (int x = 0; x < SignalValues.Length; ++x)
			{
				var ps = new Point(
					X_Min + PlotMargin + x * sampleWidth,
					Y_Max - PlotMargin - (SignalValues[x] - MinSignalValue) * y_Scale);
				PlotPoints.Add(ps);
			}

			PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(PlotPoints)));
		}
		private void Refresh_Grid()
		{
			PlotGrid = new GeometryGroup();

			double ymin = Math.Min(Y_0, Y_Min);
			double ymax = Math.Max(Y_0, Y_Max);

			//horizontal grid
			for (double s = ymin + PlotMargin; s <= ymax - PlotMargin; s += MinBarsDist)
			{
				var p1 = new Point(X_Min, s);
				var p2 = new Point(X_Max, s);
				PlotGrid.Children.Add(new LineGeometry(p1, p2));
			}

			//vertical grid
			double verticalBarsCurDistance = 0.0;
			for (double x = X_Min + PlotMargin; x <= X_Max - PlotMargin; x += sampleWidth)
			{
				verticalBarsCurDistance += sampleWidth;

				if (verticalBarsCurDistance > MinBarsDist || x < X_Min + PlotMargin + sampleWidth)
				{
					verticalBarsCurDistance = 0.0;

					var p1 = new Point(x, ymin);
					var p2 = new Point(x, ymax);
					PlotGrid.Children.Add(new LineGeometry(p1, p2));
				}
			}

			PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(PlotGrid)));
		}
		private void Refresh_X_Axis()
		{
			X_Axis = new GeometryGroup();

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
			//double pixelsPerDip = VisualTreeHelper.GetDpi().PixelsPerDip;

			//X bars
			double verticalBarsCurDistance = 0.0;
			for (double x = X_Min + PlotMargin; x <= X_Max - PlotMargin; x += sampleWidth)
			{
				verticalBarsCurDistance += sampleWidth;

				if (verticalBarsCurDistance > MinBarsDist || x < X_Min + PlotMargin + sampleWidth)
				{
					verticalBarsCurDistance = 0.0;

					var dp1 = new Point(x, yTop);
					var dp2 = new Point(x, yBottom);
					X_Axis.Children.Add(new LineGeometry(dp1, dp2));

					var text = GetTextField(t1.TotalSeconds);
					text.MaxTextWidth = MinBarsDist;
					text.SetFontWeight(FontWeights.UltraLight);
					var tg = text.BuildGeometry(new Point(x, yBottom));
					X_Axis.Children.Add(tg);
				}

				t1 = t1 + deltaT;
			}

			PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(X_Axis)));
		}
		private void Refresh_Y_Axis()
		{
			Y_Axis = new GeometryGroup();

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
			}

			double x0 = PlotMargin + texts.Max(t => t.Width) + PlotMargin * 0.5;

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
		private string MyDoubleToString(double d) => d.ToString("E02");
		private FormattedText GetTextField(double value) => 
			new FormattedText(
				MyDoubleToString(value),
				CultureInfo.CurrentCulture,
				FlowDirection.LeftToRight,
				new Typeface("Verdana"),
				10,
				Brushes.Gray,
				1.25);

		public double Y_Scale
		{
			get => y_Scale;
			set
			{
				if (value > 0)
				{
					y_Scale = value;
					UpdatePlot();
				}
			}
		}
		public double SampleWidth
		{
			get => sampleWidth;
			set
			{
				if (value > 0)
				{
					sampleWidth = value;
					UpdatePlot();
				}
			}
		}
		public void Change_XY_Scale(int factor)
		{
			double deltaScale = 1.0 + Math.Sign(factor) * 0.1;

			sampleWidth *= deltaScale;
			y_Scale *= deltaScale;

			UpdatePlot();
		}

		public SignalData Instance { get; private set; }

		public string SizeInfo => Instance?.Data == null ? string.Empty : $"Размер {Instance.Data.Length} байт";

		public PointCollection PlotPoints { get; set; } = new PointCollection();
		public GeometryGroup PlotGrid { get; set; } = new GeometryGroup();
		public GeometryGroup X_Axis { get; set; } = new GeometryGroup();
		public GeometryGroup Y_Axis { get; set; } = new GeometryGroup();


		public event PropertyChangedEventHandler PropertyChanged;

		private double sampleWidth;
		private double y_Scale;
	}
}
