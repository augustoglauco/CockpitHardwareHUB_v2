using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CockpitHardwareHUB_v2.Classes
{
    /// <summary>
    /// Gerencia a comunicação via TCP/IP para dispositivos como o ESP32.
    /// Implementa a interface ICommunicationManager.
    /// </summary>
    public class TcpIpCommunicationManager : ICommunicationManager, IDisposable
    {
        private TcpClient _tcpClient;
        private NetworkStream _networkStream;
        private readonly string _ipAddress;
        private readonly int _port;
        private CancellationTokenSource _cancellationTokenSource;

        #region Implementação da Interface ICommunicationManager
        
        public string Id => $"{_ipAddress}:{_port}";
        public bool IsConnected => _tcpClient != null && _tcpClient.Connected;
        public event EventHandler<string> DataReceived;
        public event EventHandler<bool> ConnectionStatusChanged;

        #endregion

        public TcpIpCommunicationManager(string ip, int p)
        {
            _ipAddress = ip;
            _port = p;
        }

        public async void Connect()
        {
            if (IsConnected) return;
            try
            {
                _tcpClient = new TcpClient();
                await _tcpClient.ConnectAsync(_ipAddress, _port);
                if (_tcpClient.Connected)
                {
                    _networkStream = _tcpClient.GetStream();
                    _cancellationTokenSource = new CancellationTokenSource();
                    Task.Run(() => ListenForData(_cancellationTokenSource.Token));
                    ConnectionStatusChanged?.Invoke(this, true);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error connecting via TCP/IP to {Id}: {ex.Message}");
                ConnectionStatusChanged?.Invoke(this, false);
            }
        }

        public void Disconnect()
        {
            if (!IsConnected) return;
            try
            {
                _cancellationTokenSource?.Cancel();
                _networkStream?.Close();
                _tcpClient?.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error disconnecting from {Id}: {ex.Message}");
            }
            finally
            {
                _tcpClient = null;
                ConnectionStatusChanged?.Invoke(this, false);
            }
        }

        public void SendData(string data)
        {
            if (!IsConnected || _networkStream == null) return;
            try
            {
                byte[] buffer = Encoding.UTF8.GetBytes(data + "\n");
                _networkStream.Write(buffer, 0, buffer.Length);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending data to {Id}: {ex.Message}");
                Disconnect();
            }
        }

        private async Task ListenForData(CancellationToken token)
        {
            var reader = new StreamReader(_networkStream, Encoding.UTF8);
            while (IsConnected && !token.IsCancellationRequested)
            {
                try
                {
                    string line = await reader.ReadLineAsync();
                    if (line != null)
                    {
                        DataReceived?.Invoke(this, line.Trim());
                    }
                    else { break; } // Conexão fechada pelo outro lado
                }
                catch (IOException) { break; } // Conexão fechada
                catch (Exception ex)
                {
                    if (!token.IsCancellationRequested)
                    {
                        Console.WriteLine($"Error reading data from {Id}: {ex.Message}");
                    }
                    break;
                }
            }
            if (IsConnected) Disconnect();
        }

        public void Dispose()
        {
            Disconnect();
            _cancellationTokenSource?.Dispose();
        }
    }
}