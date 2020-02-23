using PPG_Database.KeepingModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace MDP_PPG.ViewModels
{
	public class SignalDataGV : INotifyPropertyChanged
	{
		public SignalDataGV(SignalData instance)
		{
			Instance = instance ?? throw new ArgumentNullException();

			SignalValues = GetSignalValues(Instance.Data);

			LineParts = PlotLinePart.GetLines(SignalValues, 250.0);

			SampleWidth = "10";
			Y_Scale = "1";
		}

		private double[] GetSignalValues(byte[] data)
		{
			return data.Select(_byte => (double)_byte).ToArray();
		}
		private double[] SignalValues;

		public string Y_Scale
		{
			get => y_Scale.ToString();
			set
			{
				double v;
				if (!double.TryParse(value, out v)) return;

				y_Scale = v;
				foreach (var l in LineParts)
					l.Y_Scale = v;
			}
		}

		public string SampleWidth
		{
			get => sampleWidth.ToString();
			set
			{
				double v;
				if (!double.TryParse(value, out v)) return;

				sampleWidth = v;
				foreach (var l in LineParts)
				{
					l.SampleWidth = v;
				}

				if (LineParts.Count == 0) return;

				TextBlock tb2 = new TextBlock() { Text = LineParts[0].S2_Str };

				tb2.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
				double w = tb2.DesiredSize.Width;

				if (w > v)
				{
					int k = (int)(w / v) + 1;

					for (int i = 0; i < LineParts.Count; i++)
					{
						if (i % k == 0)
						{
							LineParts[i].T2_Visibility = Visibility.Visible;
							LineParts[i].TimeLinePartWidth = v * k;
						}
						else
						{
							LineParts[i].T2_Visibility = Visibility.Collapsed;
							LineParts[i].TimeLinePartWidth = 0.0;
						}
					}
				}
			}
		}

		public SignalData Instance { get; private set; }

		public string SizeInfo => $"Размер {Instance.Data.Length} байт";

		public List<PlotLinePart> LineParts { get; set; }


		public event PropertyChangedEventHandler PropertyChanged;

		private double sampleWidth;
		private double y_Scale;
	}
}
