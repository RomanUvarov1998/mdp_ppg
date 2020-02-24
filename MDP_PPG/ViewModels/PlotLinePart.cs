using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;

namespace MDP_PPG.ViewModels
{
	public class PlotLinePart : INotifyPropertyChanged
	{
		public static List<PlotLinePart> GetLines(double[] signalValues, double samplingFrequency)
		{
			List<PlotLinePart> list = new List<PlotLinePart>();

			int deltaT_inMs = (int)(1.0 / samplingFrequency * 1000.0);

			TimeSpan t1 = new TimeSpan(0);
			TimeSpan deltaT = new TimeSpan(0, 0, 0, 0, deltaT_inMs);
			TimeSpan t2 = t1 + deltaT;

			for (int i = 0; i < signalValues.Length - 1; i++)
			{
				var line = new PlotLinePart(t1, t2, signalValues[i], signalValues[i + 1], signalValues.Min(), signalValues.Max());
				list.Add(line);

				t1 = t2;
				t2 = t2 + deltaT;
			}

			if (list.Count > 0)
			{
				list[0].T1_Visibility = Visibility.Visible;
			}

			return list;
		}

		public PlotLinePart(TimeSpan t1, TimeSpan t2, double s1, double s2, double minSignalValue, double maxSignalValue)
		{
			this.t1 = t1;
			this.t2 = t2;

			this.s1 = s1;
			this.s2 = s2;

			MinSignalValue = minSignalValue;
			MaxSignalValue = maxSignalValue;

			UpdateY_Deltas();
		}

		public double Y_Scale { get; set; }
		public double MinSignalValue_Display => 0;
		public double MaxSignalValue_Display => (MaxSignalValue - MinSignalValue) * Y_Scale;
		private double K_Y = 10.0;
		public double[] Y_Deltas { get; set; } = new double[10];
		public void UpdateY_Deltas()
		{
			Y_Deltas = Y_Deltas
				.Select((d, ind) =>
					ind *
					(MaxSignalValue - MinSignalValue) / K_Y *
					Y_Scale)
				.ToArray();
		}

		public double SampleWidth { get; set; } = 10;
		public double TimeLinePartWidth { get; set; }
		public double S1_Display => (s1 - MinSignalValue) * Y_Scale;
		public double S2_Display => (s2 - MinSignalValue) * Y_Scale;

		//---------------------------------- GUI props ---------------------------
		public Visibility T1_Visibility { get; set; } = Visibility.Collapsed;
		public Visibility T2_Visibility { get; set; } = Visibility.Visible;
		public string T1_Str => GetTimeStr(t1);
		public string T2_Str => GetTimeStr(t2);
		public string S1_Str => S2_Display.ToString("E04");
		public string S2_Str => S1_Display.ToString("E04");
		private string GetTimeStr(TimeSpan t)
		{
			StringBuilder sb = new StringBuilder(string.Empty);

			sb.Append((int)t.TotalSeconds);

			if (t.Milliseconds > 0)
			{
				sb.Append(".");
				sb.Append(t.Milliseconds);
			}

			return sb.ToString();
		}
		public void UpdateDisplayedLine()
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(S1_Display)));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(S2_Display)));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MinSignalValue_Display)));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MaxSignalValue_Display)));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Y_Deltas)));

			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SampleWidth)));

			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TimeLinePartWidth)));

			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(T1_Visibility)));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(T1_Str)));

			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(T2_Visibility)));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(T2_Str)));
		}


		public event PropertyChangedEventHandler PropertyChanged;


		private TimeSpan t1;
		private TimeSpan t2;
		private double s1;
		private double s2;
		private double MinSignalValue;
		private double MaxSignalValue;
	}
}
