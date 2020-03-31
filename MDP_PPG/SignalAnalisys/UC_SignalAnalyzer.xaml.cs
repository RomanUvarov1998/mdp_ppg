using PPG_Database.KeepingModels;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace MDP_PPG.SignalAnalisys
{
	/// <summary>
	/// Логика взаимодействия для UC_SignalAnalyzer.xaml
	/// </summary>
	public partial class UC_SignalAnalyzer : UserControl, INotifyPropertyChanged
	{
		public UC_SignalAnalyzer()
		{
			InitializeComponent();

			DataContext = this;			
		}

		public void GiveData(Recording recording)
		{
			Recording = recording;

			ValueCounters = new List<ValueCounter>()
			{
				new ValueCounter(
					"Значение 1",
					Value1Counter,
					new List<ValueParameter>()
					{
						new ValueParameter("Параметр 1"),
						new ValueParameter("Параметр 2"),
						new ValueParameter("Параметр 3")
					}),
				new ValueCounter(
					"Значение 2",
					Value2Counter,
					new List<ValueParameter>()
					{
						new ValueParameter("Параметр a"),
						new ValueParameter("Параметр b"),
						new ValueParameter("Параметр c")
					})
			};
		}

		private string Value1Counter(List<ValueParameter> pars)
		{
			return pars.Select(par => par.ParValue).Sum().ToString("G4");
		}
		private string Value2Counter(List<ValueParameter> pars)
		{
			return pars.Select(par => par.ParValue).Min().ToString("G4");
		}

		public List<ValueCounter> ValueCounters
		{
			get => valueCounters;
			set
			{
				valueCounters = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ValueCounters)));
			}
		}


		private Recording Recording;

		private void Btn_CountValue_Click(object sender, RoutedEventArgs e)
		{
			Button btn = (Button)sender;
			ValueCounter counter = (ValueCounter)btn.DataContext;
			counter.TryCountValue();
		}

		private void Btn_CountAll_Click_1(object sender, RoutedEventArgs e)
		{
			foreach (var vc in ValueCounters)
				vc.TryCountValue();
		}

		public event PropertyChangedEventHandler PropertyChanged;


		private List<ValueCounter> valueCounters;
	}
}
