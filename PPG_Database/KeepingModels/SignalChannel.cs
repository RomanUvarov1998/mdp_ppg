using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPG_Database.KeepingModels
{
	public class SignalChannel : ModelBase
	{
		public SignalChannel()
		{

		}
		public SignalChannel(string name, int channelCode) : this()
		{
			Name = name;
			ChannelCode = channelCode;
		}


		public string Name { get; set; }
		public bool IsInUse { get; set; }
		public int ChannelCode { get; set; }


		//------------------- Navigation Fields -----------------------



		//-------------------------------------- API ----------------------------------

		public override bool UpdateSelfFields(ModelBase updatedModel)
		{
			throw new NotImplementedException();
		}

		public static double GetPlotScaleY(SignalChannel signalChannel)
		{
			if (!ChannelCode_PlotScaleYs.ContainsKey(signalChannel.ChannelCode))
				ChannelCode_PlotScaleYs.Add(signalChannel.ChannelCode, 1);

			return ChannelCode_PlotScaleYs[signalChannel.ChannelCode];
		}
		public static void SetPlotScaleY(SignalChannel signalChannel, double scaleY)
		{
			if (!ChannelCode_PlotScaleYs.ContainsKey(signalChannel.ChannelCode))
				ChannelCode_PlotScaleYs.Add(signalChannel.ChannelCode, 1);

			ChannelCode_PlotScaleYs[signalChannel.ChannelCode] = scaleY;
		}
		private static Dictionary<int, double> ChannelCode_PlotScaleYs = new Dictionary<int, double>();
	}
}
