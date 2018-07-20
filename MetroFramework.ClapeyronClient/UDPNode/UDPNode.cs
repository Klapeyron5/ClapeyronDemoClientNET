using MetroFramework.ClapeyronClient;
using System;
using System.Net;
using System.Globalization;

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
        public const int outPort = 49100;
        public const int inPort = 49101;
        /// <summary>
        /// Виртуальный UDP socket для прослушки порта на входящие сообщения и отправки сообщений с этого порта.
        /// </summary>
        private static UDPSocket udpSocket;

        private MainForm mainForm;

        private bool connectedToRobot = false;
        
        public UDPNode(MainForm mainForm):this(outPort,inPort)
        {
            this.mainForm = mainForm;
        }

        /// <summary>
        /// Одновременно запускает прослушку порта.
        /// </summary>
        /// <param name="outPort"></param>
        /// <param name="inPort"></param>
        private UDPNode(int outPort, int inPort)
        {
            startNode(outPort, inPort);
        }

        /// <summary>
        /// Запуск прослушки порта.
        /// </summary>
        /// <param name="outPort">порт для отправки</param>
        /// <param name="inPort">порт для прослушки</param>
        private void startNode(int outPort, int inPort)
        {
            udpSocket = new UDPSocket(outPort, inPort, this);
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
            if (isConnectedToRobot())
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
            MainForm.writeLine("UDP socket error: cant create local socket =>\n=>" + excMsg);
        }

        public void onSocketListeningClosed(UDPSocket udpSocket)
        {
            setConnectedToRobot(false);
            MainForm.writeLine("UDP socket log: UDP listening is closed");
        }

        public void onSocketListeningException(UDPSocket udpSocket)
        {
            MainForm.writeLine("UDP socket error: UDP listening is closed UNEXPECTEDLY");
        }

        public void onSocketListeningReady(UDPSocket udpSocket)
        {
            setConnectedToRobot(true);
            MainForm.writeLine("Listening started on localhost: " + udpSocket.getInLocalPort() + " and on " + "AppInfo.LocalIP" + ": " + udpSocket.getInLocalPort());
        }

        public void onSocketMessageIsNotSentCantFindRemoteURL(string outIP)
        {
            MainForm.writeLine("UDP socket error: cant find remote URL to send");
        }

        public void onSocketMessageIsNotSentIOException()
        {
            MainForm.writeLine("UDP socket error: cant send data, try again");
        }

        public void onSocketMessageIsNotSentLocalNodeIsClosed()
        {
            MainForm.writeLine("UDP socket error: cant send message, please, start node");
        }

        public void onSocketMessageReceived(IPAddress authorIP, int authorPort, string receivedString)
        {
            MainForm.writeLine("RECEIVED from " + authorIP + ":" + authorPort + "| data: " + receivedString);
            string[] splittedStream = receivedString.Split('?');
            MainForm.writeLine("splittedStream: " + splittedStream[1]);
            string[] splittedMessage = splittedStream[1].Split('|');

            if (authorPort == outPort)
            {
                switch(splittedMessage[0])
                {
                    case "HiClientImARobotClapeyron":
                        float hardVers = 0.00f;
                        if ((splittedMessage[1] == "hardvers") 
                            && float.TryParse(splittedMessage[2], NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out hardVers))
                        {
                            setConnectedToRobot(true);

                            Dispatcher.Invoke(mainForm, () => { mainForm.setTelepresenceLabelConnectionStatus("connected"); });
                            Dispatcher.Invoke(mainForm, () => { mainForm.setTelepresenceLabelConnectionStatusColor(System.Drawing.Color.LightGreen); });
                            Dispatcher.Invoke(mainForm, () => { mainForm.setTelepresenceLabelLogLeftBottom("Connected"); });
                            Dispatcher.Invoke(mainForm, () => { mainForm.setTelepresenceLabelHardwareVersion(hardVers.ToString(CultureInfo.InvariantCulture)); });
                            Dispatcher.Invoke(mainForm, () => { mainForm.setSpinnerState(false); });
                            Dispatcher.Invoke(mainForm, () => { mainForm.setTelepresenceLabelBattery("calculating.."); });
                        }
                        break;
                    case "ConnectedToTheAP":
                        if (splittedMessage[1] == mainForm.getOptionsMetroTextBoxWiFiNameValue())
                        {
                            Dispatcher.Invoke(mainForm, () => { mainForm.setOptionsLabelLogConnection("Connected. Robot IP is: "+splittedMessage[3]); });
                        }
                        break;
                    case "CanNotConnectToTheAP":
                        if (splittedMessage[1] == mainForm.getOptionsMetroTextBoxWiFiNameValue())
                        {
                            Dispatcher.Invoke(mainForm, () => { mainForm.setOptionsLabelLogConnection("Robot can't connect to the AP"); });
                        }
                        break;
                    case "BAT":

                        break;
                }
            }
        }

        public void onSocketMessageSent(IPAddress outIP, int outPort, Message data)
        {
            MainForm.writeLine("UDP socket log: Message sent");
        }

        public void onSocketReceivingException(UDPSocket udpSocket, string excMsg)
        {
            MainForm.writeLine("UDP socket error: cant receive msg =>\n=>" + excMsg);
        }

        private object locker = new object();
        private void setConnectedToRobot(bool isConnected)
        {
            lock (locker)
            {
                connectedToRobot = isConnected;
            }
        }

        public bool isConnectedToRobot()
        {
            lock (locker)
            {
                return connectedToRobot;
            }
        }
    }
}
