﻿using PPG_Database.KeepingModels;
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
	public class PlotDataKeeper : INotifyPropertyChanged
	{
		public PlotDataKeeper(SignalData signalData, double fs, Brush plotBrush)
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
		public string ValueUnitName => Instance.SignalChannel.ValueUnitName;
		public double PlotOpacity => IsSelected ? 1.0 : 0.5;
		public bool IsSelected
		{
			get => isSelected;
			set
			{
				isSelected = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PlotOpacity)));
			}
		}
		public double ScaleY
		{
			get => SignalChannel.GetPlotScaleY(Instance.SignalChannel);
			set => SignalChannel.SetPlotScaleY(Instance.SignalChannel, value);
		}

		public double Max_Y => Math.Max(0, OriginalPoints.Max(p => p.Y));

		public event PropertyChangedEventHandler PropertyChanged;


		private Brush plotBrush;
		private PointCollection points;
		private bool isSelected = false;
	}
}
