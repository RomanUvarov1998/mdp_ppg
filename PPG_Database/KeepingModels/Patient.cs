using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPG_Database.KeepingModels
{
	public class Patient : ModelBase
	{
		public Patient()
		{
			Recordings = new HashSet<Recording>();
		}

		public Patient(string surname, string name, string patronimyc, int id) : this()
		{
			Surname = surname;
			Name = name;
			Patronimyc = patronimyc;
		}

		[Required]
		public string Surname { get; set; }

		[Required]
		public string Name { get; set; }

		[Required]
		public string Patronimyc { get; set; }


		//-------------------------------------- Navigation Fields --------------------
		public ICollection<Recording> Recordings { get; set; }


		//-------------------------------------- API ----------------------------------
		public override bool UpdateSelfFields(ModelBase updatedModel)
		{
			Patient p = (Patient)updatedModel;

			bool res = false;

			if (!string.Equals(Surname, p.Surname))
			{
				Surname = p.Surname;
				res = true;
			}

			if (!string.Equals(Name, p.Name))
			{
				Name = p.Name;
				res = true;
			}

			if (!string.Equals(Patronimyc, p.Patronimyc))
			{
				Patronimyc = p.Patronimyc;
				res = true;
			}

			return res;
		}
	}
}
