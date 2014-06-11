using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Data;

namespace FileTransfer.Code {
  public sealed class DatabindingDebugConverter : IValueConverter {
  

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
      Debug.WriteLine("{0} - {1}",value,parameter);
      return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
      return value;
    }
  }
}
