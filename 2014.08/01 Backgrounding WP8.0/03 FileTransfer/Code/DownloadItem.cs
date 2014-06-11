using System.ComponentModel;
using System.Runtime.CompilerServices;
using FileTransfer.Annotations;

namespace FileTransfer.Code {
  public class DownloadItem : INotifyPropertyChanged {
    private long _totalBytes;
    private long _downloadedBytes;
    private string _caption;

    public DownloadItem () {
      Caption = "Download";
      TotalBytes = 100;
      DownloadedBytes = 0;
    }

    public string Id { get; set; }
    public string Name { get; set; }
    public string Filename { get; set; }
    public string Url { get; set; }

    // used for UI 

    public long TotalBytes {
      get { return _totalBytes; }
      set {
        if (value == _totalBytes) return;
        _totalBytes = value;
        OnPropertyChanged();
      }
    }

    public long DownloadedBytes {
      get { return _downloadedBytes; }
      set {
        if (value == _downloadedBytes) return;
        _downloadedBytes = value;
        OnPropertyChanged();
      }
    }

    public string Caption {
      get { return _caption; }
      set {
        if (value == _caption) return;
        _caption = value;
        OnPropertyChanged();
      }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    [NotifyPropertyChangedInvocator]
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
      PropertyChangedEventHandler handler = PropertyChanged;
      if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
    }
  }
}
