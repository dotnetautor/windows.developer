using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Windows;
using Microsoft.Phone.Controls;
using Windows.Networking.Proximity;
using Windows.Storage.Streams;

namespace NFCMessage {
  public partial class MainPage : PhoneApplicationPage {

    private long _publishedMessageId = -1;
    private long _subscriptionId = -1;

    private long _idUriWrite = -1;
    private long _idWinMime;
    private long _idTxtPlain;
    private long _launchAppPubId = -1;

    // Constructor
    public MainPage() {
      InitializeComponent();
      CheckForNfc();
    }


    private void CheckForNfc() {
      ProximityDevice device = ProximityDevice.GetDefault();
      if (device != null) {

        MessageBox.Show("NFC steht zur Verfügung");
        device.DeviceArrived += sender => {
            // Gerät in reichweite
          };

        device.DeviceDeparted += sender => {
            // Gerät hat reichweite verlassen
          };
      } else {
        MessageBox.Show(
          "Ihr Telefon hat entweder kein NFC oder NFC ist deaktiviert");
      }
    }

    private void btnPublish_Click(object sender, RoutedEventArgs e) {

      ProximityDevice device = ProximityDevice.GetDefault();

      // Prüfen ob NFC auf diesem Gerät verfügbar ist
      if (device != null) {
        if (_publishedMessageId > -1) device.StopPublishingMessage(_publishedMessageId);
        _publishedMessageId = device.PublishMessage("Windows.WindowsDeveloper.MessageType", "Hallo Welt via NFC");
      }

    }

    private void btnStopPublic_Click(object sender, RoutedEventArgs e) {
      ProximityDevice device = ProximityDevice.GetDefault();

      // Prüfen ob NFC auf diesem Gerät verfügbar ist
      if (device != null && _publishedMessageId > -1) {
        device.StopPublishingMessage(_publishedMessageId);
        _publishedMessageId = -1;
      }
    }

    private void btnSubscribe_Click(object sender, RoutedEventArgs e) {
      ProximityDevice device = ProximityDevice.GetDefault();

      // Prüfen ob NFC auf diesem Gerät verfügbar ist
      if (device != null) {
        if (_subscriptionId > -1) device.StopSubscribingForMessage(_subscriptionId);
        _subscriptionId = device.SubscribeForMessage(
          "Windows.WindowsDeveloper.MessageType",
          (proximityDevice, message) => Dispatcher.BeginInvoke(() =>
                                                               MessageBox.Show(
                                                                 "Nachricht empfangen " +
                                                                 message.DataAsString +
                                                                 " von " +
                                                                 proximityDevice.DeviceId)
                                          ));
      }
    }

    private void btnUnsubscribe_Click(object sender, RoutedEventArgs e) {
      ProximityDevice device = ProximityDevice.GetDefault();

      // Prüfen ob NFC auf diesem Gerät verfügbar ist
      if (device != null && _subscriptionId > -1) {
        device.StopSubscribingForMessage(_subscriptionId);
      }
    }

