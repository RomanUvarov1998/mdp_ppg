using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace MDP_PPG.ViewModels
{
	public class SignalContainer
	{
		public void SetData(double[] values, double fs, double pixelsPerDip)
		{
			if (fs <= 0.0) throw new ArgumentException("fs > 0");
			if (values == null) throw new ArgumentNullException("values must be not null");

			Values = values;
			Fs = fs;
			Ts = 1.0 / Fs;
			OriginalPoints = values
				.Select((v, index) => new Point(index * Ts, v))
				.ToArray();

			Min_X = 0.0;
			Max_X = Values.Length * Ts;

			Min_Y = Values.Min();
			Max_Y = Values.Max();

			X_Range = Max_X - Min_X;
			Y_Range = Max_Y - Min_Y;

			PixelsPerDip = pixelsPerDip;
		}
		private double PixelsPerDip;

		public double[] Values;
		public Point[] OriginalPoints;
		public double Fs;
		public double Ts;

		public double Min_X;
		public double Max_X;

		public double Min_Y;
		public double Max_Y;

		public double X_Range;
		public double Y_Range;

		public void HighLightPointNearestTo(Point point, Rect rectWindow, Size scale, GeometryGroup gg)
		{
			if (DrawnPoints.Length == 0) return;

			PlotDataPoint pdp = DrawnPoints[0];
			double minDistY = Dist2(pdp.DisplayPoint, point);

			if (DrawnPoints.Length > 1)
				for (int i = 1; i < DrawnPoints.Length; ++i)
				{
					double curDist = Dist2(DrawnPoints[i].DisplayPoint, point);
					if (curDist < minDistY)
					{
						minDistY = curDist;
						pdp = DrawnPoints[i];
					}
				}

			string message = $"{pdp.ValueTime.X.ToString("G3")}; {pdp.ValueTime.Y.ToString("G4")}";
			var tf = GetTextField(message);

			var loc = new Point(pdp.DisplayPoint.X - tf.Width - 3, pdp.DisplayPoint.Y);

			loc.X = Math.Max(0.0, point.X + 20);
			loc.X = Math.Min(loc.X, rectWindow.Width - tf.Width);
			loc.Y = Math.Max(0.0, loc.Y);
			loc.Y = Math.Min(loc.Y, rectWindow.Height - tf.Height - 3);

			var geom = tf.BuildGeometry(loc);
			gg.Children.Add(geom);

			var p1 = pdp.DisplayPoint;
			var p2 = pdp.DisplayPoint;

			p1.Offset(-1, -1);
			p2.Offset(1, 1);

			var rectGeom = new RectangleGeometry(new Rect(p1, p2));
			gg.Children.Add(rectGeom);
		}

		public void SetContainers(
			PointCollection plotPoints, 
			GeometryGroup plotGrid, 
			GeometryGroup x_Axis, 
			GeometryGroup y_Axis,
			Rect rectWindow, Size scale)
		{
			//------------------- draw plot --------------------------
			#region plot
			DrawnPoints = OriginalPoints
				.Where(p =>
					p.X * scale.Width >= rectWindow.Left &&
					p.X * scale.Width <= rectWindow.Right)
				.Select(p => new PlotDataPoint(
					new Point(WtoD_X(p.X, rectWindow, scale), WtoD_Y(p.Y, rectWindow, scale)),
					p)
				)
				.ToArray();

			foreach (var pdp in DrawnPoints)
				plotPoints.Add(pdp.DisplayPoint);
			#endregion

			//-------------------- draw X axis -------------------------
			#region xAxis
			var axisX = new LineGeometry(
				new Point(0.0, X_AXIS_Y),
				new Point(rectWindow.Width, X_AXIS_Y));
			x_Axis.Children.Add(axisX);

			double deltaDist = Ts * scale.Width;
			double curDist = 0.0;
			List<double> xAxisBarsXs = new List<double>();
			for (int i = 0; i < DrawnPoints.Length; ++i)
			{
				if (curDist >= MIN_BAR_DIST_X || i == 0)
				{
					var x = DrawnPoints[i].DisplayPoint.X;
					xAxisBarsXs.Add(x);

					x_Axis.Children.Add(GetVerticalBarFor_X(x));
					x_Axis.Children.Add(TextAt(DrawnPoints[i].ValueTime.X.ToString("G4"), x));

					curDist = 0.0;
				}
				curDist += deltaDist;
			}
			#endregion

			//-------------------- draw Y axis -------------------------
			#region yAxis
			//creating Y of each bar
			List<double> yAxisBarsYs = new List<double>();
			if (rectWindow.Bottom < rectWindow.Top) throw new Exception("wrong rect");

			double distDegree = Math.Ceiling(Math.Log10(MIN_BAR_DIST_Y / scale.Height));
			double dist = Math.Pow(10.0, distDegree);

			//signal values for rect top and rect bottom
			double maxValue = DtoW_Y(0.0, rectWindow, scale);
			double minValue = DtoW_Y(rectWindow.Height, rectWindow, scale);

			List<double> roundValues = new List<double>();

			if (maxValue <= 0 && minValue <= 0)
			{
				for (double value = 0.0; value > minValue; value -= dist)
					if (value >= minValue && value <= maxValue)
						roundValues.Add(value);
			}
			else if (maxValue > 0 && minValue < 0)
			{
				for (double value = dist; value < maxValue; value += dist)
					if (value >= minValue && value <= maxValue)
						roundValues.Add(value);

				roundValues.Add(0.0);

				for (double value = -dist; value > minValue; value -= dist)
					if (value >= minValue && value <= maxValue)
						roundValues.Add(value);
			}
			else if (maxValue >= 0 && minValue >= 0)
			{
				for (double value = 0.0; value < maxValue; value += dist)
					if (value >= minValue && value <= maxValue)
						roundValues.Add(value);
			}

			roundValues = roundValues.OrderBy(v => v).ToList();

			yAxisBarsYs = roundValues
				.Select(v => WtoD_Y(v, rectWindow, scale))
				.ToList();

			//creating texts using Ys
			var texts = new List<FormattedText>();
			for (int i = 0; i < yAxisBarsYs.Count; ++i)
			{
				var ft = GetTextField(roundValues[i].ToString("G4"));
				texts.Add(ft);
			}
			double maxTextWidth = texts.Count > 0 ? texts.Max(t => t.Width) : 0;

			//drawing bars and texts
			for (int i = 0; i < yAxisBarsYs.Count; i++)
			{
				y_Axis.Children.Add(GetHorizontalBarFor_Y(yAxisBarsYs[i], maxTextWidth));
				y_Axis.Children.Add(texts[i].BuildGeometry(
					new Point(
						maxTextWidth - texts[i].Width,
						yAxisBarsYs[i] - texts[i].Height / (i == 0 ? 1 : 2)
					)
				));
			}

			//drawing axis
			var axisY = new LineGeometry(
				new Point(maxTextWidth + Y_BAR_WIDTH / 2.0, 0.0),
				new Point(maxTextWidth + Y_BAR_WIDTH / 2.0, rectWindow.Height));
			y_Axis.Children.Add(axisY);
			#endregion

			//-------------------- draw Grid -------------------------
			#region grid
			foreach (var x in xAxisBarsXs)
			{
				plotGrid.Children.Add(
					new LineGeometry(
						new Point(x, 0.0), new Point(x, rectWindow.Height)
					)
				);
			}

			foreach (var y in yAxisBarsYs)
			{
				plotGrid.Children.Add(
					new LineGeometry(
						new Point(0.0, y), new Point(rectWindow.Width, y)
					)
				);
			}
			#endregion
		}
		private double WtoD_Y(double y, Rect rectWindow, Size scale) =>
			rectWindow.Height - (y * scale.Height - rectWindow.Top);
		private double WtoD_X(double x, Rect rectWindow, Size scale) =>
			x * scale.Width - rectWindow.Left;
		private double DtoW_Y(double y, Rect rectWindow, Size scale) =>
			(rectWindow.Height + rectWindow.Top - y) / scale.Height;
		private double DtoW_X(double x, Rect rectWindow, Size scale) =>
			(x + rectWindow.Left) / scale.Width;
		private PlotDataPoint[] DrawnPoints = new PlotDataPoint[0];
		private double Dist2(Point p1, Point p2) => 
			Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2);

		private const double X_AXIS_Y = 10.0;
		private const double X_AXIS_BAR_TOP = 5.0;
		private const double X_AXIS_BAR_BOTTOM = 15.0;
		private const double MIN_BAR_DIST_X = 40.0;

		private const double Y_BAR_WIDTH = 10.0;
		private const double MIN_BAR_DIST_Y = 20.0;

		private LineGeometry GetVerticalBarFor_X(double x) =>
			new LineGeometry(new Point(x, X_AXIS_BAR_TOP), new Point(x, X_AXIS_BAR_BOTTOM));
		private Geometry TextAt(string text, double x)
		{
			var ft = GetTextField(text);
			ft.MaxTextWidth = MIN_BAR_DIST_X - 5.0;
			return ft.BuildGeometry(new Point(x, X_AXIS_BAR_BOTTOM));
		}
		private LineGeometry GetHorizontalBarFor_Y(double y, double left) =>
			new LineGeometry(new Point(left, y), new Point(left + Y_BAR_WIDTH, y));
		private FormattedText GetTextField(string text) =>
			new FormattedText(
				text,
				CultureInfo.CurrentCulture,
				FlowDirection.LeftToRight,
				new Typeface("Verdana"),
				10,
				Brushes.Gray,
				PixelsPerDip);			 
	}
}
