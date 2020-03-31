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
		public SignalReader(Action<string, bool> notifyResult, Action onSignalUploadingCompleted, Action<bool> blockInterface, 
			List<SignalChannel> signalChannels, Recording recording)
		{
			NotifyResult = notifyResult;
			OnSignalUploadingCompleted = onSignalUploadingCompleted;
			BlockInterface = blockInterface;
			SignalChannels = signalChannels;
			Recording = recording;

			ChannelsMaskPC = 0;
			foreach (var ch in SignalChannels)
				ChannelsMaskPC |= (1 << ch.ChannelCode);
		}

		public abstract void TryUploadSignal(Recording recording);
		public abstract void TurnOff();


		protected void PrepareRecordingChannels(int maskFromMC, UInt32 length)
		{
			//if (maskFromMC != ChannelsMaskPC)
			//	throw new Exception("Наборы каналов МК и ПК не совпадают (((");

			foreach (var ch in SignalChannels)
			{
				SignalData sd = Recording.SignalDatas.FirstOrDefault(_sd => _sd.SignalChannel.ChannelCode == ch.ChannelCode);

				if (sd == null)
				{
					sd = new SignalData();
					sd.SignalChannel = ch;
					Recording.SignalDatas.Add(sd);
				}

				if (SignalDataBuffers.ContainsKey(ch.ChannelCode))
					SignalDataBuffers[ch.ChannelCode] = new UInt16[length];
				else
					SignalDataBuffers.Add(ch.ChannelCode, new UInt16[length]);

				if (SignalValueNums.ContainsKey(ch.ChannelCode))
					SignalValueNums[ch.ChannelCode] = 0;
				else
					SignalValueNums.Add(ch.ChannelCode, 0);
			}
		}

		protected Action<string, bool> NotifyResult;
		protected Action<bool> BlockInterface;
		protected Action OnSignalUploadingCompleted;

		protected int ChannelsMaskPC;
		protected int ChannelsMaskMC;
		protected Recording Recording;
		protected List<SignalChannel> SignalChannels;
			
		protected Dictionary<int, UInt16[]> SignalDataBuffers = new Dictionary<int, UInt16[]>();
		protected Dictionary<int, UInt32> SignalValueNums = new Dictionary<int, UInt32>();

		public const int MAX_CHANNEL_NUM = 5;
	}
}
