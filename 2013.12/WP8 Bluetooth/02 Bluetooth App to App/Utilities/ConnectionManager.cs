using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using App2Aapp.Resources;
using Windows.Networking.Proximity;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace App2Aapp.Utilities {
  public class PeerAppInfo
    {
        internal PeerAppInfo(PeerInformation peerInformation)
        {
            this.PeerInfo = peerInformation;
            this.DisplayName = this.PeerInfo.DisplayName;
        }

        public string DisplayName { get; private set; }
        public PeerInformation PeerInfo { get; private set; }
    }
}

public class ConnectionManager {
  private StreamSocket _socket;
  private string _peerName = string.Empty;

  public delegate void MessageReceivedHandler(string message);
  public event MessageReceivedHandler MessageReceived;

  public delegate void PeerConnectedHandler();
  public event PeerConnectedHandler PeerConnected;


  private DataWriter _dataWriter;
  private DataReader _dataReader;
  private readonly BackgroundWorker _dataReadWorker;

  public ConnectionManager() {

    PeerFinder.ConnectionRequested += PeerFinder_ConnectionRequested;
    _dataReadWorker = new BackgroundWorker {WorkerSupportsCancellation = true};
    _dataReadWorker.DoWork += async (sender, args) =>
      {
        try {
          while (true) {
            // Jede Nachricht hat eine Länge (Int32)
            await _dataReader.LoadAsync(4);
            var messageLen = (uint) _dataReader.ReadInt32();
            await _dataReader.LoadAsync(messageLen);
            if (MessageReceived != null) {
              MessageReceived(_dataReader.ReadString(messageLen));
            }
          }
        } catch (Exception ex) {
          Debug.WriteLine(ex.Message);
        }
      };

  }

  private void PeerFinder_ConnectionRequested(object sender, ConnectionRequestedEventArgs args) {
    try {
      Deployment.Current.Dispatcher.BeginInvoke(async () =>
        {
          // Den Benutzer fragen ob die Verbindung aufgebaut werden soll.
          var result = MessageBox.Show(String.Format(AppResources.Msg_ChatPrompt, args.PeerInformation.DisplayName)
                                       , AppResources.Msg_ChatPromptTitle, MessageBoxButton.OKCancel);
          if (result == MessageBoxResult.OK) {
            if (await ConnectToPeer(args.PeerInformation)) {
              if (PeerConnected != null) {
                PeerConnected();
              }
            };
          } else {
            // die API hat keine Funktion, die aufgerufen werden kann, um eine Verbindung abzulehnen.
          }
        });
    } catch (Exception ex) {
      MessageBox.Show(ex.Message);
      Terminate();
    }
  }


  public async Task<bool> ConnectToPeer(PeerInformation peer) {
    try {
      _socket = await PeerFinder.ConnectAsync(peer);

      if (_socket != null) {
        // Den PeerFinder stoppen, um Energie zu sparen ;-)
        PeerFinder.Stop();
        _peerName = peer.DisplayName;
        _dataReader = new DataReader(_socket.InputStream);
        _dataWriter = new DataWriter(_socket.OutputStream);
        _dataReadWorker.RunWorkerAsync();
        return true;
      }

    } catch (Exception ex) {
      MessageBox.Show(ex.Message);
      Terminate();
    }
    return false;

  }

  public async void SendMessage(string message) {
    _dataWriter.WriteInt32(message.Length);
    await _dataWriter.StoreAsync();

    _dataWriter.WriteString(message);
    await _dataWriter.StoreAsync();
  }

  public void Terminate() {
    if (_socket != null) {
      _socket.Dispose();
    }
    if (_dataReadWorker != null) {
      _dataReadWorker.CancelAsync();
    }
  }
}



