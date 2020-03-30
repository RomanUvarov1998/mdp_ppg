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
    public SignalPortReader(Action<string, bool> notifyResult, Action<byte[]> setSignalData, Action<bool> blockInterface)
      : base(notifyResult, setSignalData, blockInterface)
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

    public double ValuesLoaded => ValueNum;
    public double TotalValues => SignalLength;

    public Visibility PrBarVis
    {
      get => prBarVis;
      set
      {
        prBarVis = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PrBarVis)));
      }
    }

    public string[] AvailablePorts
    {
      get => availablePorts;
      set
      {
        availablePorts = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AvailablePorts)));
      }
    }
    public string SelectedPort
    {
      get => selectedPort;
      set
      {
        selectedPort = value;
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

    public override void TryUploadSignal()
    {
      BlockInterface(true);

      ValueNum = 0;
      SignalLength = 0;

      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ValuesLoaded)));
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TotalValues)));

      NotifyResult("Подготовка к передаче...", false);

      try
      {
        _serialPort.PortName = SelectedPort;
        _serialPort.DataReceived += _serialPort_DataReceived;
        if (!_serialPort.IsOpen) _serialPort.Open();

        NotifyResult("Передача...", false);
        BeginTransmitting();
      }
      catch (UnauthorizedAccessException ex)
      {
        TurnOff();
        BlockInterface(false);
        NotifyResult($"Не удалось открыть порт '{SelectedPort}' из-за ошибки доступа", false);
      }
      catch (IOException)
      {
        TurnOff();
        BlockInterface(false);
        NotifyResult($"Не удалось открыть порт '{SelectedPort}', потомучто его не существует", false);
      }
    }

    public override void TurnOff()
    {
      MyTimer.Stop();
      if (_serialPort.IsOpen) _serialPort.Close();
      _serialPort.DataReceived -= _serialPort_DataReceived;
    }

    public bool LoadSettingsToMC(List<SignalChannel> signalChannels)
    {
      byte channelsSettings = 0;
      foreach (var ch in signalChannels)
        if (ch.IsInUse) channelsSettings |= (byte)(1 << ch.ChannelCode);

      try
      {
        if (_serialPort.IsOpen) _serialPort.Open();
        _serialPort.Write(new byte[] { (byte)MC_Tokens.SAVE_SETTINGS, channelsSettings }, 0, 2);
      }
      catch (Exception ex)
      {
        TurnOff();
        return false;
      }

      return true;
    }

    //---------------------------- Other --------------------------------------------

    private void BeginTransmitting()
    {
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
          AskFor(MC_Tokens.GET_SIGNAL_LENGTH);
          break;
        case Modes.SETTINGS:
          AskFor(MC_Tokens.GET_SIGNAL_LENGTH);
          break;
      }
    }

    private void _serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
    {
      MyTimer.Stop();
      if (!_serialPort.IsOpen) _serialPort.Open();

      try
      {
        while (!SignalIsCorrupted && _serialPort.BytesToRead > 0)
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
        case MC_Tokens.GET_SIGNAL_LENGTH:
          SignalLength = BitConverter.ToUInt32(ReadBuffer, 1);

          MyLog($"Signal length is '{SignalLength}'");
          PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TotalValues)));

          if (SignalLength == 0)
          {
            TurnOff();
            SignalIsCorrupted = true;
            NotifyResult("Длина сигнала равна 0", false);
          }

          RecievedValues = new UInt16[SignalLength];
          MyLog("");

          PrBarVis = Visibility.Visible;

          AskFor(MC_Tokens.GET_DATA);
          break;
        case MC_Tokens.GET_DATA:
          if (ValueNum > SignalLength)
          {
            TurnOff();
            NotifyResult("Значений больше, чем заявлено", false);
            BlockInterface(false);
            return;
          }
          
          UInt16 value = BitConverter.ToUInt16(ReadBuffer, 1);

          MyLog($"Got '{value}': {ValueNum}/{SignalLength}");
          PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ValuesLoaded)));

          checked { ++ValueNum; }
          RecievedValues[ValueNum - 1] = value;
          break;
        case MC_Tokens.DATA_END:
          if (!SignalIsCorrupted)
          {
            NotifyResult("Сигнал успешно передан", true);
            SetSignalData(MainFunctions.FromMcToDatabase(RecievedValues));
          }
          else
            NotifyResult("Сигнал был поврежден во время передачи", true);

          BlockInterface(false);
          break;
      }
    }

    private void AskFor(MC_Tokens token)
    {
      try
      {
        _serialPort.Write(new byte[] { (byte)token }, 0, 1);
      }
      catch (Exception ex)
      {
        TurnOff();
        NotifyResult(ex.Message, false);
        BlockInterface(false);
      }
    }

    private void MyLog(string s)
    {
      Console.WriteLine(s);
    }

    private SerialPort _serialPort;
    private Timer MyTimer;

    private UInt16[] RecievedValues;
    private UInt32 ValueNum = 0;

    private int bytePos = 0;
    private byte[] ReadBuffer = new byte[5];
    private UInt32 SignalLength;

    private bool SignalIsCorrupted = false;

    enum Modes
    {
      SETTINGS,
      SIGNAL_UPLOADING
    }
    private Modes _mode;
    enum MC_Tokens : Byte
    {
      GET_SIGNAL_LENGTH = 1,
      GET_DATA = 2,
      DATA_END = 3,
      SAVE_SETTINGS = 5,
    }
    private string[] availablePorts;
    private string selectedPort;
    private Visibility prBarVis = Visibility.Hidden;

    public event PropertyChangedEventHandler PropertyChanged;
  }
}
