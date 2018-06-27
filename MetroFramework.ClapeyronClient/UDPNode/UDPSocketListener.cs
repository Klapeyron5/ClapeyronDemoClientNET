using System;
using System.Net;

namespace MetroFramework.Demo.UDPNode
{
    /// <summary>
    /// События, относящиеся к созданию udp-сокета, прослушке входящих подключений на заданном порту,
    /// отправке сообщениий с заданного порта.
    /// </summary>
    interface UDPSocketListener
    {
        /// <summary>
        /// UDPSocket начал прослушку заданного порта.
        /// </summary>
        /// <param name="udpSocket"></param>
        void onSocketListeningReady(UDPSocket udpSocket);

        /// <summary>
        /// Получено сообщение на заданный в UDPSocket порт.
        /// </summary>
        /// <param name="authorIP"></param>
        /// <param name="authorPort"></param>
        /// <param name="receivedString"></param>
        void onSocketMessageReceived(IPAddress authorIP, int authorPort, String receivedString);

        /// <summary>
        /// С заданного в UDPSocket порта отправлено сообщение.
        /// </summary>
        /// <param name="outIP"></param>
        /// <param name="outPort"></param>
        /// <param name="data"></param>
        void onSocketMessageSent(IPAddress outIP, int outPort, Message data);

        /// <summary>
        /// Это событие может вызваться ТОЛЬКО (я, блять, очень надеюсь) при ручном закрытии udpSocket.
        /// Т.е. вызова netProtocol.closeNode().
        /// </summary>
        /// <param name="udpSocket"></param>
        void onSocketListeningClosed(UDPSocket udpSocket);

        /// <summary>
        /// udpSocket != null, но отправлять сообщения не получится, т.к.
        /// udpSocket.localSocket.isClosed() == true, а также
        /// udpSocket.isAlive() == false
        /// </summary>
        /// <param name="udpSocket"></param>
        /// <param name="excMsg"></param>
        void onSocketCreationException(UDPSocket udpSocket, String excMsg);

        /// <summary>
        /// Вызывается только если прослушка должна продолжиться, т.е. это событие не означает закрытие сокета.
        /// </summary>
        /// <param name="udpSocket"></param>
        /// <param name="excMsg"></param>
        void onSocketReceivingException(UDPSocket udpSocket, String excMsg);

        /// <summary>
        /// Внезапная неожидаемая остановка прослушки.
        /// </summary>
        /// <param name="udpSocket"></param>
        void onSocketListeningException(UDPSocket udpSocket);

        /// <summary>
        /// Внезапная неожидаемая остановка прослушки.
        /// </summary>
        void onSocketMessageIsNotSentLocalNodeIsClosed();

        /// <summary>
        /// Что-то не так с адресом доставки.
        /// </summary>
        /// <param name="outIP"></param>
        void onSocketMessageIsNotSentCantFindRemoteURL(String outIP);

        /// <summary>
        /// Что-то не так с localSocket.send(), наверное, стоит еще раз попытаться отправить сообщение.
        /// </summary>
        void onSocketMessageIsNotSentIOException();
    }
}
