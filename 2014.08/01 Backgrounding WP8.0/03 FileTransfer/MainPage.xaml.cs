using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Xml.Linq;
using Windows.Storage;
using FileTransfer.Code;
using Microsoft.Phone.BackgroundTransfer;
using Microsoft.Phone.Controls;

namespace FileTransfer {
  public partial class MainPage : PhoneApplicationPage {

    private ObservableCollection<DownloadItem> _items = new ObservableCollection<DownloadItem>(); 

    // Constructor
    public MainPage() {
      InitializeComponent();  
    }

    protected override void OnNavigatedTo(NavigationEventArgs e) {
      EnsureTransfersDirectory();

      _items = new ObservableCollection<DownloadItem>(GetDownloadItems());
      lsItems.ItemsSource = _items;

      // reattach to ongoning transfers
      foreach (var request in BackgroundTransferService.Requests) {
        var file = Path.GetFileName(request.DownloadLocation.OriginalString);
        var item = _items.SingleOrDefault(i => i.Filename == file);
        if (item == null) continue;

        request.TransferProgressChanged += (sender, args) => UpdateProgress(item, request);
        request.TransferStatusChanged += (sender, args) => UpdateTransferStatus(item, request);        
        UpdateTransferStatus(item, request);
        UpdateProgress(item, request);
      }
    }


    private async void BtnDownload_OnClick(object sender, RoutedEventArgs e) {
       // den Sender als Button verwenden
      var button = sender as Button;
      if (button != null) {
        // Der DataContext das Buttons enthält das Element der Liste
        // in diesem Beispiel ist das ein DownloadItem.
        var item = button.DataContext as DownloadItem;
        switch (button.Content as string) {
          case "Download":
            StartDownload(item);
            break;
          case "Cancel":
            ChancelDownload(item);
            break;
          case "Show":
            var shared = await ApplicationData.Current.LocalFolder.GetFolderAsync("shared");
            var folder = await shared.GetFolderAsync("transfers");
            StorageFile pdffile = await folder.GetFileAsync(item.Filename);

            // Launch the pdf file.
            await Windows.System.Launcher.LaunchFileAsync(pdffile);
            break;
        }
      }
    }

    public IEnumerable<DownloadItem> GetDownloadItems(string uri = "data.xml") {
      var data = XElement.Load(uri);
      return from f in data.Descendants("file")
             select new DownloadItem {
               Name = f.Attribute("name").Value,
               Filename = f.Attribute("filename").Value,
               Url = f.Attribute("url").Value
             };
    }

    private static void EnsureTransfersDirectory() {
      using (var isoStore = IsolatedStorageFile.GetUserStoreForApplication()) {
        if (!isoStore.DirectoryExists("shared/transfers/")) {
          isoStore.CreateDirectory("shared/transfers/");
        }
      }
    }


    private static void UpdateProgress(DownloadItem item, BackgroundTransferRequest request) {
      item.TotalBytes = request.TotalBytesToReceive;
      item.DownloadedBytes = request.BytesReceived;
    }

    private static void UpdateTransferStatus(DownloadItem item, BackgroundTransferRequest request) {
      switch (request.TransferStatus) {
        case TransferStatus.Transferring:
          item.Caption = "Cancel";
          break;
        case TransferStatus.Waiting:
          item.Caption = "Waiting";
          break;
        case TransferStatus.Completed:
          if (request.TransferStatus == TransferStatus.Completed && request.TransferError == null) {
            item.Caption = "Show";
          } else {
            item.Caption = "Download";
            
          }
          break;
        default:
          item.Caption = request.TransferStatus.ToString();
          break;
      }
    }

    public void StartDownload(DownloadItem item) {
      var request = new BackgroundTransferRequest(new Uri(item.Url, UriKind.Absolute)) {
        Method = "GET",
        DownloadLocation = new Uri("shared/transfers/" + item.Filename, UriKind.RelativeOrAbsolute)

      };

      request.TransferProgressChanged += (sender, args) => UpdateProgress(item, args.Request);
      request.TransferStatusChanged += (sender, args) => UpdateTransferStatus(item, request);

      try {
        BackgroundTransferService.Add(request);
      } catch (Exception exception) {
        MessageBox.Show("Fehler beim Landen: " + exception.Message);
      }

      item.TotalBytes = 100;
      item.DownloadedBytes = 0;

      item.Id = request.RequestId;

    }

    public void ChancelDownload(DownloadItem item) {
      var request = BackgroundTransferService.Find(item.Id);
      if (request == null) return;
      try {
        BackgroundTransferService.Remove(request);
      } catch (Exception exception) {
        MessageBox.Show("Fehler beim Abbrechen: " + exception.Message);
      }

      item.TotalBytes = 100;
      item.DownloadedBytes = 0;

      item.Id = null;
    }

    private  void BtnReset_OnClick(object sender, RoutedEventArgs e) {
      ////alle Dateien entfernen
      //var shared = await ApplicationData.Current.LocalFolder.GetFolderAsync("shared");
      //var folder = await shared.GetFolderAsync("transfers");

      //foreach (var storageFile in await folder.GetFilesAsync()) {
      //  await storageFile.DeleteAsync();
      //}

      // alle BackGroundRequests entfernen
      foreach (var request in BackgroundTransferService.Requests) {
        BackgroundTransferService.Remove(BackgroundTransferService.Find(request.RequestId));
      }

      // die Liste mit den Elementen neu laden
      _items = new ObservableCollection<DownloadItem>(GetDownloadItems());
      lsItems.ItemsSource = _items;
      
    }
  }
}