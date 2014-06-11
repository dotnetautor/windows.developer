using System;
using System.Globalization;
using System.Windows.Data;
using Microsoft.Phone.Scheduler;

namespace Notifications.Utilities {
  public sealed class TypeTextConverter : IValueConverter {
   

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
      var t = value.GetType();
      if (t == typeof (Alarm)) {
        return "Alarm";
      } else if (t == typeof (Reminder)) {
        return "Reminder";
      }
      return "<Unknown>";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
     return null;
    }
  }
}
