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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MDP_PPG.PagedViews
{
	/// <summary>
	/// Логика взаимодействия для UC_SearchStringField.xaml
	/// </summary>
	public partial class UC_SearchStringField : UserControl, INotifyPropertyChanged
	{
		public UC_SearchStringField()
		{
			InitializeComponent();

			DataContext = this;
		}


		public event PropertyChangedEventHandler PropertyChanged;


		public ICanDoFilteredSearch ValueUserThatCanSearch;

		public string InputText
		{
			get => _inputText;
			set
			{
				_inputText = value;
				ValueUserThatCanSearch?.DoFilteredSearch();
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(InputText)));
			}
		}

		public string PropUIName
		{
			get => _propUIName;
			set
			{
				_propUIName = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PropUIName)));
			}
		}


		private string _propUIName;
		private string _inputText = string.Empty;
	}
}
