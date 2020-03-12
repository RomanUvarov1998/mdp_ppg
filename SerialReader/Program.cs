using System;
using System.IO.Ports;

public class PortChat
{
  static bool _continue;
  static SerialPort _serialPort;
  //static DateTime StartTime;

  public static void Main()
  {
    string message;
    StringComparer stringComparer = StringComparer.OrdinalIgnoreCase;

    // Create a new SerialPort object with default settings.
    _serialPort = new SerialPort();

    // Allow the user to set the appropriate properties.
    _serialPort.PortName = "COM3";//= SetPortName(_serialPort.PortName);
    _serialPort.BaudRate = 9600;// SetPortBaudRate(_serialPort.BaudRate);
    _serialPort.Parity = Parity.None;// SetPortParity(_serialPort.Parity);
    _serialPort.DataBits = 8;// SetPortDataBits(_serialPort.DataBits);
    _serialPort.StopBits = StopBits.One;// SetPortStopBits(_serialPort.StopBits);
    _serialPort.Handshake = Handshake.None;// SetPortHandshake(_serialPort.Handshake);

    // Set the read/write timeouts
    _serialPort.ReadTimeout = SerialPort.InfiniteTimeout;
    _serialPort.WriteTimeout = SerialPort.InfiniteTimeout;

    _serialPort.DataReceived += _serialPort_DataReceived;
    _serialPort.Open();

    _continue = true;

    Console.WriteLine("Type QUIT to exit");

    while (_continue)
    {
      message = Console.ReadLine();

      if (stringComparer.Equals("quit", message))
      {
        _continue = false;
      } 
      else if (stringComparer.Equals("b", message))
      {
        BeginTransmitting();
      }
    }

    _serialPort.Close();
  }
  private static void BeginTransmitting()
  {
    MyLog("Begin transmitting");
    MyLog("Got: ");
    StateTx = StatesTx.SIGNAL_LENGTH_01;
    //StartTime = DateTime.Now;
    _serialPort.Write(new byte[] { 49 }, 0, 1);//49
  }

  private static UInt16[] RecievedValues;
  private static UInt32 ValueNum = 0;

  private static int bytePos = 0;
  private static byte[] ReadBuffer = new byte[2];
  private static UInt32 SignalLength;
  enum StatesTx : Byte
  {
    BEFORE_TX = 0,
    SIGNAL_LENGTH_01 = 1,
    SIGNAL_LENGTH_23 = 2,
    DATA = 3,
    END = 4,
  }
  private static StatesTx StateTx;

  private static void _serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
  {
    try
    {
      while (_serialPort.BytesToRead > 0)
      {
        int b = _serialPort.ReadByte(); MyLog($" - {bytePos} {b}");
        ReadBuffer[bytePos] = (byte)b;
        bytePos++;

        if (bytePos < ReadBuffer.Length) continue;

        bytePos = 0;

        ProcessBuffer();
      }
    }
    catch (TimeoutException) { }
  }

  private static void ProcessBuffer()
  {
    MyLog($"State is '{StateTx}'");
    switch (StateTx)
    {
      case StatesTx.SIGNAL_LENGTH_01:
        SignalLength = BitConverter.ToUInt16(ReadBuffer, 0);
        StateTx = StatesTx.SIGNAL_LENGTH_23;
        MyLog("");
        break;
      case StatesTx.SIGNAL_LENGTH_23:
        SignalLength |= ((UInt32)BitConverter.ToUInt16(ReadBuffer, 0) << 16);
        MyLog($"Signal length is '{SignalLength}'");
        RecievedValues = new UInt16[SignalLength];
        StateTx = StatesTx.DATA;
        MyLog("");
        break;
      case StatesTx.DATA:
        UInt16 value = BitConverter.ToUInt16(ReadBuffer, 0);
        Console.WriteLine($"Got '{value}': {ValueNum + 1}/{SignalLength}");
        RecievedValues[ValueNum] = value;
        checked { ++ValueNum; }
        MyLog("");
        _serialPort.Write(new byte[] { 51 }, 0, 1);

        if (ValueNum >= SignalLength)
        {
          StateTx = StatesTx.END;
          _serialPort.Close();

          Console.WriteLine($"Values: {RecievedValues.Length}");

          //TimeSpan ts = DateTime.Now - StartTime;
          //Console.WriteLine($"Elapsed time: {ts.TotalMilliseconds.ToString("G4")}");

          string s = Console.ReadLine();

          for (int i = 0; i < RecievedValues.Length; ++i)
            Console.WriteLine($" {RecievedValues[i]}");
        }
        break;
    }
  }
  
  private static void MyLog(string s)
  {
    //Console.WriteLine(s);
  }
}