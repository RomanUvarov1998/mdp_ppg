using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Threading;

public class PortChat
{
  static bool _continue;
  static SerialPort _serialPort;

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

  private static Dictionary<UInt32, UInt16> RecievedValues = new Dictionary<UInt32, UInt16>();


  private static int bytePos = 0;
  private static byte[] ReadBuffer = new byte[7];
  private static UInt32 SignalLength;
  private static MessageTypes GetMessageType()
  {
    Byte t = (Byte)BitConverter.ToUInt16(ReadBuffer, 0);
    switch (t)
    {
      case (Byte)MessageTypes.BEFORE_TX:   return MessageTypes.BEFORE_TX;
      case (Byte)MessageTypes.SIGNAL_LENGTH: return MessageTypes.SIGNAL_LENGTH;
      case (Byte)MessageTypes.DATA: return MessageTypes.DATA;
      case (Byte)MessageTypes.APPROVING:  return MessageTypes.APPROVING;
      case (Byte)MessageTypes.DONE:  return MessageTypes.DONE;
      default: throw new Exception("Wrong frame type ((((");
    }
  }
  enum MessageTypes : Byte
  {
    BEFORE_TX = 1,
    SIGNAL_LENGTH = 2,
    DATA = 3,
    APPROVING = 4,
    DONE = 5
  }


  //Get Signal Value
  //0: type, 1-4: frame number, 5-6: value
  private static UInt32 GetFrameNum() => BitConverter.ToUInt32(ReadBuffer, 1);
  private static UInt16 GetValue() => BitConverter.ToUInt16(ReadBuffer, 5);


  //Get Approving Message
  //0: type, 1-4: frame number, 5-6: nothing  
  private static UInt32 GetApprovingFrameNum() => BitConverter.ToUInt32(ReadBuffer, 1);


  //Get Signal Size
  //0: type, 1-4: signal size, 5-6: nothing
  private static UInt32 GetSignalLength() => BitConverter.ToUInt32(ReadBuffer, 1);


  private static void ApproveGettingValue(UInt32 frameNum)
  {
    MyLog($"! {frameNum}");
    _serialPort.Write(new byte[] { 51 }, 0, 1);
  }
  private static void BeginTransmitting()
  {
    MyLog("Begin transmitting");
    MyLog("Got: ");
    _serialPort.Write(new byte[] { 49 }, 0, 1);//49
  }

  private static void _serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
  {
    try
    {
      if (bytePos < ReadBuffer.Length)
      {
        int b = _serialPort.ReadByte();
        MyLog($" - {bytePos} {b}");
        ReadBuffer[bytePos] = (byte)b;
        bytePos++;
        ApproveGettingValue(0);
      }

      if (bytePos >= ReadBuffer.Length)
      {
        bytePos = 0;
      }
      else return;

      MyLog("");

      UInt16 value;
      UInt32 frameNum;
      MessageTypes messageType = GetMessageType();
      MyLog($"State is '{messageType}'");
      switch (messageType)
      {
        case MessageTypes.SIGNAL_LENGTH:
          SignalLength = GetSignalLength();
          MyLog($"Signal length is '{SignalLength}'");
          break;
        case MessageTypes.DATA:
          value = GetValue();
          frameNum = GetFrameNum();

          if (!RecievedValues.ContainsKey(frameNum))
          {
            RecievedValues.Add(frameNum, value);
            MyLog($"Got №{frameNum} '{value}'");
            Console.WriteLine($"{100 * RecievedValues.Count / SignalLength}% done");
          }
          else if (frameNum > SignalLength)
          {
            Console.Write($"{frameNum} is more than {SignalLength}!!!");
          }
          else
          {
            Console.Write($"Already have №{frameNum} '{value}'!!!");
          }
          break;
        case MessageTypes.APPROVING:
          frameNum = GetApprovingFrameNum();
          if (RecievedValues.ContainsKey(frameNum))
          {
            ApproveGettingValue(frameNum);
          }
          break;
        case MessageTypes.DONE:
          SortedDictionary<UInt32, UInt16> sd = new SortedDictionary<uint, ushort>(RecievedValues);
          UInt16[] values = sd.Values.ToArray();

          MyLog($"Values: {values.Length}");

          string s = Console.ReadLine();

          for (int i = 0; i < values.Length; ++i)
            MyLog($" {values[i]}");
          break;
      }
    }
    catch (TimeoutException) { }
  }

  private static void MyLog(string s)
  {
    //Console.WriteLine(s);
  }
}