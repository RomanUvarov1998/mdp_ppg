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
using System.Windows.Navigation;
using System.Windows.Shapes;
using MDP_PPG.ViewModels;
using System.Linq.Expressions;
using System.Data.Entity;
using PPG_Database;
using MDP_PPG.EntitiesEditing;

namespace MDP_PPG.PagedViews
{
	/// <summary>
	/// Логика взаимодействия для UC_Patients.xaml
	/// </summary>
	public partial class UC_Patients : UserControl, INotifyPropertyChanged, ICanDoFilteredSearch
	{
		public UC_Patients()
		{
			InitializeComponent();

			PagedView = new PagedView<Patient>(
				RefreshButtonsIsEnabled,
				 "Пациенты",
				 model => new PatientTVM(model),
				 parent => (pat => true),
				 query => query.OrderByDescending(p => p.Id),
				 curItem => (item => item.Id >= curItem.Id),
				 query => query.Include(p => p.Recordings));

			uC_SearfhStringFields = new List<UC_SearchStringField>()
			{
				sfName,
				sfSurname,
				sfPatronimyc,
			};

			foreach (var sf in uC_SearfhStringFields)
				sf.ValueUserThatCanSearch = this;

			DataContext = this;
		}


		private List<UC_SearchStringField> uC_SearfhStringFields;


		public void DoFilteredSearch()
		{
			var expressions = new List<Expression<Func<Patient, bool>>>();

			if (!string.IsNullOrWhiteSpace(sfName.InputText))
				expressions.Add(
					p => p.Name.ToLower().StartsWith(sfName.InputText.ToLower())
				);

			if (!string.IsNullOrWhiteSpace(sfSurname.InputText))
				expressions.Add(
					p => p.Surname.ToLower().StartsWith(sfSurname.InputText.ToLower())
				);

			if (!string.IsNullOrWhiteSpace(sfPatronimyc.InputText))
				expressions.Add(
					p => p.Patronimyc.ToLower().StartsWith(sfPatronimyc.InputText.ToLower())
				);

			Console.WriteLine("Not BUSY, search");

			PagedView.SetFilter(expressions);
		}


		public PagedView<Patient> PagedView { get; set; }


		private async void Lw_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			var context = new PPG_Context();

			var dlg = new W_EditPatient(PagedView.SelectedItem.Instance);
			bool? retVal = dlg.ShowDialog();

			if (retVal != true)
			{
				context.Dispose();
				return;
			}

			await PagedView.EditItemAsync(dlg.GetPatient(), context);

			context.Dispose();
		}
		private async void BtnAddPatient_Click(object sender, RoutedEventArgs e)
		{
			var context = new PPG_Context();

			Patient patientScaffold = new Patient(
				sfSurname.InputText,
				sfName.InputText,
				sfPatronimyc.InputText,
				0);

			var dlg = new W_EditPatient(patientScaffold);
			bool? retVal = dlg.ShowDialog();

			if (retVal != true)
			{
				context.Dispose();
				return;
			}

			Patient newPatientCompleted = dlg.GetPatient();

			await PagedView.AddItemAsync(newPatientCompleted, context);

			context.Dispose();
		}
		private async void BtnDeleteSelection_Click(object sender, RoutedEventArgs e)
		{
			MessageBoxResult res = MessageBox.Show($"Вы действительно хотите удалить выбранного пациента?", "Подтверждение удаления",
							MessageBoxButton.YesNo, MessageBoxImage.Exclamation);

			if (res != MessageBoxResult.Yes) return;

			await PagedView.DeleteItemAsync(PagedView.SelectedItem.Instance);
		}

		private void BtnClearFields_Click(object sender, RoutedEventArgs e)
		{
			foreach (var sf in uC_SearfhStringFields)
				sf.InputText = string.Empty;

			DoFilteredSearch();
		}

		private void BtnPageBack_Click(object sender, RoutedEventArgs e)
		{
			PagedView.GoToPrevPage();
		}
		private void BtnPageNext_Click(object sender, RoutedEventArgs e)
		{
			PagedView.GoToNextPage();
		}

		private void TbPageNum_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.Key != Key.Enter)
				return;

			var textBox = sender as TextBox;
			if (textBox == null) return;

			BindingOperations.GetBindingExpression(textBox, TextBox.TextProperty)?.UpdateSource();
		}

		private void Btn_Add10RandPats_Click(object sender, RoutedEventArgs e)
		{
			//Random rndgen = new Random();
			//string s = "йцукенгшщзхъфывапролджэячсмитьбю";
			//string s2 = "1234567890-/";
			//string s3 = "1234567890";
			//char[] letters = s.ToCharArray();
			//char[] letters2 = s2.ToCharArray();
			//char[] letters3 = s3.ToCharArray();

			//using (var context = new PPG_Context())
			//{
			//	int count = 1000;
			//	for (int i = 0; i < count; i++)
			//	{
			//		Patient pat = new Patient();

			//		pat.Name = "";
			//		pat.Surname = "";
			//		pat.Patronymic = "";
			//		pat.Branch = "";
			//		pat.Post = "";
			//		pat.WeightGr = (short)rndgen.Next(4000);
			//		pat.Hist = i.ToString("D10");

			//		for (int j = 0; j < 20; j++)
			//		{
			//			pat.Name += letters[rndgen.Next(letters.Length)].ToString();
			//			pat.Surname += letters[rndgen.Next(letters.Length)].ToString();
			//			pat.Patronymic += letters[rndgen.Next(letters.Length)].ToString();

			//			if (j == 0)
			//			{
			//				pat.Name = pat.Name.ToUpper();
			//				pat.Surname = pat.Surname.ToUpper();
			//				pat.Patronymic = pat.Patronymic.ToUpper();
			//			}
			//		}

			//		for (int j = 0; j < 10; j++)
			//		{
			//			pat.Branch += letters2[rndgen.Next(letters2.Length)].ToString();
			//			pat.Post += letters2[rndgen.Next(letters2.Length)].ToString();
			//		}

			//		//DateTime start = DateTime.Today.Date.AddDays(-7 * 8);
			//		//int range = (DateTime.Today - start).Days;
			//		pat.Birthday = DateTime.Today.Date.AddDays(-rndgen.Next(7 * 2));//start.AddDays(rndgen.Next(7 * 6));

			//		pat.GestationalAge = (byte)(15 + rndgen.Next(19));

			//		//pat.Sex = rndgen.NextDouble() < 0.5 ? ESex.Мужской : ESex.Женский;
			//		pat.Sex = "";

			//		pat.State = EStates.Открыт;

			//		context.Patients.Add(pat);

			//		Console.WriteLine($"{i * 100 / count}% completed");
			//	}

			//	try
			//	{
			//		context.SaveChanges();
			//	}
			//	catch (Exception exception)
			//	{
			//		MessageBox.Show(exception.Message);
			//		return;
			//	}
			//}

			//ReloadPage_OnFilterChangedAsync();
		}


		public event PropertyChangedEventHandler PropertyChanged;

		private void RefreshButtonsIsEnabled()
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BtnDelPatEnabled)));
		}

		public bool BtnDelPatEnabled => PagedView.SelectedItem != null;

		public bool IsLoadingData
		{
			get => _isLoadingData;
			set
			{
				_isLoadingData = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsLoadingData)));
			}
		}

		private bool _isLoadingData = true;
	}
}
