using System;

namespace CockpitHardwareHUB_v2.Classes
{
    /// <summary>
    /// Define o contrato para todas as classes gerenciadoras de comunicação.
    /// Garante que qualquer tipo de comunicação (Serial, TCP/IP, etc.) 
    /// tenha uma forma padronizada de ser controlada pela aplicação.
    /// </summary>
    public interface ICommunicationManager
    {
        /// <summary>
        /// Um identificador único para esta conexão (ex: "COM3" ou "192.168.1.101:8080").
        /// Essencial para identificar o dispositivo na UI e nos dicionários de gerenciamento.
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Indica se a conexão está atualmente ativa.
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// Inicia e estabelece a conexão com o dispositivo.
        /// </summary>
        void Connect();

        /// <summary>
        /// Encerra a conexão com o dispositivo.
        /// </summary>
        void Disconnect();

        /// <summary>
        /// Envia uma string de dados/comando para o dispositivo conectado.
        /// </summary>
        /// <param name="data">O comando a ser enviado.</param>
        void SendData(string data);

        /// <summary>
        /// Evento disparado quando novos dados são recebidos do dispositivo.
        /// A string contém o comando recebido.
        /// </summary>
        event EventHandler<string> DataReceived;

        /// <summary>
        /// Evento disparado quando o status da conexão muda (conectado/desconectado).
        /// O booleano indica o novo estado (true = conectado, false = desconectado).
        /// </summary>
        event EventHandler<bool> ConnectionStatusChanged;
    }
}