using MDP_PPG.Helpers;
using Microsoft.Win32;
using PPG_Database.KeepingModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
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
			_recordingRxModel = new RecordingRxModel();
		}

		public override void TryUploadSignal(Recording recording)
		{
			string path;

			if (OF_Dialog.ShowDialog() == true)
				path = OF_Dialog.FileName;
			else return;

			byte[] data;
					 
			try
			{
				string extension = Path.GetExtension(path);
				if (!extension.Equals(".txt"))
					throw new Exception("Не .txt файлы еще не сделаны (((");

				var ci = new CultureInfo("en-us");
				using (var sr = new StreamReader(path))
				{
					string line = sr.ReadLine();
					if (line == null) throw new Exception("Нет списка каналов (((");
					int[] channelsCodes = line.Split('\t').Select(s => int.Parse(s)).ToArray();
					
					line = sr.ReadLine();
					if (line == null) throw new Exception("Нет количества значений (((");
					UInt32 signalLength = (UInt32)double.Parse(line, ci);

					int channelsMask = 0;
					foreach (var chc in channelsCodes)
						channelsMask |= (1 << chc);

					_recordingRxModel.PrepareToRecieve(channelsMask, signalLength);

					for (UInt32 i = 0; i < signalLength; ++i)
					{
						line = sr.ReadLine();
						if (line == null) throw new Exception("Присутствуют не все отсчеты сигнала (((");
						double[] values = line.Split('\t').Select(s => double.Parse(s, ci)).ToArray();
						for (int v = 0; v < values.Length; ++v)
						{
							_recordingRxModel.AppendValue(channelsCodes[v], values[v]);
						}
					}

					if (!_recordingRxModel.CheckIsFull())
						throw new Exception("Сигнал был получен не полностью (((");
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, ex.Source);
				NotifyResult("Ошибка чтения файла", false);
				BlockInterface(false);
				return;
			}

			NotifyResult($"Файл '{Path.GetFileName(path)}' успешно загружен", true);
			//this.Recording.SignalData.Data = data;

			_recordingRxModel.PutInModel(Recording, SignalChannels);
			BlockInterface(false);
		}

		public override void TurnOff()
		{
			//nothing to turn off
		}


		private OpenFileDialog OF_Dialog;
		private RecordingRxModel _recordingRxModel;


		public event PropertyChangedEventHandler PropertyChanged;
	}
}
