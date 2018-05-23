using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.Diagnostics;
using System.Threading;

namespace HFTF_AI_v00._01._00
{
    class Program
    {
        static void Main(string[] args)
        {
            FrameBuffer fB = new FrameBuffer();
            fB.ReadFrame();
            TestNet tN = new TestNet(fB, 10, 5, 8);
            Stopwatch sW = new Stopwatch();
            Console.Write("Press enter to start: ");
            Console.ReadLine();

            sW.Start();

            fB.ReadFrame();
            tN.ActivateNetPar();

            sW.Stop();

            Console.WriteLine("Full net cycle took {0} miliseconds.", sW.ElapsedMilliseconds);
            fB.SaveFrame();
        }

        public static Random rng = new Random();

        public class FrameBuffer
        {
            public const int bufferSize = 6; //Controlls how many frames the AI remmebrs
            public Bitmap[] bufferLeft = new Bitmap[bufferSize];
            public Bitmap[] bufferRight = new Bitmap[bufferSize];
            public int bufferOffset = 0;
            public int natX = 700; //Game window size
            public int natY = 448; //single window is 700 x 448
                                   //these need to be even numbers
            int offX = 2; //Offsets of the game window
            int offY = 106;

            public FrameBuffer()
            {
                for (int i = 0; i < bufferSize; i++)
                {
                    bufferLeft[i] = new Bitmap(natX/2, natY);
                    bufferRight[i] = new Bitmap(natX/2, natY);
                }
            }

            public void ReadFrame()
            {
                IncBufOffset();

                //Thread left = new Thread(this.ReadFrameLeft);
                //Thread right = new Thread(this.ReadFrameRight);

                //left.Start();
                //right.Start();

                /*
                Parallel.Invoke(() =>
                {
                    left.Start();
                },  // close first Action

                () =>
                {
                    right.Start();
                } //close second Action
                ); //close parallel.invoke
                */

                
                try
                {
                    Task t1 = Task.Run( () => { this.ReadFrameLeft(); } );
                    Task t2 = Task.Run( () => { this.ReadFrameRight(); } );

                    Task.WaitAll(t1, t2);
                }
                finally
                {
                    Console.WriteLine("Both frame read tasks done");
                }
            }

            public void ReadFrameLeft()
            {
                using (Graphics g = Graphics.FromImage(bufferLeft[bufferOffset]))
                {
                    g.CopyFromScreen(offX, offY, 0, 0, new Size(natX / 2, natY), CopyPixelOperation.SourceCopy);
                }
            }

            public void ReadFrameRight()
            {
                using (Graphics g = Graphics.FromImage(bufferRight[bufferOffset]))
                {
                    g.CopyFromScreen(offX + (natX / 2), offY, 0, 0, new Size(natX / 2, natY), CopyPixelOperation.SourceCopy);
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
                bufferLeft[bufferOffset].Save("BufferExportLeft.png", ImageFormat.Png);
                bufferRight[bufferOffset].Save("BufferExportRight.png", ImageFormat.Png);
                Console.WriteLine("Buffer index {0} Saved to disk.", bufferOffset);
            }
        }

        public class TestNet
        {
            public TestNodeInput[] input;
            public TestNodeFirst[] first;
            public TestNodeDeep[][] deep;
            public TestNodeOut[] output;

            public const int subScaler = 4;
            int netWidth;
            int netDepth;

            public TestNet(FrameBuffer buffer, int width, int depth, int end)
            {
                input = new TestNodeInput[(buffer.natX / 2) * (buffer.natY / 2)];
                first = new TestNodeFirst[width];
                deep = new TestNodeDeep[depth][];
                for (int i = 0; i < depth; i++)
                {
                    deep[i] = new TestNodeDeep[width];
                }
                output = new TestNodeOut[end];
                netWidth = width;
                netDepth = depth;

                int nodeCount = 0;
                for (int i = 0; i < buffer.natX; i = i + 2)
                {
                    for (int l = 0; l < buffer.natY; l = l + 2)
                    {
                        input[nodeCount] = new TestNodeInput(buffer, i, l);
                        nodeCount++;
                    }
                }

                for (int i = 0; i < width; i++)
                {
                    first[i] = new TestNodeFirst(input);
                }

                for (int i = 0; i < width; i++)
                {
                    deep[0][i] = new TestNodeDeep(first);
                }

                for (int i = 1; i < deep.Length; i++)
                {
                    for (int l = 0; l < deep[i].Length; l++)
                    {
                        deep[i][l] = new TestNodeDeep(deep[i - 1]);
                    }
                }

                for (int i = 0; i < output.Length; i++)
                {
                    output[i] = new TestNodeOut(deep[deep.Length - 1]);
                }
            }

            public void ActivateNet()
            {
                foreach (TestNodeInput node in input)
                    node.GetData();
                foreach (TestNodeFirst node in first)
                    node.GetData();
                foreach (TestNodeDeep[] node in deep)
                {
                    for (int i = 0; i < node.Length; i++)
                    {
                        node[i].GetData();
                    }
                }
                foreach (TestNodeOut node in output)
                    node.GetData();
            }

