using System;
using System.Drawing;
using System.IO;
using System.Net;

namespace MetroFramework.ClapeyronClient
{
    class CamStreamer
    {
        private static Bitmap bmp;

        public static Bitmap image
        {
            get
            {
                return bmp;
            }
        }

        public static void shot(string sourceURL, MainForm _form)
        {
            try
            {
                byte[] buffer = new byte[640 * 480];
                int read, total = 0;

                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(sourceURL);
                WebResponse resp = req.GetResponse();

                Stream stream = resp.GetResponseStream();

                while ((read = stream.Read(buffer, total, 1000)) != 0)
                {
                    total += read;
                }

                bmp = (Bitmap)Bitmap.FromStream(new MemoryStream(buffer, 0, total));
                for (int i = 0; i < 10; i++)
                {
                    bmp.SetPixel(315 - i, 240, Color.White);
                    bmp.SetPixel(325 + i, 240, Color.White);
                    bmp.SetPixel(320, 235 - i, Color.White);
                    bmp.SetPixel(320, 245 + i, Color.White);
                }
            }

            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
