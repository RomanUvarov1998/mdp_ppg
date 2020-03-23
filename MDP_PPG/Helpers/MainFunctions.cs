using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDP_PPG.Helpers
{
	public static class MainFunctions
	{
		public static byte[] FromMcToDatabase(UInt16[] values)
		{
			byte[] bytes = new byte[values.Length * 2];
			Buffer.BlockCopy(values, 0, bytes, 0, bytes.Length);
			return bytes;
		}

		public static byte[] FromStringLinesFileToDatabase(string[] lines)
		{
			List<byte> list = new List<byte>();

			for (int i = 0; i < lines.Length; i += 4)
			{
				if (!int.TryParse(lines[i], out int value))
					throw new Exception($"Не удалось распознать строку №{i}, содержащую {lines[i]}");

				byte[] bs = BitConverter.GetBytes(value);

				list.AddRange(bs);
			}

			return list.ToArray();
		}
		public static byte[] FromCSV_FileToDatabase(string[] lines)
		{
			List<byte> list = new List<byte>();

			for (int i = 0; i < lines.Length; i += 4)
			{
				if (!int.TryParse(lines[i], out int value))
					throw new Exception($"Не удалось распознать строку №{i}, содержащую {lines[i]}");

				byte[] bs = BitConverter.GetBytes(value);

				list.AddRange(bs);
			}

			return list.ToArray();
		}

		public static byte[] FromBinFileToDatabase(byte[] bytes) => bytes;

		public static double[] FromDatabaseToAnalysis(byte[] bytes)
		{
			List<double> list = new List<double>();

			for (int i = 0; i < bytes.Length; i += 2)
			{
				UInt16 value = BitConverter.ToUInt16(bytes, i);
				list.Add(value);
			}

			return list.ToArray();
		}
	}
}
