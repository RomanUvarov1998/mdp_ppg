using PPG_Database.KeepingModels;
using System.Threading.Tasks;

namespace MDP_PPG.PagedViews
{
	public interface IManagablePagedView
	{
		void Freeze();
		Task RecountPages_GoToFirstPage(ModelBase updatedParent);
		Task RecountPages_RefreshCurrentPage(ModelBase updatedParent);
		Task RefreshSelectedItem();
		Task LoadPageOnPageNumChanged();
		ModelBase GetSelectedItem { get; }
	}
}
