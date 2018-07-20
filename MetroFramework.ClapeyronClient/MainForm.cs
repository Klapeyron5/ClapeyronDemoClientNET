using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using MetroFramework.Forms;
using ObjectRecognition;
using MetroFramework.Demo.UDPNode;
using System.Drawing.Drawing2D;
using MetroFramework.ClapeyronClient.FastBitmap;

namespace MetroFramework.ClapeyronClient
{
    public partial class MainForm : MetroForm
    {
    //    Commander cmd;
        Color second_color = Color.Orange;

        UDPNode udpNode;
        ThreadRunner streamer;

        XmlDocument config_xml = new XmlDocument();
        const string default_robot_ip = "192.168.0.100";
        const string default_robot_port = "49101";
        const string default_client_port = "49100";
        const string default_cam_string = "http://192.168.8.1:8080/?action=snapshot";

        public const int telepresencePictureBoxCameraWidth = 521;
        public const int telepresencePictureBoxCameraHeight = 415;
        public const int pictureBoxCenterX = (521 - 1) / 2;
        public const int pictureBoxCenterY = (415 - 1) / 2;

        public MainForm()
        {
            InitializeComponent();
            setTelepresenceLabelSoftwareVersion(AppInfo.version.ToString(System.Globalization.CultureInfo.InvariantCulture));
            
            //test start
            PixelMap testPixelMap = new PixelMap(521,415,0,0,0);
            Bitmap testBmp = testPixelMap.GetBitmap();
            Graphics graphics = Graphics.FromImage(testBmp);
            try
            {
                Pen pen;
                pen = new Pen(Color.Green);
                graphics.DrawEllipse(pen, pictureBoxCenterX-50, pictureBoxCenterY-50, 101, 101);
            }
            catch (Exception ex) { }
            drawBitmapOnTelepresencePictureBox(testBmp);
            //test end
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            loadOptions();
            PopulateLearnedItemsList();

            IPAddress remoteIPAddress = IPAddress.Parse(optionsMetroTextBoxRobotIP.Text);
            IPEndPoint endPoint = new IPEndPoint(remoteIPAddress, int.Parse(optionsMetroTextBoxRobotPort.Text));
         //   cmd = new Commander(new UdpClient(int.Parse(optionsMetroTextBoxRobotPort.Text)), new UdpClient(int.Parse(optionsMetroTextBoxClientPort.Text)), endPoint, this);
         //   cmd.init();

        //    streamer = new ThreadRunner(new ThreadStart(stream));
        //    streamer.Background(true);
         //   streamer.Suspend();
        //    ThreadRunner cleaner = new ThreadRunner(new ThreadStart(clearPictureBox));
        }

