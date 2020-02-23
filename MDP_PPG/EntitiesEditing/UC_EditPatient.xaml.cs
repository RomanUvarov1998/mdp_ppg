using PPG_Database.KeepingModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

namespace MDP_PPG.EntitiesEditing
{
	/// <summary>
	/// Логика взаимодействия для UC_EditPatient.xaml
	/// </summary>
	public partial class UC_EditPatient : UserControl, INotifyPropertyChanged
	{
		public UC_EditPatient()
		{
			InitializeComponent();

			DataContext = this;
		}

		public void SetInputControls(
			Patient patient,
			Action notifyCheckValidation)
		{
			NotifyCheckValidation = notifyCheckValidation;

			bool patientExists = patient.Id != 0;

			vtbSurname.SetValues(
				patient.Surname,
				ValidateSurname,
				CoerseFIO,
				NotifyCheckValidation);
			vtbName.SetValues(
				patient.Name,
				ValidateName,
				CoerseFIO,
				NotifyCheckValidation);
			vtbPatronymic.SetValues(
				patient.Patronimyc,
				ValidatePatronymic,
				CoerseFIO,
				NotifyCheckValidation);

			IdValue = patient.Id;

			NotifyCheckValidation();

			vtbSurname.tb.Focus();
		}

		public Patient GetPatient() => new Patient(
																			vtbSurname.MyText,
																			vtbName.MyText,
																			vtbPatronymic.MyText,
																			IdValue);

		#region inputValidation
		public bool BtnOkActive => vtbSurname.AllowsSavingInput &&
																vtbName.AllowsSavingInput &&
																vtbPatronymic.AllowsSavingInput;

		private Action NotifyCheckValidation;

		private int IdValue = 0;

		private Regex regexName = new Regex("^([А-Яа-яA-Za-z]{0,40})$", RegexOptions.Compiled);
		private bool ValidateName(string value)
		{
			return regexName.IsMatch(value);
		}
		private Regex regexSurname = new Regex("^([А-Яа-яA-Za-z]{1,40})$", RegexOptions.Compiled);
		private bool ValidateSurname(string value)
		{
			return regexSurname.IsMatch(value);
		}
		private Regex regexPatronymic = new Regex("^([А-Яа-яA-Za-z]{0,40})$", RegexOptions.Compiled);
		private bool ValidatePatronymic(string value)
		{
			return regexPatronymic.IsMatch(value);
		}

		private string CoerseFIO(string value)
		{
			string result = string.Empty;

			if (value.Length > 0)
				result = value[0].ToString().ToUpper();

			if (value.Length > 1)
				result += value.Substring(1).ToLower();

			return result;
		}		
		#endregion


		public event PropertyChangedEventHandler PropertyChanged;

		public void AutoFill()
		{
			vtbSurname.MyText = "Иванов";
			vtbName.MyText = "Иван";
			vtbPatronymic.MyText = "Иванович";
		}
	}
}
