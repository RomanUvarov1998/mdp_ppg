using PPG_Database.KeepingModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDP_PPG.ViewModels
{
	public class PatientTVM : IViewModel<Patient>, INotifyPropertyChanged
	{
		public PatientTVM(Patient instance)
		{
			Instance = instance;
		}

		public Patient Instance { get; private set; }

		public string Surname => Instance.Surname;
		public string Name => Instance.Name;
		public string Patronimyc => Instance.Patronimyc;
		public string CreatedDate => Instance.CreatedDate.ToString();
		public int RecordingsCount => Instance.Recordings.Count;

		public void UpdateViewOnModelUpdated(Patient updatedModel)
		{
			Instance.UpdateSelfFields(updatedModel);

			Instance.Recordings = updatedModel.Recordings;

			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Surname)));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Patronimyc)));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RecordingsCount)));
		}

		public event PropertyChangedEventHandler PropertyChanged;
	}
}