        void loadOptions()
        {
            //load xml-document
            try { config_xml.Load("config.xml"); }
            catch (System.IO.FileNotFoundException ex)
            {
                //TODO: create new file
                MessageBox.Show(ex.ToString());
            }

            Console.WriteLine("Loaded");

            for (int i = 0; i < config_xml.ChildNodes.Count; i++)
            {
                if (config_xml.ChildNodes[i].Name.Equals("options"))
                {
                    //Console.WriteLine(">> options");
                    for (int j = 0; j < config_xml.ChildNodes[i].ChildNodes.Count; j++)
                    {
                        if (config_xml.ChildNodes[i].ChildNodes[j].Name.Equals("networking"))
                        {
                            //Console.WriteLine(">> networking");
                            for (int k = 0; k < config_xml.ChildNodes[i].ChildNodes[j].ChildNodes.Count; k++)
                            {
                                if (config_xml.ChildNodes[i].ChildNodes[j].ChildNodes[k].Name.Equals("robot_ip"))
                                {
                                    optionsMetroTextBoxRobotIP.Text = config_xml.ChildNodes[i].ChildNodes[j].ChildNodes[k].InnerText;
                                    //Console.WriteLine(">> robot_ip: " + optionsMetroTextBoxRobotIP.Text);
                                }
                                if (config_xml.ChildNodes[i].ChildNodes[j].ChildNodes[k].Name.Equals("robot_port"))
                                {
                                    optionsMetroTextBoxRobotPort.Text = config_xml.ChildNodes[i].ChildNodes[j].ChildNodes[k].InnerText;
                                }
                                if (config_xml.ChildNodes[i].ChildNodes[j].ChildNodes[k].Name.Equals("client_port"))
                                {
                                    optionsMetroTextBoxClientPort.Text = config_xml.ChildNodes[i].ChildNodes[j].ChildNodes[k].InnerText;
                                }
                                if (config_xml.ChildNodes[i].ChildNodes[j].ChildNodes[k].Name.Equals("cam_string"))
                                {
                                    optionsMetroTextBoxCamURL.Text = config_xml.ChildNodes[i].ChildNodes[j].ChildNodes[k].InnerText;
                                }
                            }
                        }
                        if (config_xml.ChildNodes[i].ChildNodes[j].Name.Equals("interface"))
                        {
                            for (int k = 0; k < config_xml.ChildNodes[i].ChildNodes[j].ChildNodes.Count; k++)
                            {
                                if (config_xml.ChildNodes[i].ChildNodes[j].ChildNodes[k].Name.Equals("color"))
                                {
                                    switch (int.Parse(config_xml.ChildNodes[i].ChildNodes[j].ChildNodes[k].InnerText))
                                    {
                                        case 0:
                                            metroStyleManager.Style = MetroColorStyle.Orange;
                                            second_color = Color.Orange;
                                            break;
                                        case 1:
                                            metroStyleManager.Style = MetroColorStyle.Purple;
                                            second_color = Color.MediumOrchid;
                                            break;
                                        case 2:
                                            metroStyleManager.Style = MetroColorStyle.Blue;
                                            second_color = Color.Aqua;
                                            break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void stream()
        {
            int ticker = 0;
            while (!streamer.is_terminated)
            {
                CamStreamer.shot(optionsMetroTextBoxCamURL.Text,this);
                telepresencePictureBoxCamera.Image = CamStreamer.image;
                streamer.ev_suspend.WaitOne();

                ticker++;
                if ((cv_is_on)&(ticker%10 == 0))
                {
                    Dispatcher.Invoke(this, () => { AnalizeImage((Bitmap)telepresencePictureBoxCamera.Image); });
                    ticker = 0;
                }
            }
        }

        private void save_color_to_xml(int _color)
        {
            for (int i = 0; i < config_xml.ChildNodes.Count; i++)
            {
                if (config_xml.ChildNodes[i].Name.Equals("options"))
                {
                    for (int j = 0; j < config_xml.ChildNodes[i].ChildNodes.Count; j++)
                    {
                        if (config_xml.ChildNodes[i].ChildNodes[j].Name.Equals("interface"))
                        {
                            for (int k = 0; k < config_xml.ChildNodes[i].ChildNodes[j].ChildNodes.Count; k++)
                            {
                                if (config_xml.ChildNodes[i].ChildNodes[j].ChildNodes[k].Name.Equals("color"))
                                {
                                    config_xml.ChildNodes[i].ChildNodes[j].ChildNodes[k].InnerText = _color.ToString();
                                }
                            }
                        }
                    }
                }
            }

            config_xml.Save("config.xml");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            metroStyleManager.Style = MetroColorStyle.Orange;
            second_color = Color.Orange;

            save_color_to_xml(0);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            metroStyleManager.Style = MetroColorStyle.Purple;
            second_color = Color.MediumOrchid;

            save_color_to_xml(1);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            metroStyleManager.Style = MetroColorStyle.Blue;
            second_color = Color.Aqua;

            save_color_to_xml(2);
        }

        private void telepresenceMetroToggleConnection_CheckedChanged(object sender, EventArgs e)
        {
            if (telepresenceMetroToggleConnection.Checked)
            {
                telepresenceMetroProgressSpinner.Visible = true;
                telepresenceLabelLogLeftBottom.Text = "Connecting..";
                
                udpNode = new UDPNode(this);
                udpNode.sendNewString(optionsMetroTextBoxRobotIP.Text, UDPNode.inPort, "?HiRobotClapeyronImAClient?");
            //    cmd.send("INIT");
            }
            else
            {
                telepresenceMetroProgressSpinner.Visible = false;
                telepresenceLabelLogLeftBottom.Text = "Canceled";
                telepresenceLabelConnectionStatus.Text = "not connected";
                telepresenceLabelConnectionStatus.ForeColor = Color.LightSalmon;

                udpNode.sendNewString(optionsMetroTextBoxRobotIP.Text, UDPNode.inPort, "?ByeRobotClapeyron?");
                udpNode.closeNode();
          //      cmd.send("WASD_OFF");
          //      cmd.send("STREAM_OFF");
          //      cmd.send("CAM_OFF");

          //      cmd.is_connected = false;
            }
        }

        private void telepresenceMetroToggleWASD_CheckedChanged(object sender, EventArgs e)
        {
            if (telepresenceMetroToggleWASD.Checked)
            {
                udpNode.sendNewString(optionsMetroTextBoxRobotIP.Text, UDPNode.inPort, "?Move|Start?");
                //      cmd.send("WASD_ON");
                telepresenceLabelLogLeftBottom.Text = "WASD on";
                panel2.Visible = true;
                panel3.Visible = true;
                panel4.Visible = true;
                panel5.Visible = true;
            }
            else
            {
                udpNode.sendNewString(optionsMetroTextBoxRobotIP.Text, UDPNode.inPort, "?Move|Stop?");
                //      cmd.send("WASD_OFF");
                telepresenceLabelLogLeftBottom.Text = "WASD off";
                panel2.Visible = false;
                panel3.Visible = false;
                panel4.Visible = false;
                panel5.Visible = false;
            }
        }

        private void telepresenceMetroToggleCamera_CheckedChanged(object sender, EventArgs e)
        {
            if (telepresenceMetroToggleCamera.Checked)
            {
                streamer.Resume();
                telepresenceLabelLogLeftBottom.Text = "Stream on";
            }
            else
            {
                streamer.Suspend();
                ThreadRunner cleaner = new ThreadRunner(new ThreadStart(clearPictureBox));
                telepresenceLabelLogLeftBottom.Text = "Stream off";
            }
        }

        private void telepresenceMetroToggleGrasper_CheckedChanged(object sender, EventArgs e)
        {
            if (telepresenceMetroToggleGrasper.Checked)
            {
           //     cmd.send("GRASP_ON");
                telepresenceLabelLogLeftBottom.Text = "Grasp on";
                label2.Visible = true;
            }
            else
            {
          //      cmd.send("GRASP_OFF");
                telepresenceLabelLogLeftBottom.Text = "Grasp off";
                label2.Visible = false;
            }
        }

        bool cv_is_on = false;

        private void telepresenceMetroToggleCV_CheckedChanged(object sender, EventArgs e)
        {
            if (telepresenceMetroToggleCV.Checked)
            {
                cv_is_on = true;
                telepresenceLabelLogLeftBottom.Text = "CV on";
                label32.Visible = true;
                label32.Text = "May contain:";
            }
            else
            {
                cv_is_on = false;
                telepresenceLabelLogLeftBottom.Text = "CV off";
                label32.Visible = false;
            }
        }

        //gui accessing methods
        public void setTelepresenceLabelLogLeftBottom(string text)
        {
            telepresenceLabelLogLeftBottom.Text = text;
        }
        public void setTelepresenceLabelHardwareVersion(string text)
        {
            telepresenceLabelHardwareVersion.Text = text;
        }
        public void setTelepresenceLabelConnectionStatus(string text)
        {
            telepresenceLabelConnectionStatus.Text = text;
        }
        public void setTelepresenceLabelSoftwareVersion(string text)
        {
            telepresenceLabelSoftwareVersion.Text = text;
        }
        public void setTelepresenceLabelBattery(string text)
        {
            telepresenceLabelBattery.Text = text;
        }
        public void setTelepresenceLabelConnectionStatusColor(Color color)
        {
            telepresenceLabelConnectionStatus.ForeColor = color;
        }
        public void setTelepresenceLabelBatteryColor(Color color)
        {
            telepresenceLabelBattery.ForeColor = color;
        }
        public void setSpinnerState(bool state)
        {
            telepresenceMetroProgressSpinner.Visible = state;
        }
        public void setPictureBox(Bitmap bmp)
        {
            telepresencePictureBoxCamera.Image = bmp;
        }
        public void setHallValue(string value)
        {
            label2.Text = "Hall sensor: " + value;
        }
        public void clearPictureBox()
        {
            Thread.Sleep(500);
            telepresencePictureBoxCamera.Image = new Bitmap(640, 480);
        }
        public void setOptionsLabelLogConnection(string value)
        {
            optionsLabelLogConnection.Text = value;
        }
        public string getOptionsMetroTextBoxWiFiNameValue()
        {
            return optionsMetroTextBoxWiFiName.Text;
        }

        // robot control via keyboard
        bool keyIsPressed = false;
        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            //wasd
            if ((e.KeyData == Keys.W) && (!keyIsPressed))
            {
         //       cmd.send("FWD");
                panel5.BackColor = second_color;
                keyIsPressed = true;
            }
            if ((e.KeyData == Keys.S) && (!keyIsPressed))
            {
          //      cmd.send("BWD");
                panel3.BackColor = second_color;
                keyIsPressed = true;
            }
            if ((e.KeyData == Keys.A) && (!keyIsPressed))
            {
          //      cmd.send("LFT");
                panel2.BackColor = second_color;
                keyIsPressed = true;
            }
            if ((e.KeyData == Keys.D) && (!keyIsPressed))
            {
          //      cmd.send("RGH");
                panel4.BackColor = second_color;
                keyIsPressed = true;
            }
            //rotate
            if ((e.KeyData == Keys.Q) && (!keyIsPressed))
            {
         //       cmd.send("R_RGH");
                keyIsPressed = true;
            }
            if ((e.KeyData == Keys.E) && (!keyIsPressed))
            {
          //      cmd.send("R_LFT");
                keyIsPressed = true;
            }
            //arm
            if ((e.KeyData == Keys.C) && (!keyIsPressed))
            {
          //      cmd.send("M_INC");
                keyIsPressed = true;
            }
            if ((e.KeyData == Keys.V) && (!keyIsPressed))
            {
          //      cmd.send("M_DEC");
                keyIsPressed = true;
            }
            //grasp
            if ((e.KeyData == Keys.F) && (!keyIsPressed))
            {
         //       cmd.send("G_INC");
                keyIsPressed = true;
            }
            if ((e.KeyData == Keys.G) && (!keyIsPressed))
            {
          //      cmd.send("G_DEC");
                keyIsPressed = true;
            }
        }

        private void MainForm_KeyUp(object sender, KeyEventArgs e)
        {
            //wasd
            if (e.KeyData == Keys.W)
            {
          //      cmd.send("STP");
                System.Threading.Thread.Sleep(5);
                panel5.BackColor = Color.Silver;
                keyIsPressed = false;
            }
            if (e.KeyData == Keys.S)
            {
         //       cmd.send("STP");
                System.Threading.Thread.Sleep(5);
                panel3.BackColor = Color.Silver;
                keyIsPressed = false;
            }
            if (e.KeyData == Keys.A)
            {
           //     cmd.send("STP");
                System.Threading.Thread.Sleep(5);
                panel2.BackColor = Color.Silver;
                keyIsPressed = false;
            }
            if (e.KeyData == Keys.D)
            {
          ///      cmd.send("STP");
                System.Threading.Thread.Sleep(5);
                panel4.BackColor = Color.Silver;
                keyIsPressed = false;
            }
            //grasp
            if (e.KeyData == Keys.Q)
            {
           //     cmd.send("R_STP");
                System.Threading.Thread.Sleep(5);
                keyIsPressed = false;
            }
            if (e.KeyData == Keys.E)
            {
          //      cmd.send("R_STP");
                System.Threading.Thread.Sleep(5);
                keyIsPressed = false;
            }
            //arm
            if (e.KeyData == Keys.C)
            {
          //      cmd.send("M_STP");
                System.Threading.Thread.Sleep(5);
                keyIsPressed = false;
            }
            if (e.KeyData == Keys.V)
            {
           //     cmd.send("M_STP");
                System.Threading.Thread.Sleep(5);
                keyIsPressed = false;
            }
            //grasp
            if (e.KeyData == Keys.F)
            {
           //     cmd.send("G_STP");
                System.Threading.Thread.Sleep(5);
                keyIsPressed = false;
            }
            if (e.KeyData == Keys.G)
            {
           //     cmd.send("G_STP");
                System.Threading.Thread.Sleep(5);
                keyIsPressed = false;
            }
        }

        private void optionsMetroButtonDefault_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < config_xml.ChildNodes.Count; i++)
            {
                if (config_xml.ChildNodes[i].Name.Equals("options"))
                {
                    //Console.WriteLine(">> options");
                    for (int j = 0; j < config_xml.ChildNodes[i].ChildNodes.Count; j++)
                    {
                        if (config_xml.ChildNodes[i].ChildNodes[j].Name.Equals("networking"))
                        {
                            //Console.WriteLine(">> networking");
                            for (int k = 0; k < config_xml.ChildNodes[i].ChildNodes[j].ChildNodes.Count; k++)
                            {
                                if (config_xml.ChildNodes[i].ChildNodes[j].ChildNodes[k].Name.Equals("robot_ip"))
                                {
                                    optionsMetroTextBoxRobotIP.Text = default_robot_ip;
                                }
                                if (config_xml.ChildNodes[i].ChildNodes[j].ChildNodes[k].Name.Equals("robot_port"))
                                {
                                    optionsMetroTextBoxRobotPort.Text = default_robot_port;
                                }
                                if (config_xml.ChildNodes[i].ChildNodes[j].ChildNodes[k].Name.Equals("client_port"))
                                {
                                    optionsMetroTextBoxClientPort.Text = default_client_port;
                                }
                                if (config_xml.ChildNodes[i].ChildNodes[j].ChildNodes[k].Name.Equals("cam_string"))
                                {
                                    optionsMetroTextBoxCamURL.Text = default_cam_string;
                                }
                            }
                        }
                    }
                }
            }
        }

        private void optionsMetroButtonSave_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < config_xml.ChildNodes.Count; i++)
            {
                if (config_xml.ChildNodes[i].Name.Equals("options"))
                {
                    //Console.WriteLine(">> options");
                    for (int j = 0; j < config_xml.ChildNodes[i].ChildNodes.Count; j++)
                    {
                        if (config_xml.ChildNodes[i].ChildNodes[j].Name.Equals("networking"))
                        {
                            //Console.WriteLine(">> networking");
                            for (int k = 0; k < config_xml.ChildNodes[i].ChildNodes[j].ChildNodes.Count; k++)
                            {
                                if (config_xml.ChildNodes[i].ChildNodes[j].ChildNodes[k].Name.Equals("robot_ip"))
                                {
                                    config_xml.ChildNodes[i].ChildNodes[j].ChildNodes[k].InnerText = optionsMetroTextBoxRobotIP.Text;
                                    //Console.WriteLine(">> robot_ip: " + metroTextBox1.Text);
                                }
                                if (config_xml.ChildNodes[i].ChildNodes[j].ChildNodes[k].Name.Equals("robot_port"))
                                {
                                    config_xml.ChildNodes[i].ChildNodes[j].ChildNodes[k].InnerText = optionsMetroTextBoxRobotPort.Text;
                                }
                                if (config_xml.ChildNodes[i].ChildNodes[j].ChildNodes[k].Name.Equals("client_port"))
                                {
                                    config_xml.ChildNodes[i].ChildNodes[j].ChildNodes[k].InnerText = optionsMetroTextBoxClientPort.Text;
                                }
                                if (config_xml.ChildNodes[i].ChildNodes[j].ChildNodes[k].Name.Equals("cam_string"))
                                {
                                    config_xml.ChildNodes[i].ChildNodes[j].ChildNodes[k].InnerText = optionsMetroTextBoxCamURL.Text;
                                }
                            }
                        }
                    }
                }
            }
            config_xml.Save("config.xml");
        }

        /*
         *
         * 
         * CV GUI click methods
         * 
         * 
         */
        private Bitmap learningImage = null;
        private Bitmap backgroundImage = null;

        private void metroButton3_Click(object sender, EventArgs e)  //load background
        {
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Filter = "Image Files (*.png, *.jpg)|*.png;*.jpg";
                dlg.DefaultExt = ".png"; // Default file extension 

                if (dlg.ShowDialog() == DialogResult.OK)
                {   
                    string fileName = dlg.FileName;
                    backgroundImage = new Bitmap(dlg.FileName);
                    pictureBox3.Image = backgroundImage;
                }
            }
        }

        private void metroButton4_Click(object sender, EventArgs e) //load object
        {
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Filter = "Image Files (*.png, *.jpg)|*.png;*.jpg";
                dlg.DefaultExt = ".png"; // Default file extension 

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    string fileName = dlg.FileName;
                    learningImage = new Bitmap(dlg.FileName);
                    pictureBox4.Image = learningImage;
                }
            }
        }

