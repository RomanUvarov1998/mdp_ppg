using PPG_Database;
using PPG_Database.KeepingModels;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDP_PPG.ViewModels
{
	public class RecordingTVM : IViewModel<Recording>, INotifyPropertyChanged
	{
		public RecordingTVM(Recording instance)
		{
			Instance = instance;
			Refresh_RecordedChannelsStr();
		}

		public Recording Instance { get; private set; }
		
		public string CreatedDate => Instance.RecordingDateTime.ToString(Recording.DATE_TIME_FORMAT);
		public string RecordedChannelsStr { get; set; } 
		
		private void Refresh_RecordedChannelsStr()
		{
			StringBuilder sb = new StringBuilder(string.Empty);

			if (Instance.SignalDatas.Count > 0)
			{
				foreach (var sd in Instance.SignalDatas)
					sb.AppendLine($"{sd.SignalChannel.ChannelCode} {sd.SignalChannel.Name}");
			}
			else
			{
				sb.Append("Нет записанных каналов");
			}

			RecordedChannelsStr = sb.ToString();
		}

		public void UpdateViewOnModelUpdated(Recording updatedModel)
		{
			Instance.UpdateSelfFields(updatedModel);
			Refresh_RecordedChannelsStr();

			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CreatedDate)));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RecordedChannelsStr)));
		}


		public event PropertyChangedEventHandler PropertyChanged;
	}
}
