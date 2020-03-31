using MDP_PPG.ViewModels;
using PPG_Database.KeepingModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MDP_PPG.SignalAnalisys
{
	/// <summary>
	/// Логика взаимодействия для W_SignalAnalyser.xaml
	/// </summary>
	public partial class W_SignalAnalyser : Window, INotifyPropertyChanged
	{
		public W_SignalAnalyser(Recording recording)
		{
			InitializeComponent();

			DataContext = this;

			pvSignalPlot.PixelsPerDip = VisualTreeHelper.GetDpi(this).PixelsPerDip;
			pvSignalPlot.GiveData(recording);

			analyser.GiveData(recording);
		}

		public event PropertyChangedEventHandler PropertyChanged;

		private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			pvSignalPlot.OnWindowResized();
		}
	}
}
