using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Data.SqlClient;
using System.IO;
using System.Windows;
using PPG_Database.KeepingModels;

namespace PPG_Database
{
	public class PPG_Context : DbContext
	{
		public PPG_Context() : base(
			new SqlConnection($"Data Source=(LocalDb)\\MSSQLLocalDB;" +
				$"Initial Catalog=PPG_MDP_Database;" +
				$"MultipleActiveResultSets=true;" +
				$"Integrated Security=SSPI;" +
				$"AttachDBFilename={DatabasePath}"), true)
		{
			lock (m_oCreateDatabaseLock)
			{
				string databaseDir = Path.GetDirectoryName(DatabasePath);

				if (!Directory.Exists(databaseDir)) Directory.CreateDirectory(databaseDir);

				if (Database.Exists() && !Database.CompatibleWithModel(throwIfNoMetadata: false))
				{
					Database.Delete();
					MessageBox.Show("Модель изменилась, БД будет пересоздана");
				}

				if (!Database.Exists())
				{
					Database.Create();
					Seed();
				}
			}

			this.Configuration.LazyLoadingEnabled = false;
		}

		const string DatabasePath = "c:\\ProgramData\\PPG_Database\\PPG_MDP_Database.mdf";

		private static readonly object m_oCreateDatabaseLock = new object();
		private void Seed()
		{

		}

		public DbSet<Patient> Patients { get; set; }
		public DbSet<Recording> Recordings { get; set; }
		public DbSet<SignalData> SignalDatas { get; set; }

		protected override void OnModelCreating(DbModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			modelBuilder.Entity<Patient>()
				.HasMany(p => p.Recordings)
				.WithRequired(r => r.Patient)
				.HasForeignKey(r => r.PatientId)
				.WillCascadeOnDelete(true);

			modelBuilder.Entity<SignalData>()
				.HasKey(d => d.RecordingId);

			modelBuilder.Entity<Recording>()
				.HasOptional(r => r.SignalData)
				.WithRequired(d => d.Recording)
				.WillCascadeOnDelete(true);
		}
	}
}
