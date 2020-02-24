using MDP_PPG.ViewModels;
using PPG_Database;
using PPG_Database.KeepingModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace MDP_PPG.PagedViews
{
	public class PagedView<TItem> : INotifyPropertyChanged, IManagablePagedView
		where TItem : ModelBase
	{
		public PagedView(
			Action refreshButtonsIsEnabled,
			string uiEntityName,
			Func<TItem, IViewModel<TItem>> getView,
			Func<ModelBase, Expression<Func<TItem, bool>>> parentFilterGenerator,
			Func<IQueryable<TItem>, IOrderedQueryable<TItem>> addOrderByExpression,
			Func<TItem, Expression<Func<TItem, bool>>> isBeforeOrCurrent, // for global num searching
			Func<IQueryable<TItem>, IQueryable<TItem>> alwaysInclude)
		{
			RefreshButtonsIsEnabled = refreshButtonsIsEnabled;

			UIEntityName = uiEntityName;
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(UIEntityName)));

			DebugEntityName = UIEntityName;

			GetParentFilter = parentFilterGenerator;

			GetView = getView;

			AddOrderByExpression = addOrderByExpression;

			IsBeforeOrCurrent = isBeforeOrCurrent;

			AlwaysInclude = alwaysInclude;
		}


		//------------------------------- GUI Properties ---------------------------------------	
		#region guiProps
		public string PageNumStr
		{
			get => _pageNum.ToString();
			set
			{
				if (int.TryParse(value, out int intValue))
				{
					if (intValue == _pageNum) return;

					intValue = Math.Min(PagesCount, intValue);
					intValue = Math.Max(1, intValue);

					_pageNum = intValue;
					OnPageChanging?.Invoke();
				}

				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PageNumStr)));
				RefreshButtonsIsEnabled?.Invoke();
			}
		}
		public bool IsLoadingData => _isLoadingData;
		public List<IViewModel<TItem>> ItemsList => _itemsList;
		public IViewModel<TItem> SelectedItem
		{
			get => _selectedItem;
			set
			{
				_selectedItem = value;
				MyLog($"Selected new ITEM, {(_selectedItem == null ? "NULL" : "NOT_NULL")}");

				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedItem)));
				RefreshButtonsIsEnabled?.Invoke();

				OnSelectionChanged?.Invoke();
			}
		}
		public int ItemsListCount => ItemsList.Count;
		public string UIEntityName { get; }
		#endregion

		//------------------------------- CRUD API -----------------------------------------------
		#region crudApi
		public async Task AddItemAsync(TItem item, PPG_Context context)
		{
			context.Set<TItem>().Add(item);

			try
			{
				await context.SaveChangesAsync();
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, $"While adding {DebugEntityName}");
			}
			finally
			{
				OnItemAdding?.Invoke();
			}
		}
		public async Task AddItemsRangeAsync(IEnumerable<TItem> items, PPG_Context context)
		{
			context.Set<TItem>().AddRange(items);

			try
			{
				await context.SaveChangesAsync();
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, $"While adding {DebugEntityName}");
			}
			finally
			{
				OnItemAdding?.Invoke();
			}
		}
		public async Task EditItemAsync(TItem newItem, PPG_Context context)
		{
			int count = context.Set<TItem>()
				.Count(it => it.Id == newItem.Id);

			if (count == 0)
			{
				MessageBox.Show($"Сущность {DebugEntityName} не " +
					$"найдена в базе, редактирование не удалось(((");
				return;
			}

			context.Entry<TItem>(newItem).State = newItem.Id == 0 ? EntityState.Added : EntityState.Modified;

			try
			{
				await context.SaveChangesAsync();
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, $"While editing {DebugEntityName}");
			}
			finally
			{
				OnItemEditing?.Invoke();
				//await RefreshItem(SelectedItem.Instance.Id);
				//await Refresh_Parent_ParentBrothers_MyBrothers();
			}
		}
		public async Task DeleteItemAsync(TItem newItem, PPG_Context context)
		{
			TItem oldItem = context.Set<TItem>()
				.FirstOrDefault(it => it.Id == newItem.Id);

			if (oldItem == null)
			{
				MessageBox.Show($"Сущность {DebugEntityName} не " +
					$"найдена в базе, удаление не удалось(((");
				return;
			}

			context.Entry<TItem>(oldItem).State = oldItem.Id == 0 ? EntityState.Detached : EntityState.Deleted;

			try
			{
				await context.SaveChangesAsync();
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, $"While deleting {DebugEntityName}");
			}
			finally
			{
				OnItemDeleting?.Invoke();
			}
		}



		public async Task AddItemAsync(TItem newItem)
		{
			using (var context = new PPG_Context())
			{
				await AddItemAsync(newItem, context);
			}
		}
		public async Task EditItemAsync(TItem updatedItem)
		{
			using (var context = new PPG_Context())
			{
				await EditItemAsync(updatedItem, context);
			}
		}
		public async Task DeleteItemAsync(TItem deletingItem)
		{
			using (var context = new PPG_Context())
			{
				await DeleteItemAsync(deletingItem, context);
			}
		}


		public void SetFilter(List<Expression<Func<TItem, bool>>> newWhereFilterExpressions)
		{
			if (newWhereFilterExpressions != null)
				WhereFilterExpressions = newWhereFilterExpressions;

			OnFilterChanging?.Invoke();
			NotifyBindedPropsChanged();
		}

		public void GoToNextPage()
		{
			_pageNum++;
			OnPageChanging?.Invoke();
		}
		public void GoToPrevPage()
		{
			_pageNum--;
			OnPageChanging?.Invoke();
		}



		public Action OnFilterChanging;
		public Action OnPageChanging;
		public Action OnSelectionChanged;

		public Action OnItemEditing;
		public Action OnItemAdding;
		public Action OnItemDeleting;
		#endregion


		//------------------------------- ?????? -----------------------------------------------


		// if changed, loads new data, if new parent the same, refresh page
		public ModelBase ParentItem => _parentItem;
		private void NotifyBindedPropsChanged()
		{
			MyLog("Refresh buttons )))))))))))))))))))))))");

			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedItem)));

			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ItemsList)));

			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ItemsListCount)));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanGoNextPage)));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanGoPrevPage)));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ParentItem)));

			RefreshButtonsIsEnabled?.Invoke();
		}
		private Action RefreshButtonsIsEnabled { get; set; }


		//---------------------------- Paging --------------------------------------------------------
		#region paging
		private void TbPageNum_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.Key != Key.Enter)
				return;

			var textBox = sender as TextBox;
			if (textBox == null) return;

			BindingOperations.GetBindingExpression(textBox, TextBox.TextProperty)?.UpdateSource();
		}

		//private int PageNum
		//{
		//	get => _pageNum;
		//	set
		//	{
		//		_pageNum = value;

		//		if (_pageNum > PagesCount)
		//			_pageNum = PagesCount;

		//		if (_pageNum < 1)
		//			_pageNum = 1;
		//	}
		//}
		public int PagesCount { get; set; }
		public bool CanGoNextPage => _pageNum < PagesCount;
		public bool CanGoPrevPage => _pageNum > 1;

		private int ROWS_PER_PAGE_COUNT = 20;
		#endregion

		//---------------------------- IManagablePagedView --------------------------------------------------------
		#region IManagablePagedView
		public void Freeze()
		{
			_isLoadingData = true;
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsLoadingData)));
			RefreshButtonsIsEnabled?.Invoke();
		}
		public async Task RecountPages_GoToFirstPage(ModelBase updatedParent, bool selectFirstItemIfExists)
		{
			_parentItem = updatedParent;
			WhereParentExpression = GetParentFilter(_parentItem);
			await RecountPages();
			_pageNum = 1;
			await LoadPageOnPageNumChanged(selectFirstItemIfExists);
			_isLoadingData = false;

			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsLoadingData)));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedItem)));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ItemsList)));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ItemsListCount)));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ParentItem)));
			RefreshButtonsIsEnabled?.Invoke();
		}
		public async Task RecountPages_RefreshCurrentPage(ModelBase updatedParent)
		{
			_parentItem = updatedParent ?? null;
			if (_parentItem != null) WhereParentExpression = GetParentFilter(_parentItem);

			await RecountPages();

			// if any item was selected, save it's id
			long selId = 0;
			if (_selectedItem != null)
			{
				selId = _selectedItem.Instance.Id;

				MyLog("RefreshFromOutside Saving selected item");

				long? globalSelItemInd = await Task.Run(() => GetGlobalItemNum(selId));

				MyLog($"RefreshFromOutside Got global selected item num, it is { (globalSelItemInd == null ? "null" : globalSelItemInd.Value.ToString()) }");

				// if selected item wasn't deleted from DB
				if (globalSelItemInd != null)
				{
					// count new PageNum
					_pageNum = ((int)(globalSelItemInd.Value / ROWS_PER_PAGE_COUNT));

					if (globalSelItemInd % ROWS_PER_PAGE_COUNT > 0) _pageNum++;
				}
			}

			_itemsList = await Task.Run(() => GetItems());
			_selectedItem = ItemsList.FirstOrDefault(item => item.Instance.Id == selId);
			_isLoadingData = false;

			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsLoadingData)));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedItem)));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ItemsList)));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ItemsListCount)));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ParentItem)));
			RefreshButtonsIsEnabled?.Invoke();
		}
		public async Task RefreshSelectedItem()
		{
			if (_selectedItem.Instance == null) throw new NullReferenceException();

			MyLog($"RefreshItem itemId = {_selectedItem.Instance.Id}");

			TItem updatedItem = await Task.Run(() => GetItemById(_selectedItem.Instance.Id));

			if (updatedItem == null) throw new NullReferenceException();

			IViewModel<TItem> updatingItemViewModel = _itemsList
				.FirstOrDefault(item => item.Instance.Id == updatedItem.Id);

			if (updatingItemViewModel == null) throw new NullReferenceException();

			updatingItemViewModel.UpdateViewOnModelUpdated(updatedItem);

			_isLoadingData = false;

			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsLoadingData)));
			RefreshButtonsIsEnabled?.Invoke();
		}
		public async Task LoadPageOnPageNumChanged(bool selectFirstItemIfExists)
		{
			_itemsList = await Task.Run(() => GetItems());
			_selectedItem = selectFirstItemIfExists ? ItemsList.FirstOrDefault() : null;

			_isLoadingData = false;
			
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedItem)));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ItemsList)));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ItemsListCount)));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsLoadingData)));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ParentItem)));
			RefreshButtonsIsEnabled?.Invoke();
			//NotifyBindedPropsChanged();
		}
		public ModelBase GetSelectedItem => SelectedItem?.Instance ?? null;

		private async Task RecountPages()
		{
			int? rowsCount = await Task.Run(() => GetRowsCount());

			SetPagesCount(rowsCount.Value);

			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PageNumStr)));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PagesCount)));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanGoNextPage)));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanGoPrevPage)));
		}
		private void SetPagesCount(int rowsCount)
		{
			PagesCount = rowsCount / ROWS_PER_PAGE_COUNT +
				rowsCount % ROWS_PER_PAGE_COUNT > 0 ? 1 : 0;

			if (PagesCount < _pageNum) _pageNum = PagesCount;
		}
		#endregion

		//-------------------------------------- Query Making -----------------------------------------------------
		#region queryMaking	
		private Func<IQueryable<TItem>, IQueryable<TItem>> AlwaysInclude;
		private Expression<Func<TItem, bool>> WhereParentExpression = item => true;
		private List<Expression<Func<TItem, bool>>> WhereFilterExpressions =
			new List<Expression<Func<TItem, bool>>>();
		private Func<IQueryable<TItem>, IOrderedQueryable<TItem>> AddOrderByExpression;
		private Func<TItem, Expression<Func<TItem, bool>>> IsBeforeOrCurrent;
		private Func<ModelBase, Expression<Func<TItem, bool>>> GetParentFilter;
		private Func<TItem, IViewModel<TItem>> GetView;

		private IQueryable<TItem> GetQuery(PPG_Context context, bool include, bool where, bool parent, bool paging, bool orderBy)
		{
			IQueryable<TItem> query = context.Set<TItem>();

			if (include)
			{
				query = AlwaysInclude.Invoke(query);
			}

			if (where)
			{
				foreach (var whereFilterExpresstion in WhereFilterExpressions)
					query = query.Where(whereFilterExpresstion);
			}

			if (parent)
			{
				query = query
				.Where(WhereParentExpression);
			}

			if (orderBy)
			{
				query = AddOrderByExpression(query);
			}

			if (paging)
			{
				query = query
					.Skip((_pageNum - 1) * ROWS_PER_PAGE_COUNT)
					.Take(ROWS_PER_PAGE_COUNT);
			}

			return query;
		}

		private TItem GetItemById(long itemId)
		{
			using (var context = new PPG_Context())
			{
				IQueryable<TItem> query = GetQuery(
					context: context,
					include: true,
					where: true,
					parent: true,
					paging: false,
					orderBy: false);

				return query.FirstOrDefault(item => item.Id == itemId);
			}
		}
		private int GetRowsCount()
		{
			int rowsCount;
			using (var context = new PPG_Context())
			{
				IQueryable<TItem> query = GetQuery(
					context: context,
					include: false,
					where: true,
					parent: true,
					paging: false,
					orderBy: false);

				rowsCount = query.Count();
			}
			return rowsCount;
		}
		private List<IViewModel<TItem>> GetItems()
		{
			if (_pageNum == 0 || PagesCount == 0) return new List<IViewModel<TItem>>();

			List<TItem> mList;
			using (var context = new PPG_Context())
			{
				IQueryable<TItem> query = GetQuery(
					context: context,
					include: true,
					where: true,
					parent: true,
					paging: true,
					orderBy: true);

				mList = query.ToList();
			}

			List<IViewModel<TItem>> vList = new List<IViewModel<TItem>>();

			for (int i = 0; i < mList.Count; i++)
				vList.Add(GetView(mList[i]));

			return vList;
		}
		private long? GetGlobalItemNum(long itemId)
		{
			using (var context = new PPG_Context())
			{
				TItem item = GetItemById(itemId);

				if (item == null)
					return null;

				IQueryable<TItem> query = GetQuery(
					context: context,
					include: true,
					where: true,
					parent: true,
					paging: false,
					orderBy: false);

				return query.Where(IsBeforeOrCurrent(item)).Count();
			}
		}
		#endregion

		//-------------------------------------- Debugging --------------------------------------------------------
		#region debugging
		private void MyLog(string action)
		{
			//Console.WriteLine($"--{DebugEntityName} {action}");
		}
		public string DebugEntityName { get; set; }
		#endregion

		#region other
		public event PropertyChangedEventHandler PropertyChanged;

		private int _pageNum = 1;
		private bool _isLoadingData = false;
		private ModelBase _parentItem = null;
		private IViewModel<TItem> _selectedItem = null;
		private List<IViewModel<TItem>> _itemsList = new List<IViewModel<TItem>>();
		#endregion
	}
}
