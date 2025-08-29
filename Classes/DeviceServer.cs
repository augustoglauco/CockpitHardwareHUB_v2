using System;

namespace CockpitHardwareHUB_v2.Classes
{
    /// <summary>
    /// Classe de gerenciamento para um único dispositivo de hardware.
    /// Atua como uma camada intermediária entre a UI e o gerenciador de comunicação.
    /// Versão refatorada para ser agnóstica ao tipo de comunicação, usando ICommunicationManager.
    /// </summary>
    public class DeviceServer
    {
        private readonly ICommunicationManager _communicationManager;

        /// <summary>
        /// ID do dispositivo, obtido diretamente do gerenciador de comunicação (ex: "COM3" ou "192.168.1.101").
        /// </summary>
        public string Id => _communicationManager.Id;

        /// <summary>
        /// Status da conexão do dispositivo.
        /// </summary>
        public bool IsRunning => _communicationManager.IsConnected;

        /// <summary>
        /// Evento que repassa os dados recebidos do gerenciador de comunicação para a camada superior (Form1).
        /// </summary>
        public event EventHandler<string> DataReceived;

        /// <summary>
        /// Evento que repassa as mudanças de status da conexão para a camada superior (Form1).
        /// </summary>
        public event EventHandler<bool> StatusChanged;


        /// <summary>
        /// Construtor que recebe um gerenciador de comunicação já instanciado (Injeção de Dependência).
        /// </summary>
        /// <param name="communicationManager">Uma instância de um comunicador (ex: SerialPortManager ou TcpIpCommunicationManager)</param>
        public DeviceServer(ICommunicationManager communicationManager)
        {
            _communicationManager = communicationManager ?? throw new ArgumentNullException(nameof(communicationManager));

            // Inscreve-se nos eventos do comunicador para poder repassá-los
            _communicationManager.DataReceived += OnCommunicationManager_DataReceived;
            _communicationManager.ConnectionStatusChanged += OnCommunicationManager_ConnectionStatusChanged;
        }

        /// <summary>
        /// Inicia a conexão com o dispositivo chamando o método Connect do comunicador.
        /// </summary>
        public void Start()
        {
            _communicationManager.Connect();
        }

        /// <summary>
        /// Interrompe a conexão com o dispositivo.
        /// </summary>
        public void Stop()
        {
            _communicationManager.Disconnect();
        }
        
        /// <summary>
        /// Envia dados para o dispositivo através do comunicador.
        /// </summary>
        public void SendData(string data)
        {
            _communicationManager.SendData(data);
        }

        /// <summary>
        /// Handler interno que é acionado quando o comunicador recebe dados.
        /// Ele simplesmente repassa o evento para cima.
        /// </summary>
        private void OnCommunicationManager_DataReceived(object sender, string data)
        {
            // Dispara o evento público do DeviceServer
            DataReceived?.Invoke(this, data);
        }
        
        /// <summary>
        /// Handler interno que é acionado quando o status da conexão do comunicador muda.
        /// Ele repassa o evento para cima.
        /// </summary>
        private void OnCommunicationManager_ConnectionStatusChanged(object sender, bool isConnected)
        {
            // Dispara o evento público do DeviceServer
            StatusChanged?.Invoke(this, isConnected);
        }
    }
}