using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPG_Database.KeepingModels
{
	public class SignalData : ModelBase
	{
		public SignalData()
		{
				
		}
		//public SignalData(byte[] data, int recordingId) : this()
		//{
		//	Data = data;
		//	RecordingId = recordingId;
		//}

		public int RecordingId { get; set; }
		public int SignalChannelId { get; set; }
		public byte[] Data { get; set; }

		//------------------- Navigation Fields -----------------------
		public Recording Recording { get; set; }
		public SignalChannel SignalChannel { get; set; }

		public override bool UpdateSelfFields(ModelBase updatedModel)
		{
			throw new NotImplementedException();
		}
	}
}
