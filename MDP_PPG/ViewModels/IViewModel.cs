using PPG_Database.KeepingModels;

namespace MDP_PPG.ViewModels
{
	public interface IViewModel<TEntity>
		where TEntity : ModelBase
	{
		TEntity Instance { get; }

		void UpdateViewOnModelUpdated(TEntity updatedModel);
	}
}
