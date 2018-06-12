using System;
using System.Drawing;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace MetroFramework.ClapeyronClient
{
    class Commander
    {
        private UdpClient _sender;
        private UdpClient _receiver;
        private IPEndPoint _end_point;

        private Thread recv_thread;
        private Thread bat_thread;
        private MainForm _form;

        public bool is_connected = false;

        public Commander(UdpClient sender, UdpClient receiver, IPEndPoint end_point, MainForm form)
        {
            _sender = sender;
            _receiver = receiver;
            _end_point = end_point;
            _form = form;
        }

        [STAThread]
        public void init()
        {
            recv_thread = new Thread(new ThreadStart(onReceive));
            bat_thread = new Thread(new ThreadStart(onChargeRequest));
            recv_thread.IsBackground = true;
            bat_thread.IsBackground = true;
            recv_thread.Start();
            bat_thread.Start();

            _form.setLabel16("Ready");
        }

        public void send(string datagram)
        {
            try
            {
                byte[] bytes = Encoding.UTF8.GetBytes(datagram);
                _sender.Send(bytes, bytes.Length, _end_point);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message); //debug
            }
        }

        private void onReceive()
        {
            IPEndPoint RemoteIpEndPoint = null;

            try
            {
                while (true)
                {
                    string[] parsed_data;
                    string raw_data;
                    Console.WriteLine("recv.."); //debug

                    byte[] initialBytes = _receiver.Receive(ref RemoteIpEndPoint);
                    raw_data = Encoding.UTF8.GetString(initialBytes);

                    Console.WriteLine("recv: " + raw_data); //debug

                    if (raw_data != null)
                    {
                        parsed_data = raw_data.Split('_');
                        if (parsed_data[0].Equals("INIT"))
                        {
                            is_connected = true;

                            Dispatcher.Invoke(_form, () => { _form.setLabel3("connected"); });
                            Dispatcher.Invoke(_form, () => { _form.setLabel3Color(System.Drawing.Color.LightGreen); });
                            Dispatcher.Invoke(_form, () => { _form.setLabel16("Connected"); });
                            Dispatcher.Invoke(_form, () => { _form.setLabel6(parsed_data[1]); });
                            Dispatcher.Invoke(_form, () => { _form.setLabel7(parsed_data[2]); });
                            Dispatcher.Invoke(_form, () => { _form.setSpinnerState(false); });
                            Dispatcher.Invoke(_form, () => { _form.setLabel11("calculating.."); });
                        }
                    }

                    Console.WriteLine("cmd_run"); //debug

                    while (is_connected)
                    {
                        byte[] receiveBytes = _receiver.Receive(ref RemoteIpEndPoint);
                        raw_data = Encoding.UTF8.GetString(receiveBytes);

                        if (raw_data != null)
                        {
                            parsed_data = raw_data.Split('_');
                            switch (parsed_data[0])
                            {
                                case "BAT":
                                    //TODO: make BAT stream as a connection checker
                                    int charge = int.Parse(parsed_data[1]);

                                    if (charge >= 1300)
                                    {
                                        Dispatcher.Invoke(_form, () => { _form.setLabel11Color(Color.LightGreen); }); //green
                                        if (charge >= 1400)
                                        {
                                            Dispatcher.Invoke(_form, () => { _form.setLabel11("100%"); });
                                        }
                                        else
                                        {
                                            charge = (int)(0.6 * charge) - 740;
                                            Dispatcher.Invoke(_form, () => { _form.setLabel11(charge.ToString() + "%"); });
                                        }
                                    }
                                    else
                                    {
                                        if (charge > 1250)
                                        {
                                            charge = (int)(0.6 * charge) - 740;
                                            Dispatcher.Invoke(_form, () => { _form.setLabel11(charge.ToString() + "%"); });
                                            Dispatcher.Invoke(_form, () => { _form.setLabel11Color(Color.LightYellow); }); //yellow
                                        }
                                        else
                                        {
                                            if (charge > 1200)
                                            {
                                                charge = (int)(0.18 * charge) - 215;
                                                Dispatcher.Invoke(_form, () => { _form.setLabel11(charge.ToString() + "%"); });
                                                Dispatcher.Invoke(_form, () => { _form.setLabel11Color(Color.Salmon); }); //red
                                            }
                                            else
                                            {
                                                Dispatcher.Invoke(_form, () => { _form.setLabel11("1%"); });
                                                Dispatcher.Invoke(_form, () => { _form.setLabel11Color(Color.DarkRed); }); //black
                                            }
                                        }
                                    }

                                    break;

                                case "HALL":
                                    Dispatcher.Invoke(_form, () => { _form.setHallValue(parsed_data[1]);});
                                    break;

                                default:
                                    break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message); //debug
            }
        }

        private void onChargeRequest()
        {
            while (true)
            {
                send("BAT");
                //Console.WriteLine("bat");
                Thread.Sleep(10000);
            }
        }
    }
}