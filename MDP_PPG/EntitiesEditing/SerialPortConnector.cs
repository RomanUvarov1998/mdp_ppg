using MDP_PPG.Helpers;
using PPG_Database.KeepingModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Ports;
using System.Linq;
using System.Windows;

namespace MDP_PPG.EntitiesEditing
{
	public class SerialPortConnector : BackgroundWorker, INotifyPropertyChanged
	{
		public SerialPortConnector(Action<string, bool> notifyResult, Action<bool> setUIBlocked)
		{
			this.WorkerReportsProgress = true;
			this.WorkerSupportsCancellation = true;

			this.DoWork += SerialPortConnector_DoWork;
			this.ProgressChanged += SerialPortConnector_ProgressChanged;
			this.RunWorkerCompleted += SerialPortConnector_RunWorkerCompleted;

			_notifyResult = notifyResult;
			_setUIBlocked = setUIBlocked;

			_readBuffer = new byte[5];
			_writeBuffer = new byte[1];
		}

		//--------------------------- API --------------------------
		public event PropertyChangedEventHandler PropertyChanged;
		public string[] AvailablePorts
		{
			get => _availablePorts;
			set
			{
				_availablePorts = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AvailablePorts)));
			}
		}
		public string SelectedPort
		{
			get => _selectedPort;
			set
			{
				_selectedPort = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedPort)));
			}
		}
		public Visibility PrBarVis
		{
			get => _prBarVis;
			set
			{
				_prBarVis = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PrBarVis)));
			}
		}
		public void RefreshPortsList()
		{
			AvailablePorts = SerialPort.GetPortNames();
		}

		public void ResieveRecording(Recording recording,
			ICollection<SignalChannel> signalChannels, Action<UInt32> notifyTotal,
			Action<UInt32> notifyRecieved)
		{
			if (!CheckIsPortReady()) return;

			CreateSerialPort();

			_recordingRxModel = new RecordingRxModel();
			_recording = recording;
			_signalChannels = signalChannels;

			_notifyTotal = notifyTotal;
			_notifyRecieved = notifyRecieved;

			_serialPort.Open();

			_mode = Modes.RECORDING_UPLOADING;

			PrBarVis = Visibility.Visible;
			this.RunWorkerAsync();
		}

		public void SaveSettings(ICollection<SignalChannel> signalChannels)
		{
			if (!CheckIsPortReady()) return;

			CreateSerialPort();

			_signalChannels = signalChannels;

			_serialPort.Open();

			_mode = Modes.SETTINGS;

			this.RunWorkerAsync();
		}

		public void TurnOff()
		{
			this.CancelAsync();
			_serialPort.Close();
			if (_serialPort != null) _serialPort.Dispose();
		}

		//---------------------------- events ----------------------
		private void SerialPortConnector_DoWork(object sender, DoWorkEventArgs e)
		{
			_setUIBlocked(true);

			switch (_mode)
			{
				case Modes.RECORDING_UPLOADING:
					UploadSignal(e);
					break;
				case Modes.SETTINGS:
					SaveSettings(e);
					break;
			}
		}
		private void SerialPortConnector_ProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			//throw new NotImplementedException();
		}
		private void SerialPortConnector_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			if (e.Error != null)
			{
				_notifyResult.Invoke(e.Error.Message, false);
			}
			else if (e.Cancelled)
			{
				_notifyResult.Invoke("Операция была отменена", false);
			}
			else
			{
				switch (_mode)
				{
					case Modes.RECORDING_UPLOADING:
						_notifyResult.Invoke("Сигнал успешно загружен", true);
						_recordingRxModel.PutInModel(_recording, _signalChannels);
						break;
					case Modes.SETTINGS:
						_notifyResult.Invoke("Настройки успешно сохранены", true);
						break;
				}
			}

			PrBarVis = Visibility.Hidden;
			TurnOff();
			_setUIBlocked(false);
		}

		//---------------------------- private ---------------------
		private bool CheckIsPortReady()
		{
			if (SelectedPort == null)
			{
				_notifyResult.Invoke("Выберите порт для передачи сигнала с микроконтроллера", false);
				return false;
			}

			if (!CheckSelectedPortExists())
			{
				_notifyResult.Invoke("Данный порт больше не существует, выберите другой порт", false);
				return false;
			}

			return true;
		}
		private bool CheckSelectedPortExists()
		{
			if (SelectedPort == null) return false;

			string selPort = SelectedPort;
			RefreshPortsList();

			SelectedPort = selPort;

			return AvailablePorts.Contains(selPort);
		}
		private void CreateSerialPort()
		{
			if (_serialPort != null)
			{
				_serialPort.Close();
				_serialPort.Dispose();
			}

			_serialPort = new SerialPort();
			_serialPort.PortName = SelectedPort;
			_serialPort.BaudRate = 9600;
			_serialPort.Parity = Parity.None;
			_serialPort.DataBits = 8;
			_serialPort.StopBits = StopBits.One;
			_serialPort.Handshake = Handshake.None;

			_serialPort.ReadTimeout = SerialPort.InfiniteTimeout; //500;
			_serialPort.WriteTimeout = SerialPort.InfiniteTimeout; //500;
		}
		private void Send(byte data)
		{
			_writeBuffer[0] = data;
			_serialPort.Write(_writeBuffer, 0, _writeBuffer.Length);
		}
		private void RecieveBuffer(MC_Tokens txToken, MC_Tokens? rxToken2 = null)
		{
			int rxToken;

			while (true)
			{
				rxToken = _serialPort.ReadByte();

				if (rxToken == (byte)txToken) break;

				if (rxToken2 != null && (int)rxToken2.Value == rxToken) break;

				if (this.CancellationPending) return;
			}

			_readBuffer[0] = (byte)rxToken;
			_serialPort.Read(_readBuffer, 1, _readBuffer.Length - 1);
		}

		private void UploadSignal(DoWorkEventArgs e)
		{
			if (this.CancellationPending) goto BackgroundWorkCanceled;

			Send((byte)MC_Tokens.CHANNELS_MASK);
			RecieveBuffer(MC_Tokens.CHANNELS_MASK);
			int channelsMask = _readBuffer[1];

			if (this.CancellationPending) goto BackgroundWorkCanceled;

			Send((byte)MC_Tokens.GET_SIGNAL_LENGTH);
			RecieveBuffer(MC_Tokens.GET_SIGNAL_LENGTH);
			UInt32 signalLength = BitConverter.ToUInt32(_readBuffer, 1);
			_notifyTotal.Invoke(signalLength);
			_recordingRxModel.PrepareToRecieve(channelsMask, signalLength);

			if (this.CancellationPending) goto BackgroundWorkCanceled;

			while (true)
			{
				if (this.CancellationPending)
				{
					e.Cancel = true;
					return;
				}

				Send((byte)MC_Tokens.GET_DATA);
				RecieveBuffer(MC_Tokens.GET_DATA, MC_Tokens.DATA_END);
				if ((MC_Tokens)_readBuffer[0] == MC_Tokens.DATA_END)
				{
					break;
				}

				if (this.CancellationPending) goto BackgroundWorkCanceled;

				double value = BitConverter.ToUInt16(_readBuffer, 2);
				_recordingRxModel.AppendValue(_readBuffer[1], value);
				_notifyRecieved.Invoke(_recordingRxModel.RecievedValues);
			}

			if (this.CancellationPending) goto BackgroundWorkCanceled;

			if (!_recordingRxModel.CheckIsFull())
				throw new Exception("Сигнал был получен не полностью (((");

			if (this.CancellationPending) goto BackgroundWorkCanceled;

			e.Result = true;
			return;

		BackgroundWorkCanceled:
			e.Cancel = true;
		}
		private void SaveSettings(DoWorkEventArgs e)
		{
			if (this.CancellationPending) goto BackgroundWorkCanceled;

			int mask1 = 0;
			foreach (var ch in _signalChannels)
				if (ch.IsInUse)
					mask1 |= (1 << ch.ChannelCode);

			if (this.CancellationPending) goto BackgroundWorkCanceled;

			Send((byte)MC_Tokens.SAVE_SETTINGS);

			if (this.CancellationPending) goto BackgroundWorkCanceled;

			Send((byte)mask1);
			RecieveBuffer(MC_Tokens.SAVE_SETTINGS);

			if (mask1 != _readBuffer[1])
				throw new Exception("Настройки сохранены неуспешно");

			e.Result = true;
			return;

		BackgroundWorkCanceled:
			e.Cancel = true;
		}

		private string[] _availablePorts;
		private string _selectedPort;
		private Visibility _prBarVis = Visibility.Hidden;

		private RecordingRxModel _recordingRxModel;
		private SerialPort _serialPort;
		private byte[] _readBuffer;
		private byte[] _writeBuffer;
		private Action<string, bool> _notifyResult;
		private Action<bool> _setUIBlocked;
		private Recording _recording;
		private ICollection<SignalChannel> _signalChannels;
		private Action<UInt32> _notifyTotal;
		private Action<UInt32> _notifyRecieved;
		enum MC_Tokens : Byte
		{
			CHANNELS_MASK = 1,
			GET_SIGNAL_LENGTH = 2,
			GET_DATA = 3,
			DATA_END = 4,
			SAVE_SETTINGS = 5,
		}
		enum Modes
		{
			SETTINGS,
			RECORDING_UPLOADING
		}
		private Modes _mode;
	}

	public class RecordingRxModel
	{
		public RecordingRxModel()
		{
			_channelBuffers = new Dictionary<int, ChannelBuffer>();
		}

		public void PrepareToRecieve(int mask, UInt32 length)
		{
			for (int i = 0; i < 8; ++i)
				if ((mask & (1 << i)) > 0)
					_channelBuffers.Add(i, new ChannelBuffer(length, i));
		}

		public void AppendValue(int channelCode, double value)
		{
			_channelBuffers[channelCode].AppendValue(value);
		}

		public UInt32 RecievedValues => _channelBuffers.Max(chb => chb.Value.ValueNum);

		public void PutInModel(Recording instance, ICollection<SignalChannel> signalChannels)
		{
			foreach (var ch in _channelBuffers.Keys)
			{
				var sd = _channelBuffers[ch].GetSignalData(signalChannels);
				instance.SignalDatas.Add(sd);
			}
		}

		public bool CheckIsFull() => _channelBuffers.All(chb => chb.Value.IsFull());

		private Dictionary<int, ChannelBuffer> _channelBuffers;
	}

	public class ChannelBuffer
	{
		public ChannelBuffer(UInt32 length, int channelCode)
		{
			_valuesBuffer = new double[length];
			_channelCode = channelCode;
		}

		public void AppendValue(double value)
		{
			_valuesBuffer[ValueNum] = value;
			++ValueNum;
			Console.WriteLine($"{_channelCode} : {ValueNum - 1}");
		}

		public SignalData GetSignalData(ICollection<SignalChannel> signalChannels)
		{
			var sd = new SignalData();

			sd.Data = MainFunctions.FromMcToDatabase(_valuesBuffer);
			sd.SignalChannel = signalChannels.First(ch => ch.ChannelCode == _channelCode);

			return sd;
		}

		public bool IsFull() => ValueNum == _valuesBuffer.Length;

		public UInt32 ValueNum = 0;

		private double[] _valuesBuffer;
		private int _channelCode;
	}
}
