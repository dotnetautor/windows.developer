using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace FileTransfer.Code {
  public sealed class LongToVisibilityConverter : IValueConverter {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
      var total = parameter as long? ?? -1;
      var actual = value as long? ?? -1;

      return ( actual > 0 && (total < 0 || actual < total )) ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
      return value ;
    }
  }
}