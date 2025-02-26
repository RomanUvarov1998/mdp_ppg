﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPG_Database.KeepingModels
{
	public class Recording : ModelBase
	{
		public Recording()
		{
			SignalDatas = new HashSet<SignalData>();
		}
		public Recording(int patientId) : this()
		{
			PatientId = patientId;
		}
		public int PatientId { get; set; }

		public DateTime RecordingDateTime { get; set; }

		[NotMapped]
		public double Fs = 250;


		//------------------- Navigation Fields -----------------------
		public Patient Patient { get; set; }

		public ICollection<SignalData> SignalDatas { get; set; }


		//-------------------------------------- API ----------------------------------
		public override bool UpdateSelfFields(ModelBase updatedModel)
		{
			Recording r = (Recording)updatedModel;
			bool res = false;

			if (!DateTime.Equals(RecordingDateTime, r.RecordingDateTime))
			{
				RecordingDateTime = r.RecordingDateTime;
				res = true;
			}

			return res;
		}

		[NotMapped]
		public static string DATE_TIME_FORMAT = "dd.MM.yyyy hh.mm";
	}
}
