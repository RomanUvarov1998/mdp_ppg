using MDP_PPG.Helpers;
using PPG_Database.KeepingModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Timers;
using System.Windows;

namespace MDP_PPG.EntitiesEditing
{
  public class SignalPortReader : SignalReader, INotifyPropertyChanged
  {
    public SignalPortReader(Action<string, bool> notifyResult, Action onSignalUploadingCompleted, Action<bool> blockInterface,
      List<SignalChannel> signalChannels, Recording recording)
      : base(notifyResult, onSignalUploadingCompleted, blockInterface, signalChannels, recording)
    {
      // Create a new SerialPort object with default settings.
      _serialPort = new SerialPort();

      // Allow the user to set the appropriate properties.
      _serialPort.BaudRate = 9600;// SetPortBaudRate(_serialPort.BaudRate);
      _serialPort.Parity = Parity.None;// SetPortParity(_serialPort.Parity);
      _serialPort.DataBits = 8;// SetPortDataBits(_serialPort.DataBits);
      _serialPort.StopBits = StopBits.One;// SetPortStopBits(_serialPort.StopBits);
      _serialPort.Handshake = Handshake.None;// SetPortHandshake(_serialPort.Handshake);

      // Set the read/write timeouts
      _serialPort.ReadTimeout = 500;
      _serialPort.WriteTimeout = 500;

      MyTimer = new Timer(500.0);
      MyTimer.AutoReset = true;
      MyTimer.Elapsed += MyTimer_Elapsed;

      RefreshPortsList();
      SelectedPort = AvailablePorts.FirstOrDefault();
    }


