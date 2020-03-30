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
		public SignalReader(Action<string, bool> notifyResult, Action<byte[]> setSignalData, Action<bool> blockInterface)
		{
			NotifyResult = notifyResult;
			SetSignalData = setSignalData;
			BlockInterface = blockInterface;
		}

		public abstract void TryUploadSignal();

		public abstract void TurnOff();

		protected Action<string, bool> NotifyResult;
		protected Action<bool> BlockInterface;
		protected Action<byte[]> SetSignalData;
	}
}
