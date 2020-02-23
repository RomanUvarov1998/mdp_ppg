using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPG_Database.KeepingModels
{
	public class SignalData
	{
		public SignalData()
		{
				
		}
		public SignalData(byte[] data, int recordingId) : this()
		{
			Data = data;
			RecordingId = recordingId;
		}

		public int RecordingId { get; set; }
		public DateTime CreatedDate { get; set; } = DateTime.Now;
		public byte[] Data { get; set; }

		//------------------- Navigation Fields -----------------------
		public Recording Recording { get; set; }
	}
}
