using MDP_PPG.PagedViews;
using MDP_PPG.ViewModels;
using System;
using System.Collections.Generic;
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

namespace MDP_PPG
{
	/// <summary>
	/// Логика взаимодействия для MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();

			DataContext = this;

			pvPatients.PagedView.DebugEntityName = "П";
			pvRecordings.PagedView.DebugEntityName = "З";

			IManagablePagedView pat_PV = pvPatients.PagedView;
			IManagablePagedView rec_PV = pvRecordings.PagedView;

			pvPatients.PagedView.OnItemAdding = async delegate
			{
				pat_PV.Freeze();
				rec_PV.Freeze();
				pvSignalPlot.Freeze();

				await pat_PV.RecountPages_GoToFirstPage(null, true);
				await rec_PV.RecountPages_GoToFirstPage(pat_PV.GetSelectedItem, false);
				await pvSignalPlot.LoadData(rec_PV.GetSelectedItem);
			};
			pvPatients.PagedView.OnItemEditing = async delegate
			{
				pat_PV.Freeze();

				await pat_PV.RefreshSelectedItem();
			};
			pvPatients.PagedView.OnItemDeleting = async delegate
			{
				pat_PV.Freeze();
				rec_PV.Freeze();
				pvSignalPlot.Freeze();

				await pat_PV.RecountPages_RefreshCurrentPage(null);
				await rec_PV.RecountPages_GoToFirstPage(pat_PV.GetSelectedItem, false);
				await pvSignalPlot.LoadData(rec_PV.GetSelectedItem);
			};
			pvPatients.PagedView.OnPageChanging = async delegate
			{
				pat_PV.Freeze();
				rec_PV.Freeze();
				pvSignalPlot.Freeze();

				await pat_PV.LoadPageOnPageNumChanged(true);
				await rec_PV.RecountPages_GoToFirstPage(pat_PV.GetSelectedItem, false);
				await pvSignalPlot.LoadData(rec_PV.GetSelectedItem);
			};
			pvPatients.PagedView.OnFilterChanging = async delegate
			{
				pat_PV.Freeze();
				rec_PV.Freeze();
				pvSignalPlot.Freeze();

				await pat_PV.RecountPages_GoToFirstPage(null, true);
				await rec_PV.RecountPages_GoToFirstPage(pat_PV.GetSelectedItem, false);
				await pvSignalPlot.LoadData(rec_PV.GetSelectedItem);
			};
			pvPatients.PagedView.OnSelectionChanged = async delegate
			{
				rec_PV.Freeze();
				pvSignalPlot.Freeze();

				await rec_PV.RecountPages_GoToFirstPage(pat_PV.GetSelectedItem, false);
				await pvSignalPlot.LoadData(rec_PV.GetSelectedItem);
			};


			pvRecordings.PagedView.OnItemAdding = async delegate
			{
				pat_PV.Freeze();
				rec_PV.Freeze();
				pvSignalPlot.Freeze();

				await pat_PV.RefreshSelectedItem();
				await rec_PV.RecountPages_GoToFirstPage(pat_PV.GetSelectedItem, false);
				await pvSignalPlot.LoadData(rec_PV.GetSelectedItem);
			};
			pvRecordings.PagedView.OnItemEditing = async delegate
			{
				pat_PV.Freeze();
				rec_PV.Freeze();
				pvSignalPlot.Freeze();

				await pat_PV.RefreshSelectedItem();
				await rec_PV.RefreshSelectedItem();
				await pvSignalPlot.LoadData(rec_PV.GetSelectedItem);
			};
			pvRecordings.PagedView.OnItemDeleting = async delegate
			{
				pat_PV.Freeze();
				rec_PV.Freeze();
				pvSignalPlot.Freeze();

				await pat_PV.RefreshSelectedItem();
				await rec_PV.RecountPages_RefreshCurrentPage(pat_PV.GetSelectedItem);
				await pvSignalPlot.LoadData(rec_PV.GetSelectedItem);
			};
			pvRecordings.PagedView.OnPageChanging = async delegate
			{
				rec_PV.Freeze();
				pvSignalPlot.Freeze();

				await rec_PV.LoadPageOnPageNumChanged(false);
				await pvSignalPlot.LoadData(rec_PV.GetSelectedItem);
			};
			pvRecordings.PagedView.OnFilterChanging = async delegate
			{
				rec_PV.Freeze();
				pvSignalPlot.Freeze();

				await rec_PV.RecountPages_GoToFirstPage(pat_PV.GetSelectedItem, false);
				await pvSignalPlot.LoadData(rec_PV.GetSelectedItem);
			};
			pvRecordings.PagedView.OnSelectionChanged = async delegate
			{
				pvSignalPlot.Freeze();

				await pvSignalPlot.LoadData(rec_PV.GetSelectedItem);
			};

			pvPatients.DoFilteredSearch();
		}

		private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			pvSignalPlot.OnWindowResized();
		}
	}
}
