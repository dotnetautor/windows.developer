using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.ApplicationModel;

namespace BackgroundTaskDemo.Code {
  public class ViewModelBase : INotifyPropertyChanged {
    public static bool IsInDesignMode {
      get { return DesignMode.DesignModeEnabled; }
    }
    
    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
      PropertyChangedEventHandler handler = PropertyChanged;
      if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
    }
  }
}
