using System;
using System.IO.Ports;

namespace CockpitHardwareHUB_v2.Classes
{
    /// <summary>
    /// Gerencia a comunicação via porta serial (USB).
    /// Versão refatorada para implementar a interface ICommunicationManager.
    /// </summary>
    public class SerialPortManager : ICommunicationManager, IDisposable
    {
        private readonly SerialPort _serialPort;

        #region Implementação da Interface ICommunicationManager

        public string Id => _serialPort?.PortName ?? string.Empty;
        public bool IsConnected => _serialPort != null && _serialPort.IsOpen;
        public event EventHandler<string> DataReceived;
        public event EventHandler<bool> ConnectionStatusChanged;

        #endregion

        public SerialPortManager(string portName, int baudRate)
        {
            _serialPort = new SerialPort(portName, baudRate)
            {
                Parity = Parity.None,
                DataBits = 8,
                StopBits = StopBits.One,
                Handshake = Handshake.None,
                ReadTimeout = 500,
                WriteTimeout = 500
            };
            _serialPort.DataReceived += SerialPort_DataReceived;
        }

        public void Connect()
        {
            if (_serialPort == null || IsConnected) return;
            try
            {
                _serialPort.Open();
                ConnectionStatusChanged?.Invoke(this, true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error connecting to serial port {Id}: {ex.Message}");
                ConnectionStatusChanged?.Invoke(this, false);
            }
        }

        public void Disconnect()
        {
            if (!IsConnected) return;
            try
            {
                _serialPort.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error disconnecting from serial port {Id}: {ex.Message}");
            }
            finally
            {
                ConnectionStatusChanged?.Invoke(this, false);
            }
        }

        public void SendData(string data)
        {
            if (!IsConnected) return;
            try
            {
                _serialPort.WriteLine(data);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending data to {Id}: {ex.Message}");
            }
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                while (_serialPort.BytesToRead > 0)
                {
                    string line = _serialPort.ReadLine();
                    DataReceived?.Invoke(this, line.Trim());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error receiving data from {Id}: {ex.Message}");
            }
        }

        public void Dispose()
        {
            Disconnect();
            _serialPort?.Dispose();
        }

        public static string[] GetAvailablePorts()
        {
            return SerialPort.GetPortNames();
        }
    }
}