using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDP_PPG.Helpers
{
	public static class MainFunctions
	{
		public static byte[] FromMcToDatabase(double[] values)
		{
			byte[] bytes = new byte[values.Length * sizeof(double)];
			Buffer.BlockCopy(values, 0, bytes, 0, bytes.Length);
			return bytes;
		}

		//public static byte[] FromStringLinesFileToDatabase(string[] lines)
		//{
		//	List<byte> list = new List<byte>();

		//	for (int i = 0; i < lines.Length; i += 4)
		//	{
		//		if (!int.TryParse(lines[i], out int value))
		//			throw new Exception($"Не удалось распознать строку №{i}, содержащую {lines[i]}");

		//		byte[] bs = BitConverter.GetBytes(value);

		//		list.AddRange(bs);
		//	}

		//	return list.ToArray();
		//}
		//public static byte[] FromCSV_FileToDatabase(string[] lines)
		//{
		//	List<byte> list = new List<byte>();

		//	for (int i = 0; i < lines.Length; i += 4)
		//	{
		//		if (!int.TryParse(lines[i], out int value))
		//			throw new Exception($"Не удалось распознать строку №{i}, содержащую {lines[i]}");

		//		byte[] bs = BitConverter.GetBytes(value);

		//		list.AddRange(bs);
		//	}

		//	return list.ToArray();
		//}

		//public static byte[] FromBinFileToDatabase(byte[] bytes) => bytes;

		public static double[] FromDatabaseToAnalysis(byte[] bytes)
		{
			int size = sizeof(double);

			if (bytes.Length % size != 0) throw new Exception("Недостаточно байт в массиве (((");

			double[] values = new double[bytes.Length / size];

			int dPos = 0;
			for (int i = 0; i < bytes.Length; i += size)
			{
				values[dPos] = BitConverter.ToDouble(bytes, i);
				++dPos;
			}

			return values;
		}
	}
}
