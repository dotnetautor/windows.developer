using System;
using System.Linq;
using System.Windows;
using App2Aapp.Resources;
using App2Aapp.Utilities;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Tasks;
using Windows.Networking.Proximity;

namespace App2Aapp {
  public partial class MainPage : PhoneApplicationPage {

    private readonly ConnectionManager _connectionManager;

    // Constructor
    public MainPage() {
      InitializeComponent();

      PeerFinder.Start();

      _connectionManager = new ConnectionManager();
      _connectionManager.MessageReceived += message => 
        Deployment.Current.Dispatcher.BeginInvoke(() => {
          tbResult.Text += string.Format("-> {0}\r\n", message);
      });
      _connectionManager.PeerConnected += () => 
        Deployment.Current.Dispatcher.BeginInvoke(() => 
          ToggleUI(true));
    }


    private async void btnRefresh_Click(object sender, System.Windows.RoutedEventArgs e) {
      try {

        var peers = await PeerFinder.FindAllPeersAsync();
        PeerList.ItemsSource = (peers ?? new PeerInformation[] {}).Select(p => new PeerAppInfo(p));
      } catch (Exception ex) {
        if ((uint)ex.HResult == 0x8007048F) {
          var result = MessageBox.Show(AppResources.Err_BluetoothOff, AppResources.Err_BluetoothOffCaption, MessageBoxButton.OKCancel);
          if (result == MessageBoxResult.OK) {
            var connectionSettingsTask = new ConnectionSettingsTask { ConnectionSettingsType = ConnectionSettingsType.Bluetooth };
            connectionSettingsTask.Show();
          }
        } else if ((uint)ex.HResult == 0x80070005) {
          // You should remove this check before releasing. 
          MessageBox.Show(AppResources.Err_MissingCaps);
        } else if ((uint)ex.HResult == 0x8000000E) {
          MessageBox.Show(AppResources.Err_NotAdvertising);
        } else {
          MessageBox.Show(ex.Message);
        }
      }
    }

    private async void PeerList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e) {
      // Connect to the selected peer.
      var pdi = PeerList.SelectedItem as PeerAppInfo;
      var peer = pdi.PeerInfo;

      var res = await _connectionManager.ConnectToPeer(peer);
      tbResult.Text = (res) ? "Sucessfull connected\r\n" : "Could not connect \r\n";
      ToggleUI(res);

    }

    private void ToggleUI(bool enableChat = true) {
      btnRefresh.Visibility = (enableChat) ? Visibility.Collapsed : Visibility.Visible;
      PeerList.Visibility = (enableChat) ? Visibility.Collapsed: Visibility.Visible;
      sendUi.Visibility = (enableChat) ? Visibility.Visible : Visibility.Collapsed;
      tbResult.Visibility = (enableChat) ? Visibility.Visible : Visibility.Collapsed;
    }

    private void BtnSend_OnClick(object sender, RoutedEventArgs e) {
      tbResult.Text += string.Format("<- {0}\r\n", tbMsg.Text);
      _connectionManager.SendMessage(tbMsg.Text);

    }

    private void MainPage_OnLoaded(object sender, RoutedEventArgs e) {
      if (Microsoft.Devices.Environment.DeviceType == Microsoft.Devices.DeviceType.Emulator) {
        MessageBox.Show(AppResources.Msg_EmulatorMode, "Warning", MessageBoxButton.OK);
      }

    }
  }
}