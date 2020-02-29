using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDP_PPG.Helpers
{
	public static class MainFunctions
	{
		public static int[] GetFromBytesAsInts(byte[] bytes)
		{
			List<int> list = new List<int>();

			for (int i = 0; i < bytes.Length; i += 4)
			{
				int value = BitConverter.ToInt32(bytes, i);
				list.Add(value);
			}

			return list.ToArray();
		}

		public static byte[] GetFromStringLines(string[] lines)
		{
			List<byte> list = new List<byte>();

			for (int i = 0; i < lines.Length; i += 4)
			{
				int value = int.Parse(lines[i]);

				byte[] bs = BitConverter.GetBytes(value);

				list.AddRange(bs);
			}

			return list.ToArray();
		}
	}
}
