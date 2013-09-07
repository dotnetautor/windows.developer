using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Windows.Networking.Proximity;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace NFCSimpleChat {
  public partial class MainPage : PhoneApplicationPage {

    private StreamSocket _proximitySocket = null;
    private PeerInformation _requestingPeer;
    private DataReader _dataReader;
    private DataWriter _dataWriter;
    private bool _started = false;

    // Constructor
    public MainPage() {
      InitializeComponent();

    }

    protected override void OnNavigatedTo(NavigationEventArgs e) {
      DisplayNameTextBox.Text = PeerFinder.DisplayName;
      // /MainPage.xaml?ms_nfp_launchargs=Windows.Networking.Proximity.PeerFinder:StreamSocket

      // If activated from launch or from the background, create a peer connection.
      if (e.Uri.ToString().IndexOf("ms_nfp_launchargs=Windows.Networking.Proximity.PeerFinder:StreamSocket", StringComparison.InvariantCultureIgnoreCase) > -1) {
        AdvertiseForPeersButton_Click(null, null);
      }

    }

    protected override void OnNavigatingFrom(NavigatingCancelEventArgs e) {
      if (_started) {
        // Detach the callback handler (there can only be one PeerConnectProgress handler).
        PeerFinder.TriggeredConnectionStateChanged -= TriggeredConnectionStateChanged;
        // Detach the incoming connection request event handler.
        PeerFinder.Stop();
        CloseSocket();
        _started = false;
      }
    }


     private void WriteMessageText(string message, bool overwrite = false) {
       Dispatcher.BeginInvoke(() => {
            if (overwrite)
              tbResult.Text = message;
            else
              tbResult.Text += message;
          });
    }

    

    // Click event handler for "Advertise" button.
    private void AdvertiseForPeersButton_Click(object sender, RoutedEventArgs e) {
      if (_started) {
        WriteMessageText("Sie warten bereits auf eine Verbindung !\r\n");
        return;
      }

      PeerFinder.DisplayName = DisplayNameTextBox.Text;

      if ((PeerFinder.SupportedDiscoveryTypes & PeerDiscoveryTypes.Triggered) == PeerDiscoveryTypes.Triggered) {
        PeerFinder.TriggeredConnectionStateChanged += TriggeredConnectionStateChanged;

        WriteMessageText("Tappen Sie ein Gerät an, das auch auf eine Verbindung wartet.\n");
      } else {
        WriteMessageText("Antippen für eine Verbindung wird von Ihrem Gerät nicht unterstützt.\n");
      }

      if ((PeerFinder.SupportedDiscoveryTypes & PeerDiscoveryTypes.Browse) != PeerDiscoveryTypes.Browse) {
        WriteMessageText("Das Verbinden über W-Lan wird von Ihrem Gerät nicht unterstützt.\n");
      }

      PeerFinder.Start();
      _started = true;
    }

    private void TriggeredConnectionStateChanged(
        object sender,
        TriggeredConnectionStateChangedEventArgs e) {
      if (e.State == TriggeredConnectState.PeerFound) {
        WriteMessageText("Gerät in Reichweite gefunden.\n");
      }
      if (e.State == TriggeredConnectState.Completed) {
        Connected(e.Socket);
      }
    }

    private void Connected(StreamSocket socket) {
      // DataReader und Writer erstelen
      _dataReader = new DataReader(socket.InputStream);
      _dataWriter = new DataWriter(socket.OutputStream);
      WriteMessageText("Verbunden\r\n", true);
      ListenForIncomingMessage();
    }

    private async void ListenForIncomingMessage() {
      try {
        // Länge und Nachricht lesen
        uint sizeFieldCount = await _dataReader.LoadAsync(1);
        byte b = _dataReader.ReadByte();
        await _dataReader.LoadAsync(b);
         WriteMessageText( "\r\n<--" + _dataReader.ReadString(b));

        // Auf die nächste nachricht warten.
       ListenForIncomingMessage();
      } catch (Exception ex) {
        Dispatcher.BeginInvoke(() => MessageBox.Show("Verbindung unterbrochen oder beendet!"));
        CloseSocket();
      }
    }

    // Click event handler for "Stop" button.
    private void StopFindingPeersButton_Click(object sender, RoutedEventArgs e) {
      _started = false;
      PeerFinder.Stop();
      CloseSocket(); 
    }

    // Handle external connection requests.


    private async void btnSend_Click(object sender, RoutedEventArgs e) {

      // Länge der Nachricht
      uint commandSize = _dataWriter.MeasureString(tbSend.Text);
      _dataWriter.WriteByte((byte)commandSize);

      // Nachricht
      var sentCommandSize = _dataWriter.WriteString(tbSend.Text);
      await _dataWriter.StoreAsync();

       WriteMessageText("\r\n-->" + tbSend.Text);
      tbSend.Text = "";

    }

    private void CloseSocket() {
      if (_dataReader != null) {
        _dataReader.Dispose();
        _dataReader = null;
      }

      if (_dataWriter != null) {
        _dataWriter.Dispose();
        _dataWriter = null;
      }

      if (_proximitySocket != null) {
        _proximitySocket.Dispose();
        _proximitySocket = null;
      }
    }

  }
}