            public void ActivateNetPar()
            {
                /*
                Thread input1 = new Thread(() => this.ActivateInput(true));
                Thread input2 = new Thread(() => this.ActivateInput(false));

                //left.Start();
                //right.Start();

                Parallel.Invoke(() =>
                {
                    input1.Start();
                },  // close first Action

                () =>
                {
                    input2.Start();
                } //close second Action
                ); //close parallel.invoke
                */

                
                try
                {
                    Task t1 = Task.Run(() => { this.ActivateInput(true); });
                    Task t2 = Task.Run(() => { this.ActivateInput(false); });

                    Task.WaitAll(t1, t2);
                }
                finally
                {
                    Console.WriteLine("Both input node tasks done");
                }
                

                foreach (TestNodeFirst node in first)
                    node.GetData();
                foreach (TestNodeDeep[] node in deep)
                {
                    for (int i = 0; i < node.Length; i++)
                    {
                        node[i].GetData();
                    }
                }
                foreach (TestNodeOut node in output)
                    node.GetData();
            }

            public void ActivateInput(bool firstHalf)
            {
                int start = 0;
                if (firstHalf == false)
                    start = (input.Length / 2);
                int end = start + (input.Length / 2);
                for (int l = start; l < end; l = l + subScaler)
                    input[l].GetData();
            }
        }

        public class TestNodeInput
        {
            FrameBuffer frameBuffer;
            int xLoc;
            int yLoc;

            public byte r;
            public byte g;
            public byte b;
            public Single bright;

            public TestNodeInput(FrameBuffer buf, int x, int y)
            {
                frameBuffer = buf;
                xLoc = x;
                yLoc = y;
            }

            public void GetData()
            {
                if (xLoc < frameBuffer.natX/2)
                {
                    r = frameBuffer.bufferLeft[frameBuffer.bufferOffset].GetPixel(xLoc, yLoc).R;
                    g = frameBuffer.bufferLeft[frameBuffer.bufferOffset].GetPixel(xLoc, yLoc).G;
                    b = frameBuffer.bufferLeft[frameBuffer.bufferOffset].GetPixel(xLoc, yLoc).B;
                    bright = frameBuffer.bufferLeft[frameBuffer.bufferOffset].GetPixel(xLoc, yLoc).GetBrightness();
                }
                else
                {
                    r = frameBuffer.bufferRight[frameBuffer.bufferOffset].GetPixel(xLoc - (frameBuffer.natX/2), yLoc).R;
                    g = frameBuffer.bufferRight[frameBuffer.bufferOffset].GetPixel(xLoc - (frameBuffer.natX / 2), yLoc).G;
                    b = frameBuffer.bufferRight[frameBuffer.bufferOffset].GetPixel(xLoc - (frameBuffer.natX / 2), yLoc).B;
                    bright = frameBuffer.bufferRight[frameBuffer.bufferOffset].GetPixel(xLoc - (frameBuffer.natX / 2), yLoc).GetBrightness();
                }
            }
        }

        public class TestNodeFirst
        {

            TestNodeInput[] inputArray;
            double[] rMult;
            double[] gMult;
            double[] bMult;
            double[] brightMult;

            public double value;

            public TestNodeFirst(TestNodeInput[] input)
            {
                inputArray = input;

                rMult = new double[input.Length];
                for (int i = 0; i < input.Length; i++)
                    rMult[i] = rng.NextDouble();

                gMult = new double[input.Length];
                for (int i = 0; i < input.Length; i++)
                    gMult[i] = rng.NextDouble();

                bMult = new double[input.Length];
                for (int i = 0; i < input.Length; i++)
                    bMult[i] = rng.NextDouble();

                brightMult = new double[input.Length];
                for (int i = 0; i < input.Length; i++)
                    brightMult[i] = rng.NextDouble();
            }

            public void GetData()
            {
                double total = 0;
                int count = 0;

                for (int i = 0; i < inputArray.Length; i++)
                {
                    total = total + ((inputArray[i].r/255)*rMult[i]) + ((inputArray[i].g/255)*gMult[i]) + ((inputArray[i].b/255) * bMult[i]) + (inputArray[i].bright*brightMult[i]);
                    count = count + 4;
                }

                value = total / count;
            }
        }

        public class TestNodeDeep
        {
            TestNodeFirst[] firstArray;
            TestNodeDeep[] prevArray;
            bool isFirstDeep = true;
            double[] mult;

            public double value;

            public TestNodeDeep(TestNodeFirst[] first)
            {
                isFirstDeep = true;
                firstArray = first;

                mult = new double[first.Length];
                for (int i = 0; i < first.Length; i++)
                    mult[i] = rng.NextDouble();
            }

            public TestNodeDeep(TestNodeDeep[] prev)
            {
                isFirstDeep = false;
                prevArray = prev;

                mult = new double[prev.Length];
                for (int i = 0; i < prev.Length; i++)
                    mult[i] = rng.NextDouble();
            }

            public void GetData()
            {
                double total = 0;
                int count = 0;
                if (isFirstDeep == true)
                {
                    for (int i = 0; i < firstArray.Length; i++)
                    {
                        total = total + (firstArray[i].value * mult[i]);
                        count++;
                    }
                }
                else
                {
                    for (int i = 0; i < prevArray.Length; i++)
                    {
                        total = total + (prevArray[i].value * mult[i]);
                        count++;
                    }
                }

                value = total / count;
            }
        }

        public class TestNodeOut
        {
            TestNodeDeep[] prevArray;
            double[] mult;

            double value;

            public TestNodeOut(TestNodeDeep[] prev)
            {
                prevArray = prev;

                mult = new double[prev.Length];
                for (int i = 0; i < prev.Length; i++)
                    mult[i] = rng.NextDouble();
            }

            public void GetData()
            {
                double total = 0;
                int count = 0;
                for (int i = 0; i < prevArray.Length; i++)
                {
                    total = total + (prevArray[i].value * mult[i]);
                    count++;
                }

                value = total / count;
            }
        }
    }
}
