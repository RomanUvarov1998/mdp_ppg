using PPG_Database;
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

namespace MDP_PPG.EntitiesEditing
{
	/// <summary>
	/// Логика взаимодействия для W_EditPatient.xaml
	/// </summary>
	public partial class W_EditPatient : Window, INotifyPropertyChanged
	{
		public W_EditPatient(Patient patient)
		{
			InitializeComponent();

			uc.SetInputControls(patient, CheckValidation);

			DataContext = this;
		}



		public Patient GetPatient() => uc.GetPatient();
		private void ButtonOk_OnClick(object _sender, RoutedEventArgs _e)
		{
			DialogResult = true;
		}
		private void ButtonCancel_OnClick(object _sender, RoutedEventArgs _e)
		{
			DialogResult = false;
		}
		private void Window_Closing(object sender, CancelEventArgs e)
		{
			MessageBoxResult res = MessageBoxResult.None;

			if (DialogResult == null && BtnOkActive)
				res = MessageBox.Show("Сохранить изменения?", "Сохранение изменений",
					MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

			switch (res)
			{
				case MessageBoxResult.Yes:
					e.Cancel = true;
					ButtonOk_OnClick(null, null);
					break;
				case MessageBoxResult.Cancel:
					e.Cancel = true;
					return;
				case MessageBoxResult.No:
					DialogResult = false;
					return;
			}
		}



		//-------------------------------------------------------------------------------------------------

		public bool BtnOkActive => uc.BtnOkActive;

		private void CheckValidation()
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BtnOkActive)));
		}

		private void Btn_AutoFill_Click(object sender, RoutedEventArgs e)
		{
			uc.AutoFill();
		}


		public event PropertyChangedEventHandler PropertyChanged;
	}
}
