using PPG_Database.KeepingModels;
using System.Threading.Tasks;

namespace MDP_PPG.PagedViews
{
	public interface IManagablePagedView
	{
		void Freeze();
		Task RecountPages_GoToFirstPage(ModelBase updatedParent, bool selectFirstItemIfExists);
		Task RecountPages_RefreshCurrentPage(ModelBase updatedParent);
		Task RefreshSelectedItem();
		Task LoadPageOnPageNumChanged(bool selectFirstItemIfExists);
		ModelBase GetSelectedItem { get; }
	}
}
