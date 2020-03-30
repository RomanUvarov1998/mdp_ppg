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
	}
}
