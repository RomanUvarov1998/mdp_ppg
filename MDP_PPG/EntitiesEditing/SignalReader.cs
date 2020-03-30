using PPG_Database.KeepingModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDP_PPG.EntitiesEditing
{
	public abstract class SignalReader 
	{
		public SignalReader(Action<string, bool> notifyResult, Action onSignalUploadingCompleted, Action<bool> blockInterface)
		{
			NotifyResult = notifyResult;
			OnSignalUploadingCompleted = onSignalUploadingCompleted;
			BlockInterface = blockInterface;
		}

		public abstract void TryUploadSignal(Recording recording);
		public abstract void TurnOff();

		protected void PrepareRecordingLength(UInt32 length)
		{
			Recording.SignalData = new SignalData(new byte[length * 2], Recording.Id);
			ValuesBuffer = new UInt16[length];
		}

		protected UInt16[] ValuesBuffer;

		protected Action<string, bool> NotifyResult;
		protected Action<bool> BlockInterface;
		protected Action OnSignalUploadingCompleted;

		protected int ChannelsMask;
		protected Recording Recording;
	}
}
