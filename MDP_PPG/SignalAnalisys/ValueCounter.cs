using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace MDP_PPG.SignalAnalisys
{
	public class ValueCounter : INotifyPropertyChanged, IValueCounter
	{
		public ValueCounter(
			string valueName,
			Func<List<ValueParameter>, string> valueCounterFcn,
			List<ValueParameter> parameters)
		{
			ValueName = valueName;
			ValueCounterFcn = valueCounterFcn;
			Parameters = parameters;

			foreach (var par in Parameters)
				par.ValueKeeper = this;
		}

		public bool IsCountingValue
		{
			get => isCountingValue;
			set
			{
				isCountingValue = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsCountingValue)));
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PreloadingVisibility)));
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ResultVisibility)));
			}
		}
		public Visibility PreloadingVisibility => IsCountingValue ? Visibility.Visible : Visibility.Collapsed;
		public Visibility ResultVisibility => !IsCountingValue ? Visibility.Visible : Visibility.Collapsed;
		public string ValueName
		{
			get => valueName;
			set
			{
				valueName = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ValueName)));
			}
		}
		public string CountedValue
		{
			get => countedValue;
			set
			{
				countedValue = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CountedValue)));
			}
		}
		public List<ValueParameter> Parameters
		{
			get => parameters;
			set
			{
				parameters = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Parameters)));
			}
		}
		public bool CountBtnIsEnabled
		{
			get => countBtnIsEnabled;
			set
			{
				countBtnIsEnabled = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CountBtnIsEnabled)));
			}
		}

		private Func<List<ValueParameter>, string> ValueCounterFcn;

		public async void TryCountValue()
		{
			IsCountingValue = true;
			CountBtnIsEnabled = false;

			CountedValue = await Task.Run(() => ValueCounterFcn(Parameters));

			CountBtnIsEnabled = true;
			IsCountingValue = false;
		}

		public void SetCountBtnEnabled()
		{
			bool allParsAreValid = true;

			foreach (var par in Parameters)
			{
				if (!par.IsValid)
				{
					allParsAreValid = false;
					break;
				}
			}

			CountBtnIsEnabled = allParsAreValid;
		}

		public event PropertyChangedEventHandler PropertyChanged;

		private string valueName;
		private string countedValue;
		private List<ValueParameter> parameters;
		private bool countBtnIsEnabled = true;
		private bool isCountingValue;
	}
}
