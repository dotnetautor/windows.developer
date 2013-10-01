using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using BluetoothAppToDevice.Resources;
using BluetoothAppToDevice.Utilities;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Tasks;
using Windows.Networking.Proximity;

namespace BluetoothAppToDevice {
  public partial class MainPage : PhoneApplicationPage {
    private ObservableCollection<PairedDeviceInfo> _pairedDevices; // a local copy of paired device information 
    private readonly ConnectionManager _connectionManager;


    // Constructor
    public MainPage() {
      InitializeComponent();
      _connectionManager = new ConnectionManager();
      _connectionManager.Initialize();
      _connectionManager.MessageReceived += message =>
        {

          object[] attributes = message.Split(new [] { (char)44, (char)42 });
          if ((attributes.Length == 16) && (attributes[0].ToString() == "$GPGGA")) {
            Deployment.Current.Dispatcher.BeginInvoke(() => {
              tbResult.Text = string.Format("{1} | {2},{3} | {4},{5} - {6} \r\nSatelites:{7} - HDop:{8} Alt:{9} High:{10}", attributes);
            });
          }    
        };
    }

    private void MainPage_Loaded(object sender, RoutedEventArgs e) {
      // Bluetooth is not available in the emulator.  
      if (Microsoft.Devices.Environment.DeviceType == Microsoft.Devices.DeviceType.Emulator) {
        MessageBox.Show(AppResources.Msg_EmulatorMode, "Warning", MessageBoxButton.OK);
      }

      _pairedDevices = new ObservableCollection<PairedDeviceInfo>();
      PairedDevicesList.ItemsSource = _pairedDevices;
    }

    private void FindPairedDevices_Tap(object sender, GestureEventArgs e) {
      RefreshPairedDevicesList();
    }

    /// <summary> 
    /// Asynchronous call to re-populate the ListBox of paired devices. 
    /// </summary> 
    private async void RefreshPairedDevicesList() {
      try {
        // Search for all paired devices 
        PeerFinder.AlternateIdentities["Bluetooth:Paired"] = "";
        var peers = await PeerFinder.FindAllPeersAsync();

        // By clearing the backing data, we are effectively clearing the ListBox 
        _pairedDevices.Clear();

        if (peers.Count == 0) {
          MessageBox.Show(AppResources.Msg_NoPairedDevices);
        } else {
          // Found paired devices. 
          foreach (var peer in peers) {
            _pairedDevices.Add(new PairedDeviceInfo(peer));
          }
        }
      } catch (Exception ex) {
        if ((uint) ex.HResult == 0x8007048F) {
          var result = MessageBox.Show(AppResources.Msg_BluetoothOff, "Bluetooth Off", MessageBoxButton.OKCancel);
          if (result == MessageBoxResult.OK) {
            var connectionSettingsTask = new ConnectionSettingsTask {ConnectionSettingsType = ConnectionSettingsType.Bluetooth};
            connectionSettingsTask.Show();
          }
        } else if ((uint) ex.HResult == 0x80070005) {
          MessageBox.Show(AppResources.Msg_MissingCaps);
        } else {
          MessageBox.Show(ex.Message);
        }
      }
    }

    private void PairedDevicesList_SelectionChanged(object sender, SelectionChangedEventArgs e) {
      // Check whether the user has selected a device 
      if (PairedDevicesList.SelectedItem == null) {
        // No - hide these fields 
        ConnectToSelected.IsEnabled = false;
        ServiceNameInput.Visibility = Visibility.Collapsed;
      } else {
        // Yes - enable the connect button 
        ConnectToSelected.IsEnabled = true;

        // Show the service name field, if the ServiceName associated with this device is currently empty 
        var pdi = PairedDevicesList.SelectedItem as PairedDeviceInfo;
        ServiceNameInput.Visibility = (String.IsNullOrWhiteSpace(pdi.ServiceName)) ? Visibility.Visible : Visibility.Collapsed;
      }
    }

    private async void ConnectToSelected_Tap_1(object sender, GestureEventArgs e) {
       // Connect to the device 
      var pdi = PairedDevicesList.SelectedItem as PairedDeviceInfo;
      await _connectionManager.Connect(pdi.PeerInfo);
      var res = _connectionManager.SendCommand(Environment.NewLine);
    }

  }
}