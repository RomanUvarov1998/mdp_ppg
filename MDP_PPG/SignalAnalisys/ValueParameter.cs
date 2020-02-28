using System.ComponentModel;
using System.Windows.Media;

namespace MDP_PPG.SignalAnalisys
{
	public class ValueParameter : INotifyPropertyChanged
	{
		public ValueParameter(string parName)
		{
			ParName = parName;
		}

		public string ParName
		{
			get => parName;
			set
			{
				parName = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ParName)));
			}
		}
		public string ParValueStr
		{
			get => parValueStr.ToString();
			set
			{
				double v;
				IsValid = double.TryParse(value, out v);
				if (IsValid)
				{
					ParValue = v;
				}

				ValueKeeper.SetCountBtnEnabled();

				parValueStr = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ParValueStr)));
			}
		}
		public bool IsValid
		{
			get => isValid;
			set
			{
				isValid = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsValid)));
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TextFieldBrush)));
			}
		}
		public Brush TextFieldBrush => IsValid ? Brushes.Black : Brushes.OrangeRed;

		public double ParValue = 0.0;
		public IValueCounter ValueKeeper;

		public event PropertyChangedEventHandler PropertyChanged;

		private string parName;
		private string parValueStr = "0,0";
		private bool isValid = true;
	}
}
