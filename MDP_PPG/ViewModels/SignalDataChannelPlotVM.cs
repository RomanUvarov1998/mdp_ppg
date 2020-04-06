using PPG_Database.KeepingModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using MDP_PPG.Helpers;
using System.Windows.Media;
using System.ComponentModel;

namespace MDP_PPG.ViewModels
{
	public class SignalDataChannelPlotVM : INotifyPropertyChanged
	{
		public SignalDataChannelPlotVM(SignalData signalData, double fs, Brush plotBrush)
		{
			Instance = signalData;
			Values = MainFunctions.FromDatabaseToAnalysis(Instance.Data);
			OriginalPoints = Values
				.Select((v, index) => new Point(index * (1 / fs), v))
				.ToArray();
			PlotBrush = plotBrush;
		}

		public SignalData Instance;
		public double[] Values;
		public Point[] OriginalPoints;
		public PlotDataPoint[] DrawnPoints = new PlotDataPoint[0];


		public PointCollection Points
		{
			get => points;
			set
			{
				points = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Points)));
			}
		}
		public Brush PlotBrush
		{
			get => plotBrush;
			set
			{
				plotBrush = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PlotBrush)));
			}
		}
		public string ChannelName => Instance.SignalChannel.Name;
		public string ChannelCode => Instance.SignalChannel.ChannelCode.ToString();


		public event PropertyChangedEventHandler PropertyChanged;


		private Brush plotBrush;
		private PointCollection points;
	}
}
