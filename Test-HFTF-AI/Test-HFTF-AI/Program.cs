using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Diagnostics;
using System.Drawing.Imaging;

namespace Test_HFTF_AI
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting screen capture test...");
            TestCapture();
            Console.WriteLine("Saving test image...");
            ExportCapture();
        }

        public static readonly Stopwatch sw = new Stopwatch();

        private static Bitmap CaptureImage(int x, int y)
        {
            Bitmap b = new Bitmap(100, 100);
            using (Graphics g = Graphics.FromImage(b))
            {
                g.CopyFromScreen(x, y, 0, 0, new Size(100, 100), CopyPixelOperation.SourceCopy);
            }
            return b;
        }

        private static void TestCapture()
        {
            Bitmap bmp = null;
            sw.Restart();
            for (int i = 0; i < 100; i++)
            {
                bmp = CaptureImage(0, 0);
            }
            sw.Stop();
            Console.WriteLine(sw.ElapsedMilliseconds);
        }

        private static void ExportCapture()
        {
            Bitmap bmp = CaptureImage(0, 0);
            bmp.Save("TestImage.png", ImageFormat.Png);
        }
    }
}
