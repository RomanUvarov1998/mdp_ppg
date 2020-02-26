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

		public Point GetPoint(Point point, Rect rectWindow, Size scale)
		{
			double x = (rectWindow.Left + point.X) / scale.Width;
			double y = (rectWindow.Height - point.Y) / scale.Height;
			return new Point(x, y);
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
			var dataPoints = OriginalPoints
				.Where(p =>
					p.X * scale.Width >= rectWindow.Left &&
					p.X * scale.Width <= rectWindow.Right)
				.Select(p => new PlotDataPoint(
					new Point(p.X * scale.Width - rectWindow.Left, rectWindow.Bottom - p.Y * scale.Height),
					p)
				)
				.ToArray();

			foreach (var pdp in dataPoints)
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
			for (int i = 0; i < dataPoints.Length; ++i)
			{
				if (curDist >= MIN_BAR_DIST_X || i == 0)
				{
					var x = dataPoints[i].DisplayPoint.X;
					xAxisBarsXs.Add(x);

					x_Axis.Children.Add(GetVerticalBarFor_X(x));
					x_Axis.Children.Add(TextAt(dataPoints[i].ValueTime.X.ToString("G4"), x));

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
			double cur_y = rectWindow.Bottom - rectWindow.Top;
			while (cur_y > 0.0)
			{
				yAxisBarsYs.Add(cur_y);
				cur_y -= MIN_BAR_DIST_Y;
			}

			//creating texts using Ys
			var texts = new List<FormattedText>();
			foreach (var y in yAxisBarsYs)
			{
				var yValue = (rectWindow.Bottom - y) / scale.Height;
				var ft = GetTextField(yValue.ToString("G4"));
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