    private void btnWriteURI_Click(object sender, RoutedEventArgs e) {
     ProximityDevice device = ProximityDevice.GetDefault();

      if (device != null) {
        _idUriWrite = device.PublishBinaryMessage(
          "WindowsUri:WriteTag",
          Encoding.Unicode.GetBytes(tbUrl.Text).AsBuffer(),
          (proximityDevice, messageId) =>
            {
              Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                  device.StopPublishingMessage(_idUriWrite);
                  MessageBox.Show("Nachricht erfolgreich geschrieben !");
                });
            }
          );
      }
    }

    private void btnWriteLauch_Click(object sender, RoutedEventArgs e) {
     ProximityDevice device = ProximityDevice.GetDefault();

      if (device != null) {
        //var appId = Windows.ApplicationModel.Store.CurrentApp.AppId;
        var appId = "4bad5210-10de-4ae1-a10d-3bb5ce218d66";
        const string launchArgs = "user=default";

        string appName = "NFCMessage" + "!" + appId;

        string launchAppMessage = launchArgs + "\tWindows\t" + appName;

        var dataWriter = new DataWriter {
          UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf16LE
        };

        dataWriter.WriteString(launchAppMessage);
        _launchAppPubId = device.PublishBinaryMessage(
          "LaunchApp:WriteTag", dataWriter.DetachBuffer(), (proximityDevice, id) =>
                                                           Deployment.Current.Dispatcher.BeginInvoke(() =>
                                                             {
                                                               device.StopPublishingMessage(_launchAppPubId);
                                                               MessageBox.Show("Nachricht erfolgreich geschrieben !");
                                                             }));

      }
    }

    private void btnSubscribeWritable_Click(object sender, RoutedEventArgs e) {
      
        ProximityDevice device = ProximityDevice.GetDefault();

        // Prüfen ob NFC auf diesem Gerät verfügbar ist
        if (device != null) {
          if (_subscriptionId > -1) device.StopSubscribingForMessage(_subscriptionId);
          _subscriptionId = device.SubscribeForMessage("WriteableTag", (proximityDevice, message) => {
            var size = System.BitConverter.ToInt32(message.Data.ToArray(), 0);
            Deployment.Current.Dispatcher.BeginInvoke(() => {
              MessageBox.Show(string.Format("Beschreibbarer Tag, Größe:{0}Bytes", size));
            });
          });
        }
      }


    private void btnWinMime_Click(object sender, RoutedEventArgs e) {
      ProximityDevice device = ProximityDevice.GetDefault();

      if (device != null) {
        if (_subscriptionId > -1) device.StopSubscribingForMessage(_subscriptionId);
        _subscriptionId = device.SubscribeForMessage(
          "WindowsMime",
          (proximityDevice, message) =>
            {
              var buffer = message.Data.ToArray();
              int mimesize = 0;
              //suchen nach '\0' 
              for (mimesize = 0; mimesize < 256 && buffer[mimesize] != 0; ++mimesize) {}

              //MimeType bestimmen
              var mimeType = Encoding.UTF8.GetString(buffer, 0, mimesize).Trim();
              if (mimeType == "text/plain") {
                //convert data to string. This traitement depend on mimetype value.
                tbMime.Text = Encoding.UTF8.GetString(buffer, 256, buffer.Length - 256);
              }
            });
      }
    }

    private void btnWinMimePublish_Click(object sender, RoutedEventArgs e) {
      ProximityDevice device = ProximityDevice.GetDefault();

      if (device != null) {
        btnWinMimePublish.IsEnabled = false;
         _idWinMime = device.PublishBinaryMessage(
          "WindowsMime.text/plain",
          Encoding.UTF8.GetBytes(tbMime.Text).AsBuffer(),
          (proximityDevice, messageId) => {
            Deployment.Current.Dispatcher.BeginInvoke(() => {
              device.StopPublishingMessage(_idWinMime);
                MessageBox.Show("Nachricht erfolgreich geschrieben !");
                btnWinMimePublish.IsEnabled = true;
              });
          }
          );
      }
    }

    private void btnWriteMimePublish_Click(object sender, RoutedEventArgs e) {
      ProximityDevice device = ProximityDevice.GetDefault();

      if (device != null) {
        btnWriteMimePublish.IsEnabled = false;
        _idTxtPlain = device.PublishBinaryMessage(
          "WindowsMime:WriteTag.text/plain",
          Encoding.UTF8.GetBytes(tbMime.Text).AsBuffer(),
          (proximityDevice, messageId) => {
            Deployment.Current.Dispatcher.BeginInvoke(() => {
              device.StopPublishingMessage(_idTxtPlain);
                MessageBox.Show("Nachricht erfolgreich geschrieben !");
                btnWriteMimePublish.IsEnabled = true;
              });
          }
          );
      }
    }
  }
}