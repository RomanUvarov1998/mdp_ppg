using PPG_Database.KeepingModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace MDP_PPG.Converters
{
	public class Parent2ChildrenMessageConverter : IValueConverter
	{
		public string ChoseMessage { get; set; } = "Выберите элемент";
		public string EmptyMessage { get; set; } = "Пусто";

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			ModelBase parent = value as ModelBase;
			if (parent == null)
				return ChoseMessage;
			else
				return EmptyMessage;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
