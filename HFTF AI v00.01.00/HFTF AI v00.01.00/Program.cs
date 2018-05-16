using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;

namespace HFTF_AI_v00._01._00
{
    class Program
    {
        static void Main(string[] args)
        {
            FrameBuffer fB = new FrameBuffer();
            fB.ReadFrame();
            fB.SaveFrame();
        }

        public class FrameBuffer
        {
            static int bufferSize = 6; //Controlls how many frames the AI remmebrs
            Bitmap[] buffer = new Bitmap[bufferSize];
            int bufferOffset = 0;
            int natX = 433; //Game window size
            int natY = 224;
            int offX = 0; //Offsets of the game window
            int offY = 0;

            public FrameBuffer()
            {
                for (int i = 0; i < bufferSize; i++)
                {
                    buffer[i] = new Bitmap(natX, natY);
                }
            }

            public void ReadFrame()
            {
                IncBufOffset();
                using (Graphics g = Graphics.FromImage(buffer[bufferOffset]))
                {
                    g.CopyFromScreen(offX, offY, 0, 0, new Size(natX, natY), CopyPixelOperation.SourceCopy);
                }
            }

            public void IncBufOffset()
            {
                if (bufferOffset == (bufferSize - 1))
                    bufferOffset = 0;
                else
                    bufferOffset++;
            }

            public void SaveFrame()
            {
                buffer[bufferOffset].Save("BufferExport.png", ImageFormat.Png);
                Console.WriteLine("Buffer index {0} Saved to disk.", bufferOffset);
            }
        }
    }
}
