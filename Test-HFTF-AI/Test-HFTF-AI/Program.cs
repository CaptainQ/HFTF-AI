using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Test_HFTF_AI
{
    class Program
    {
        static void Main(string[] args)
        {

        }

        private static Bitmap CaptureImage(int x, int y)
        {
            Bitmap b = new Bitmap(100, 100);
            using (Graphics g = Graphics.FromImage(b))
            {
                g.CopyFromScreen(x, y, 0, 0, new Size(100, 100), CopyPixelOperation.SourceCopy);
            }
            return b;
        }
    }
}
