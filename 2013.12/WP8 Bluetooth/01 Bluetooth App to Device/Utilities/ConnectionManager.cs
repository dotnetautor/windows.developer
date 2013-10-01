using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.Proximity;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace BluetoothAppToDevice.Utilities {
  /// <summary>
  /// Class to control the bluetooth connection to the Device.
  /// </summary>
  public class ConnectionManager {
    private byte[] _buffer;
    private int _currentPos;

    private StreamSocket _socket;
    private DataWriter _dataWriter;
    private DataReader _dataReader;
    private BackgroundWorker _dataReadWorker;

    public delegate void MessageReceivedHandler(string message);
    public event MessageReceivedHandler MessageReceived;

  
    public void Initialize() {
      _socket = new StreamSocket();
      _dataReadWorker = new BackgroundWorker {WorkerSupportsCancellation = true};
      _dataReadWorker.DoWork += ReceiveMessages;

      _buffer = new byte[512];
      _currentPos = 0;
    }

    public void Terminate() {
      if (_socket != null) {
        _socket.Dispose();
      }
      if (_dataReadWorker != null) {
        _dataReadWorker.CancelAsync();
      }
    }

    public async Task Connect(PeerInformation peer) {
      if (_socket != null) {
        await _socket.ConnectAsync(peer.HostName, (String.IsNullOrWhiteSpace(peer.ServiceName)) ? "1" : peer.ServiceName);
        _dataReader = new DataReader(_socket.InputStream);
        _dataReadWorker.RunWorkerAsync();
        _dataWriter = new DataWriter(_socket.OutputStream);
      }
    }

    private async void ReceiveMessages(object sender, DoWorkEventArgs e) {
      try {
        while (true) {
          uint sizeFieldCount = await _dataReader.LoadAsync(1);
          if (sizeFieldCount != 1) {
            // The underlying socket was closed before we were able to read the whole data. 
            return;
          }
          
          byte b = _dataReader.ReadByte();
          _buffer[_currentPos++] = b;

          if (b == 0x0A) {
            if (MessageReceived != null) {
              MessageReceived(Encoding.UTF8.GetString(_buffer, 0, _currentPos));
            }
            _currentPos = 0;
          }
        }
      } catch (Exception ex) {
        Debug.WriteLine(ex.Message);
      }
    }

    public async Task<uint> SendCommand(string command) {
      uint sentCommandSize = 0;
      if (_dataWriter != null) {
        //uint commandSize = _dataWriter.MeasureString(command);
        //_dataWriter.WriteByte((byte)commandSize);
        sentCommandSize = _dataWriter.WriteString(command);
        await _dataWriter.StoreAsync();
      }
      return sentCommandSize;
    }
  }
}
