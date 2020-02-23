using MDP_PPG.EntitiesEditing;
using MDP_PPG.ViewModels;
using PPG_Database.KeepingModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace MDP_PPG.PagedViews
{
	/// <summary>
	/// Логика взаимодействия для UC_Recordings.xaml
	/// </summary>
	public partial class UC_Recordings : UserControl, INotifyPropertyChanged
	{
		public UC_Recordings()
		{
			InitializeComponent();

			PagedView = new PagedView<Recording>(
				RefreshButtonsIsEnabled,
				"Периоды обследований",
				model => new RecordingTVM(model),
				delegate (ModelBase pat)
				{
					if (pat == null)
						return rec => false;
					else
						return rec => rec.PatientId == pat.Id;
				},
				query => query.OrderByDescending(rec => rec.CreatedDate),
				curItem => (item => DbFunctions.DiffDays(item.CreatedDate, curItem.CreatedDate) <= 0),
				query => query.Include(r => r.SignalData));

			DataContext = this;
		}

		public PagedView<Recording> PagedView { get; set; }

		private void Btn_AddRec_Click(object sender, RoutedEventArgs e)
		{
			var dlg = new W_EditRecordings(new Recording(PagedView.ParentItem.Id));

			var res = dlg.ShowDialog();

			if (!res.GetValueOrDefault()) return;

			PagedView.AddItemAsync(dlg.GetRecording());
		}

		private void ListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			var dlg = new W_EditRecordings(PagedView.SelectedItem.Instance);

			var res = dlg.ShowDialog();

			if (!res.GetValueOrDefault()) return;

			PagedView.EditItemAsync(dlg.GetRecording());
		}

		private void Btn_DelRec_Click(object sender, RoutedEventArgs e)
		{
			var res = MessageBox.Show("Вы точно собираетесь удалить данную запись?", "Удаление записи", MessageBoxButton.YesNo, MessageBoxImage.Question);

			if (res != MessageBoxResult.Yes) return;

			PagedView.DeleteItemAsync(PagedView.SelectedItem.Instance);
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


		public event PropertyChangedEventHandler PropertyChanged;

		public void RefreshButtonsIsEnabled()
		{
			PropertyChanged?.Invoke(PagedView, new PropertyChangedEventArgs(nameof(PagedView.CanGoPrevPage)));
			PropertyChanged?.Invoke(PagedView, new PropertyChangedEventArgs(nameof(PagedView.CanGoNextPage)));
		}
	}
}
