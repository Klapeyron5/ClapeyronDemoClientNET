using System;
using System.Net;

namespace MetroFramework.Demo.UDPNode
{

    /// <summary>
    /// Надстройка над виртуальным udp сокетом (UDPSocket.cs).
    /// Обеспечивает протокольную обертку сообщений,
    /// механизм подтверждений доставки сообщений,
    /// обработку событий из udp-сокета.
    /// </summary>
    class UDPNode : UDPSocketListener
    {
        /// <summary>
        /// Виртуальный UDP socket для прослушки порта на входящие сообщения и отправки сообщений с этого порта.
        /// </summary>
        private static UDPSocket udpSocket;

        /// <summary>
        /// Одновременно запускает прослушку порта.
        /// </summary>
        /// <param name="inPort"></param>
        public UDPNode(int inPort)
        {
            startNode(inPort);
        }

        /// <summary>
        /// Запуск прослушки порта.
        /// </summary>
        /// <param name="inPort">порт для прослушки</param>
        private void startNode(int inPort)
        {
            udpSocket = new UDPSocket(inPort, this);
        }

        /// <summary>
        /// Закрытие любой возможности общения через эту ноду.
        /// </summary>
        public void closeNode()
        {
            udpSocket.closeNode();
        }

        /// <summary>
        /// Отправка вашей строки.
        /// </summary>
        /// <param name="outIP">IP адресата</param>
        /// <param name="outPort">порт адресата</param>
        /// <param name="data">строка для отправки</param>
        public void sendNewString(String outIP, int outPort, String data)
        {
            try
            {
                IPAddress hostAddr = IPAddress.Parse(outIP);
                Message message = new Message(data);
                udpSocket.sendMessage(hostAddr, outPort, message);
            }
            catch (FormatException e)
            {
                onSocketMessageIsNotSentCantFindRemoteURL(outIP);
            }
        }

        public void onSocketCreationException(UDPSocket udpSocket, string excMsg)
        {
            Program.writeLine("UDP socket error: cant create local socket =>\n=>" + excMsg);
        }

        public void onSocketListeningClosed(UDPSocket udpSocket)
        {
            Program.writeLine("UDP socket log: UDP listening is closed");
        }

        public void onSocketListeningException(UDPSocket udpSocket)
        {
            Program.writeLine("UDP socket error: UDP listening is closed UNEXPECTEDLY");
        }

        public void onSocketListeningReady(UDPSocket udpSocket)
        {
            Program.writeLine("Listening started on localhost: " + udpSocket.getLocalPort() +
                    " and on " + AppInfo.LocalIP + ": " + udpSocket.getLocalPort());
        }

        public void onSocketMessageIsNotSentCantFindRemoteURL(string outIP)
        {
            Program.writeLine("UDP socket error: cant find remote URL to send");
        }

        public void onSocketMessageIsNotSentIOException()
        {
            Program.writeLine("UDP socket error: cant send data, try again");
        }

        public void onSocketMessageIsNotSentLocalNodeIsClosed()
        {
            Program.writeLine("UDP socket error: cant send message, please, start node");
        }

        public void onSocketMessageReceived(IPAddress authorIP, int authorPort, string receivedString)
        {
            Program.writeLine("RECEIVED from " + authorIP + ":" + authorPort + "| data: " + receivedString);
        }

        public void onSocketMessageSent(IPAddress outIP, int outPort, Message data)
        {
            Program.writeLine("UDP socket log: Message sent");
        }

        public void onSocketReceivingException(UDPSocket udpSocket, string excMsg)
        {
            Program.writeLine("UDP socket error: cant receive msg =>\n=>" + excMsg);
        }
    }
}
