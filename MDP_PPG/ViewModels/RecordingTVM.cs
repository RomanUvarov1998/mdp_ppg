using PPG_Database;
using PPG_Database.KeepingModels;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace MDP_PPG.ViewModels
{
	public class RecordingTVM : IViewModel<Recording>, INotifyPropertyChanged
	{
		public RecordingTVM(Recording instance)
		{
			Instance = instance;
		}

		public Recording Instance { get; private set; }
		
		public string CreatedDate => Instance.RecordingDateTime.ToString(Recording.DATE_TIME_FORMAT);
		public string HasSignalFileStr => Instance.SignalData == null ? "Отсутствует" : "Загружен";

		public void UpdateViewOnModelUpdated(Recording updatedModel)
		{
			Instance.UpdateSelfFields(updatedModel);

			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CreatedDate)));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HasSignalFileStr)));
		}


		public event PropertyChangedEventHandler PropertyChanged;
	}
}
