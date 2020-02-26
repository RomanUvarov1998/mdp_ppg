using System.Windows;

namespace MDP_PPG.ViewModels
{
	public class PlotDataPoint
	{
		public PlotDataPoint(Point displayPoint, Point valueTime)
		{
			DisplayPoint = displayPoint;
			ValueTime = valueTime;
		}
		public Point DisplayPoint { get; }
		public Point ValueTime { get; }
	}
}
