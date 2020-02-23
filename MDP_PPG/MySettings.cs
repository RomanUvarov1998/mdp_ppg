using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace MDP_PPG
{
	public class MySettings
	{
		public static Brush GreenMark { get; set; } = new SolidColorBrush(Color.FromArgb(60, 125, 255, 0));
		public static Brush YellowMark { get; set; } = new SolidColorBrush(Color.FromArgb(60, 255, 255, 0));
		public static Brush RedMark { get; set; } = new SolidColorBrush(Color.FromArgb(60, 255, 125, 125));
	}
}