    //---------------------------- GUI Props ----------------------------------------
    public UInt32 ValuesLoaded  
    {
      get => _valuesLoaded;
      set
      {
        _valuesLoaded = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ValuesLoaded)));
      }
    }
    public UInt32 TotalValues 
    {
      get => _totalValues;
      set
      {
        _totalValues = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TotalValues)));
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

    //---------------------------- API --------------------------------------------
    public void RefreshPortsList()
    {
      AvailablePorts = SerialPort.GetPortNames();
    }
    public bool CheckSelectedPortExists()
    {
      if (SelectedPort == null) return false;

      string selPort = SelectedPort;
      RefreshPortsList();

      SelectedPort = selPort;

      return AvailablePorts.Contains(selPort);
    }

    public override void TryUploadSignal(Recording recording)
    {
      if (!TryToConnect(out string errorMsg))
      {
        TurnOff();
        BlockInterface(false);
        NotifyResult(errorMsg, false);
        return;
      }

      this.Recording = recording;

      BeginInteracting(Modes.SIGNAL_UPLOADING);
    }

    public void TrySavelSettingsToMC(List<SignalChannel> signalChannels)
    {
      if (!TryToConnect(out string errorMsg))
      {
        TurnOff();
        BlockInterface(false);
        NotifyResult(errorMsg, false);
        return;
      }

      ChannelsMaskPC = 0;
      foreach (var ch in signalChannels)
        if (ch.IsInUse) ChannelsMaskPC |= (byte)(1 << ch.ChannelCode);

      BeginInteracting(Modes.SETTINGS);
    }

    public override void TurnOff()
    {
      MyTimer.Stop();
      if (_serialPort.IsOpen) _serialPort.Close();
      _serialPort.DataReceived -= _serialPort_DataReceived;
    }

    //---------------------------- Other --------------------------------------------
    private bool TryToConnect(out string errorMsg)
    {
      BlockInterface(true);

      ValuesLoaded = 0;
      TotalValues = 0;
      PrBarVis = Visibility.Hidden;

      try
      {
        _serialPort.PortName = SelectedPort;
        _serialPort.DataReceived += _serialPort_DataReceived;
        if (!_serialPort.IsOpen) _serialPort.Open();

        NotifyResult("Соединение...", false);

        errorMsg = string.Empty;
        return true;
      }
      catch (Exception ex)
      {
        errorMsg = ex.Message;
        return false;
      }
    }

    private void BeginInteracting(Modes mode)
    {
      _mode = mode;

      MyLog("Begin transmitting");
      MyLog("Got: ");
      SignalIsCorrupted = false;

      PrBarVis = Visibility.Hidden;

      MyTimer.Start();
    }

    private void MyTimer_Elapsed(object sender, ElapsedEventArgs e)
    {
      if (!_serialPort.IsOpen)
      {
        TurnOff();

        NotifyResult("Порт неожиданно закрылся", false);
        BlockInterface(false);
        return;
      }

      switch (_mode)
      {
        case Modes.SIGNAL_UPLOADING:
          SendByteToMC(MC_Tokens.CHANNELS_MASK);
          MyLog($"{MC_Tokens.CHANNELS_MASK} ->");
          break;
        case Modes.SETTINGS:
          SendByteToMC(MC_Tokens.SAVE_SETTINGS);
          MyLog($"{MC_Tokens.SAVE_SETTINGS} ->");
          SendByteToMC((byte)ChannelsMaskPC);
          break;
      }
    }

    private void _serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
    {
      MyTimer.Stop();
      if (!_serialPort.IsOpen) _serialPort.Open();

      try
      {
        while (!SignalIsCorrupted && _serialPort.IsOpen && _serialPort.BytesToRead > 0)
        {
          int b = _serialPort.ReadByte(); MyLog($" - {bytePos} {b}");
          ReadBuffer[bytePos] = (byte)b;
          bytePos++;

          if (bytePos < ReadBuffer.Length) continue;

          bytePos = 0;

          ProcessGettingSignal();
        }
      }
      catch (TimeoutException)
      {
        TurnOff();
        NotifyResult($"Порт '{SelectedPort}' не отвечает...", false);
        BlockInterface(false);
      }
      catch (InvalidOperationException ex)
      {
        TurnOff();
        NotifyResult($"Порт '{SelectedPort}' внезапно разорвал соединение, {ex.Message} || {ex.Source}", false);
        BlockInterface(false);
      }
    }

    private void ProcessGettingSignal()
    {
      MC_Tokens token = (MC_Tokens)ReadBuffer[0];
      MyLog($"State is '{token}'");
      switch (token)
      {
        case MC_Tokens.CHANNELS_MASK:
          ChannelsMaskMC = ReadBuffer[1];

          SendByteToMC(MC_Tokens.GET_SIGNAL_LENGTH);
          MyLog($"{MC_Tokens.GET_SIGNAL_LENGTH} ->");
          break;
        case MC_Tokens.GET_SIGNAL_LENGTH:
          TotalValues = BitConverter.ToUInt32(ReadBuffer, 1);

          MyLog($"Signal length is '{TotalValues}'");
          PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TotalValues)));

          if (TotalValues == 0)
          {
            TurnOff();
            SignalIsCorrupted = true;
            NotifyResult("Длина сигнала равна 0", false);
          }

          PrepareRecordingChannels(ChannelsMaskMC, TotalValues);

          MyLog("");

          PrBarVis = Visibility.Visible;

          SendByteToMC(MC_Tokens.GET_DATA);
          MyLog($"{MC_Tokens.GET_DATA} ->");
          break;
        case MC_Tokens.GET_DATA:
          int channel = ReadBuffer[1];
          UInt16 value = BitConverter.ToUInt16(ReadBuffer, 2);

          MyLog($"Got '{value}': {ValuesLoaded}/{TotalValues}");
          PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ValuesLoaded)));

          if (SignalValueNums.ContainsKey(channel))
            SignalValueNums[channel]++;
          else
            throw new Exception($"Несуществующий канал '{channel}' (((");

          ValuesLoaded = SignalValueNums.Values.Max();

          if (ValuesLoaded > TotalValues)
          {
            TurnOff();
            NotifyResult("Значений больше, чем заявлено", false);
            BlockInterface(false);
            return;
          }

          if (SignalDataBuffers.ContainsKey(channel))
            SignalDataBuffers[channel][SignalValueNums[channel] - 1] = value;
          else
            throw new Exception($"Несуществующий канал '{channel}' (((");

          SendByteToMC(MC_Tokens.GET_DATA);
          MyLog($"{MC_Tokens.GET_DATA} ->");

          break;
        case MC_Tokens.DATA_END:
          if (!SignalIsCorrupted)
          {
            NotifyResult("Сигнал успешно передан", true);

            foreach (var cc in SignalDataBuffers.Keys)
            {
              SignalData sd = this.Recording.SignalDatas.First(_sd => _sd.SignalChannel.ChannelCode == cc);

              sd.Data = MainFunctions.FromMcToDatabase(SignalDataBuffers[cc]);
            }
          }
          else
            NotifyResult("Сигнал был поврежден во время передачи", true);

          BlockInterface(false);
          TurnOff();
          break;
        case MC_Tokens.SAVE_SETTINGS:
          if (ChannelsMaskPC == ReadBuffer[1])
          {
            TurnOff();
            NotifyResult("Настройки успешно сохранены", true);
            BlockInterface(false);
          }
          else
          {
            TurnOff();
            NotifyResult("Не удалось успешно сохранить настройки", false);
            BlockInterface(false);
          }
          break;
      }
    }

    private void SendByteToMC(byte data)
    {
      try
      {
        _serialPort.Write(new byte[] { data }, 0, 1);
      }
      catch (Exception ex)
      {
        TurnOff();
        NotifyResult(ex.Message, false);
        BlockInterface(false);
      }
    }
    private void SendByteToMC(MC_Tokens token)
    {
      SendByteToMC((byte)token);
    }

    private void MyLog(string s)
    {
      Console.WriteLine(s);
    }

    private SerialPort _serialPort;
    private Timer MyTimer;

    private int bytePos = 0;
    private byte[] ReadBuffer = new byte[5];

    private bool SignalIsCorrupted = false;

    enum Modes
    {
      SETTINGS,
      SIGNAL_UPLOADING
    }
    private Modes _mode;
    enum MC_Tokens : Byte
    {
      CHANNELS_MASK = 1,
      GET_SIGNAL_LENGTH = 2,
      GET_DATA = 3,
      DATA_END = 4,
      SAVE_SETTINGS = 5,
    }
    private string[] _availablePorts;
    private string _selectedPort;
    private Visibility _prBarVis = Visibility.Hidden;
    private UInt32 _valuesLoaded;
    private UInt32 _totalValues;

    public event PropertyChangedEventHandler PropertyChanged;
  }
}