        private void metroButton5_Click(object sender, EventArgs e) //learn
        {
            string objectName = textBox1.Text;
            if (objectName.Length == 0)
            {
                MessageBox.Show("Please give the item a name");
            }
            else if (learningImage == null || backgroundImage == null)
            {
                MessageBox.Show("Please select a background image and an image with the object to learn");
            }
            else
            {
                ObjectLearningServices.LearnObject(learningImage, backgroundImage, objectName);
                PopulateLearnedItemsList();
            }
        }

        private void metroButton6_Click(object sender, EventArgs e) //delete
        {
            if (this.listBox1.SelectedIndex != -1)
            {
                string objectName = (string)this.listBox1.SelectedItem;
                ObjectMemoryService.RemoveSignatureByName(objectName);
                PopulateLearnedItemsList();
            }
            else
            {
                MessageBox.Show("Please select an item to delete");
            }
        }

        private void PopulateLearnedItemsList()
        {
            List<ObjectSignatureData> learnedObjects = ObjectMemoryService.GetSignatures();
            this.listBox1.Items.Clear();
            foreach (ObjectSignatureData objectSignatureData in learnedObjects)
            {
                this.listBox1.Items.Add(objectSignatureData.ObjectName);
            }
        }

        private void AnalizeImage(Bitmap recimg)
        {
            if (recimg == null)
            {
                MessageBox.Show("Please select an image to analyze");
            }
            else
            {
                IList<string> identifiedObjects = ObjectIdentificationService.AnalyzeImage(recimg);
                //this.label32.Text = "";
                Dispatcher.Invoke(this, () => { this.label32.Text = ""; });

                foreach (string objectName in identifiedObjects)
                {
                    //this.label32.Text = this.label32.Text + ("May contain " + objectName + "\r\n");
                    Dispatcher.Invoke(this, () => { this.label32.Text = this.label32.Text + ("May contain " + objectName + "\r\n"); });
                }

                if (identifiedObjects.Count == 0)
                {
                    //this.label32.Text = "Did not recognize anything";
                    Dispatcher.Invoke(this, () => { this.label32.Text = "Did not recognize anything"; });
                }
            }
        }

