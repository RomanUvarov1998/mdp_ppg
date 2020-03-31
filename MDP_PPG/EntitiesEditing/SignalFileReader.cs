using MDP_PPG.Helpers;
using Microsoft.Win32;
using PPG_Database.KeepingModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MDP_PPG.EntitiesEditing
{
	public class SignalFileReader : SignalReader, INotifyPropertyChanged
	{
		public SignalFileReader(Action<string, bool> notifyResult, Action onSignalUploadingCompleted, Action<bool> blockInterface,
			List<SignalChannel> signalChannels, Recording recording)
			: base(notifyResult, onSignalUploadingCompleted, blockInterface, signalChannels, recording)
		{
			OF_Dialog = new OpenFileDialog();
			OF_Dialog.Filter = "Текстовые файлы|*.txt|Бинарные файлы|*.dat;*.bin";
		}

		public override void TryUploadSignal(Recording recording)
		{
			throw new NotImplementedException();

			string path;

			if (OF_Dialog.ShowDialog() == true)
				path = OF_Dialog.FileName;
			else return;

			byte[] data;

			try
			{
				string extension = Path.GetExtension(path);
				switch (extension)
				{
					case ".txt":
						string[] lines = File.ReadAllLines(path);
						data = MainFunctions.FromStringLinesFileToDatabase(lines);
						break;
					case ".dat":
						data = File.ReadAllBytes(path);
						break;
					case ".bin":
						data = File.ReadAllBytes(path);
						break;
					default:
						throw new Exception();
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, ex.Source);
				NotifyResult("Ошибка чтения файла", false);
				return;
			}

			NotifyResult($"Файл '{Path.GetFileName(path)}' успешно загружен", false);
			//this.Recording.SignalData.Data = data;
		}

		public override void TurnOff()
		{
			//nothing to turn off
		}


		private OpenFileDialog OF_Dialog;


		public event PropertyChangedEventHandler PropertyChanged;
	}
}
