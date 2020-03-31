using MDP_PPG.EntitiesEditing;
using MDP_PPG.SignalAnalisys;
using MDP_PPG.ViewModels;
using PPG_Database;
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
				"Записи сигналов",
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
				query => query.Include(r => r.SignalDatas.Select(sd => sd.SignalChannel)));

			DataContext = this;
		}

		public PagedView<Recording> PagedView { get; set; }

		private void Btn_AddRec_Click(object sender, RoutedEventArgs e)
		{
			var dlg = new W_EditRecordings(0, PagedView.ParentItem.Id);
			dlg.ShowDialog();
			PagedView.OnItemAdding();
		}

		private void ListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			var dlg = new W_EditRecordings(PagedView.SelectedItem.Instance.Id, PagedView.ParentItem.Id);
			dlg.ShowDialog();
			PagedView.OnItemEditing();
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
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BtnAnalyseIsEnabled)));
		}

		public bool BtnAnalyseIsEnabled => PagedView.SelectedItem?.Instance != null;
		private void Btn_Analisys_Click(object sender, RoutedEventArgs e)
		{
			var dlg = new W_SignalAnalyser(PagedView.SelectedItem.Instance);
			dlg.ShowDialog();
		}

		private void Btn_MC_Settings_Click(object sender, RoutedEventArgs e)
		{
			var dlg = new W_MC_Settings();
			dlg.ShowDialog();
		}
	}
}
