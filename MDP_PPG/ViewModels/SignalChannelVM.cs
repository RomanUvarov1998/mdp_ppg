using PPG_Database.KeepingModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace MDP_PPG.ViewModels
{
	public class SignalChannelVM : IViewModel<SignalChannel>
	{
		public SignalChannelVM(SignalChannel instance)
		{
			Instance = instance;
		}

		public SignalChannel Instance { get; private set; }

		public Brush TextBrush => Instance.IsInUse ? Brushes.Black : Brushes.Gray;
		public string Name => Instance.Name;

		public void UpdateViewOnModelUpdated(SignalChannel updatedModel)
		{
			throw new NotImplementedException();
		}
	}
}