        public static void writeLine(String data)
        {
            Console.WriteLine(data);
        }

        public void drawBitmapOnTelepresencePictureBox(Bitmap bmp)
        {
            drawBitmapOnPictureBox(telepresencePictureBoxCamera, bmp);
        }

        /// <summary>
        /// Рисует битман на данном pictureBox, при этом не размывает, если битмап меньше реальных размеров pictureBox'а
        /// </summary>
        /// <param name="pictureBox">на нем будет нарисован битмап</param>
        /// <param name="bmp">битмап, который надо отрисовать</param>
        private static void drawBitmapOnPictureBox(PictureBox pictureBox, Bitmap bmp)
        {
            if (pictureBox.Size != bmp.Size)
            {
                float zoom = 60.0f;
                Bitmap zoomed = new Bitmap((int)(bmp.Width * zoom), (int)(bmp.Height * zoom));

                using (Graphics g = Graphics.FromImage(zoomed))
                {
                    g.InterpolationMode = InterpolationMode.NearestNeighbor;
                    g.PixelOffsetMode = PixelOffsetMode.Half;
                    g.DrawImage(bmp, new Rectangle(Point.Empty, zoomed.Size));
                }
                pictureBox.Image = zoomed;
            }
            else
                pictureBox.Image = bmp;
        }

        private void optionsMetroButtonConnect_Click(object sender, EventArgs e)
        {
            if ((udpNode != null)&&(udpNode.isConnectedToRobot()))
            {
                if ((optionsMetroTextBoxWiFiName.Text != "")&&(optionsMetroTextBoxPassword.Text != ""))
                {
                    setOptionsLabelLogConnection("Robot connecting...");
                    udpNode.sendNewString(optionsMetroTextBoxRobotIP.Text, UDPNode.inPort, "?ConnectToTheAP|"+optionsMetroTextBoxWiFiName.Text+"|"+ optionsMetroTextBoxPassword.Text+"?");
                }
            }
        }
    }
}