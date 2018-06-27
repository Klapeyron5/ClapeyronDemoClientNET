using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace MetroFramework.Demo.UDPNode
{
    class UDPSocket
    {
        /// <summary>
        /// Сюда кидаем все события, происходящие в сокете.
        /// </summary>
        private UDPSocketListener udpSocketListener;

        /// <summary>
        /// Через эту переменную осуществляется непосредственно доступ к порту.
        /// Доступ рекомендуется только с помощью synchronized (специально для этого есть getLocalSocket()).
        /// </summary>
        private static UdpClient localSocket;

        /// <summary>
        /// Поток прослушки входящих сообщений (на заданном порту).
        /// </summary>
        private Thread listeningThread;

        /// <summary>
        /// Основная переменная, контролирующая жизнь сокета. Нельзя извне обратно включить сокет,
        /// если его выключали. Только с помощью создания нового через конструктор.
        /// </summary>
        private bool alive;

        /// <summary>
        /// Запускает прослушку заданного порта.
        /// </summary>
        /// <param name="inPort"></param>
        /// <param name="udpSocketListener"></param>
        public UDPSocket(int inPort, UDPSocketListener udpSocketListener)
        {
            this.udpSocketListener = udpSocketListener;
            try
            {
                localSocket = new UdpClient(inPort);//SocketExc
                startNode();
            }
            catch (ArgumentOutOfRangeException e)
            { //localSocket == null; listeningThread == null
                udpSocketListener.onSocketCreationException(this, "The port parameter is greater than MaxPort or less than MinPort.");
                setAlive(false);
            }
            catch (SocketException e)
            { //localSocket == null; listeningThread == null
                udpSocketListener.onSocketCreationException(this, e.Message);
                setAlive(false);
            }
        }

        /// <summary>
        /// Запускает прослушку заданного порта.
        /// </summary>
        private void startNode()
        {
            listeningThread = new Thread(() => listeningNodeThreadMethod());
            listeningThread.Start();
            setAlive(true);
            udpSocketListener.onSocketListeningReady(this);
        }

        /// <summary>
        /// Костыльная хрень - метод для потока прослушки.
        /// В яве можно инлайн а тут нет, отстой.
        /// </summary>
        private void listeningNodeThreadMethod()
        {
            IPEndPoint remoteIpEndPoint = null;

            while (isAlive())
            {
                try
                {
                    byte[] receivedData = getLocalSocket().Receive(ref remoteIpEndPoint);

                    if (receivedData != null)
                    {
                        String receivedString = Encoding.UTF8.GetString(receivedData);
                        udpSocketListener.onSocketMessageReceived(remoteIpEndPoint.Address, remoteIpEndPoint.Port, receivedString);
                    }
                    else
                    {
                        Program.writeLine("receivedData == null");
                    }
                }
                catch (ObjectDisposedException e)
                {
                    udpSocketListener.onSocketReceivingException(this, "The Udp Socket has been closed. " + e.Message);
                }
                catch (SocketException e)
                {
                    string log = "";
                    if (remoteIpEndPoint == null) log = "null";
                    else log = "remoteIpEndPoint: " + remoteIpEndPoint.Address + ":" + remoteIpEndPoint.Port;
                    udpSocketListener.onSocketReceivingException(this, log + "|" + e.Message);
                }
                //    else
                //      udpSocketListener.onSocketListeningClosed(this);
            }
            if (isAlive())
                udpSocketListener.onSocketListeningException(this);
            else
                udpSocketListener.onSocketListeningClosed(this);
        }

        /// <summary>
        /// Останавливает прослушку заданного порта, а также закрывает сокет. Для старта новых коммуникаций, надо создавать класс заново.
        /// </summary>
        public void closeNode()
        {
            if (isAlive())
            {
                Program.writeLine("Closing UDP listening..."); //TODO just log
                setAlive(false);
                getLocalSocket().Close();
                listeningThread.Join();
            }
            else
            {
                Program.writeLine("Tried to close node udp socket, but it's already closed"); //TODO just log
            }
        }

        /// <summary>
        /// Отправляет сообщение заданному адресату.
        /// </summary>
        /// <param name="outIP">IP адресата</param>
        /// <param name="outPort">порт адресата</param>
        /// <param name="message">форматированное сообщение</param>
        public void sendMessage(IPAddress outIP, int outPort, Message message)
        {
            byte[] sendData = Encoding.UTF8.GetBytes(message.ToString());  //TODO стандартизировать размеры https://ru.wikipedia.org/wiki/UDP (длина датаграммы)
            try
            {
                if (isAlive())
                {
                    getLocalSocket().Send(sendData, sendData.Length, new IPEndPoint(outIP, outPort));
                    udpSocketListener.onSocketMessageSent(outIP, outPort, message);
                }
                else
                {
                    udpSocketListener.onSocketMessageIsNotSentLocalNodeIsClosed();
                }
            }
            catch (Exception e) //TODO
            {
                udpSocketListener.onSocketMessageIsNotSentIOException();
            }
        }

        private object locker = new object();
        /// <summary>
        /// Осуществляйте доступ к UDP сокету (localSocket) через этот потокобезопасный метод.
        /// </summary>
        /// <returns></returns>
        private UdpClient getLocalSocket()
        {
            lock (locker)
            {
                return localSocket;
            }
        }

        /// <summary>
        /// Значение прослушивающегося порта.
        /// </summary>
        /// <returns></returns>
        public int getLocalPort()
        {
            return ((IPEndPoint)getLocalSocket().Client.LocalEndPoint).Port;
        }

        /// <summary>
        /// Если true, то работает прослушка заданного порта и возможность отправки сообщений.
        /// Иначе не работает ничего из вышеперечисленного и надо создавать экземпляр класса заново, если хотите коммуницировать.
        /// </summary>
        public bool isAlive()
        {
            lock (locker)
            {
                return alive;
            }
        }

        private void setAlive(bool isAlive)
        {
            lock (locker)
            {
                alive = isAlive;
            }
        }
    }
}
