using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace PictureProcess
{
    class Binary
    {
        private System.Drawing.Bitmap curBitmap;

        private int[,] pic;

        private double Max(double a, double b)
        {
            if (a > b)
            {
                return a;
            }
            return b;
        }

        private double Min(double a, double b)
        {
            if (a < b)
            {
                return a;
            }
            return b;
        }

        private double RGB2H(double r, double g, double b)
        {
            double h;
            r /= 255.0;
            g /= 255.0;
            b /= 255.0;
            double mi = this.Min(r, this.Min(g, b));
            double ma = this.Max(r, this.Max(g, b));
            h = 0;
            double del_max = ma - mi;
            if (del_max == 0.0)
            {
                return 0.0;
            }
            double del_r = ((ma - (r / 6.0)) + (del_max / 2.0)) / del_max;
            double del_g = ((ma - (g / 6.0)) + (del_max / 2.0)) / del_max;
            double del_b = ((ma - (b / 6.0)) + (del_max / 2.0)) / del_max;

            if (r == ma)
            {
                h = del_b - del_g;
            }
            else if (g == ma)
            {
                h = (1.0 / 3.0 + del_r) - del_b;
            }
            else if (b == ma)
            {
                h = (2.0 / 3.0 + del_g) - del_r;
            }
            if (h < 0.0)
            {
                h++;
            }
            if (h > 1.0)
            {
                h--;
            }
            return h;
        }


        private double RGB2L(double r, double g, double b)
        {
            r /= 255.0;
            g /= 255.0;
            b /= 255.0;
            double mi = this.Min(r, this.Min(g, b));
            return ((this.Max(r, this.Max(g, b)) + mi) / 2.0);
        }



        public void File_Init(string sFilename)
        {
            if (sFilename != null)
            {
                curBitmap = (Bitmap)Image.FromFile(sFilename);
            }
        }

        public void BMP_Init(Bitmap BMP)
        {
            if (BMP != null)
            {
                curBitmap = BMP;
            }
        }



        public int[,] Binary_L(double value)
        {
            if (curBitmap != null)
            {
                Rectangle rect = new Rectangle(0, 0, curBitmap.Width, curBitmap.Height);
                System.Drawing.Imaging.BitmapData bmpData = curBitmap.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite, curBitmap.PixelFormat);
                pic = new int[bmpData.Width, bmpData.Height];
                unsafe
                {
                    byte* ptr = (byte*)(bmpData.Scan0);
                    for (int i = 0; i < bmpData.Height; i++)
                    {
                        for (int j = 0; j < bmpData.Width; j++)
                        {
                            if (RGB2L(ptr[2], ptr[1], ptr[0]) < value)
                            {
                                pic[j, i] = 1;
                            }
                            else
                            {
                                pic[j, i] = 0;
                            }

                            ptr += 3;
                        }
                        ptr += bmpData.Stride - bmpData.Width * 3;
                    }
                }
                curBitmap.UnlockBits(bmpData);
                return pic;
            }
            else
            {
                return null;
            }
        }

        public int[,] Binary_H(double value)
        {
            if (curBitmap != null)
            {
                Rectangle rect = new Rectangle(0, 0, curBitmap.Width, curBitmap.Height);
                System.Drawing.Imaging.BitmapData bmpData = curBitmap.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite, curBitmap.PixelFormat);
                pic = new int[bmpData.Width, bmpData.Height];
                unsafe
                {
                    byte* ptr = (byte*)(bmpData.Scan0);
                    for (int i = 0; i < bmpData.Height; i++)
                    {
                        for (int j = 0; j < bmpData.Width; j++)
                        {
                            if (RGB2H(ptr[2], ptr[1], ptr[0]) < value)
                            {
                                pic[j, i] = 1;
                            }
                            else
                            {
                                pic[j, i] = 0;
                            }

                            ptr += 3;
                        }
                        ptr += bmpData.Stride - bmpData.Width * 3;
                    }
                }
                curBitmap.UnlockBits(bmpData);
                return pic;
            }
            else
            {
                return null;
            }
        }

        public int[,] ThiningPic(int[,] input)
        {
            int[,] result = new int[input.GetLength(0), input.GetLength(1)];
            int lWidth = input.GetLength(0);
            int lHeight = input.GetLength(1);
            //   Bitmap newBmp = new Bitmap(lWidth, lHeight);

            bool bModified;            //脏标记    
            int i, j, n, m;            //循环变量
            //Color pixel;    //像素颜色值

            //四个条件
            bool bCondition1;
            bool bCondition2;
            bool bCondition3;
            bool bCondition4;

            int nCount;    //计数器    
            int[,] neighbour = new int[5, 5];    //5×5相邻区域像素值



            bModified = true;
            while (bModified)
            {
                bModified = false;

                //由于使用5×5的结构元素，为防止越界，所以不处理外围的几行和几列像素
                for (j = 2; j < lHeight - 2; j++)
                {
                    for (i = 2; i < lWidth - 2; i++)
                    {
                        bCondition1 = false;
                        bCondition2 = false;
                        bCondition3 = false;
                        bCondition4 = false;

                        if (input[i, j] == 0)
                        {

                            result[i, j] = 0;
                            continue;
                        }

                        //获得当前点相邻的5×5区域内像素值，白色用0代表，黑色用1代表
                        for (m = 0; m < 5; m++)
                        {
                            for (n = 0; n < 5; n++)
                            {
                                if (input[i + m - 2, j + n - 2] == 1) neighbour[m, n] = 1;
                                if (input[i + m - 2, j + n - 2] == 1) neighbour[m, n] = 0;
                            }
                        }

                        //逐个判断条件。
                        //判断2<=NZ(P1)<=6
                        nCount = neighbour[1, 1] + neighbour[1, 2] + neighbour[1, 3]
                                + neighbour[2, 1] + neighbour[2, 3] +
                                +neighbour[3, 1] + neighbour[3, 2] + neighbour[3, 3];
                        if (nCount >= 2 && nCount <= 6)
                        {
                            bCondition1 = true;
                        }

                        //判断Z0(P1)=1
                        nCount = 0;
                        if (neighbour[1, 2] == 0 && neighbour[1, 1] == 1)
                            nCount++;
                        if (neighbour[1, 1] == 0 && neighbour[2, 1] == 1)
                            nCount++;
                        if (neighbour[2, 1] == 0 && neighbour[3, 1] == 1)
                            nCount++;
                        if (neighbour[3, 1] == 0 && neighbour[3, 2] == 1)
                            nCount++;
                        if (neighbour[3, 2] == 0 && neighbour[3, 3] == 1)
                            nCount++;
                        if (neighbour[3, 3] == 0 && neighbour[2, 3] == 1)
                            nCount++;
                        if (neighbour[2, 3] == 0 && neighbour[1, 3] == 1)
                            nCount++;
                        if (neighbour[1, 3] == 0 && neighbour[1, 2] == 1)
                            nCount++;
                        if (nCount == 1)
                            bCondition2 = true;

                        //判断P2*P4*P8=0 or Z0(p2)!=1
                        if (neighbour[1, 2] * neighbour[2, 1] * neighbour[2, 3] == 0)
                        {
                            bCondition3 = true;
                        }
                        else
                        {
                            nCount = 0;
                            if (neighbour[0, 2] == 0 && neighbour[0, 1] == 1)
                                nCount++;
                            if (neighbour[0, 1] == 0 && neighbour[1, 1] == 1)
                                nCount++;
                            if (neighbour[1, 1] == 0 && neighbour[2, 1] == 1)
                                nCount++;
                            if (neighbour[2, 1] == 0 && neighbour[2, 2] == 1)
                                nCount++;
                            if (neighbour[2, 2] == 0 && neighbour[2, 3] == 1)
                                nCount++;
                            if (neighbour[2, 3] == 0 && neighbour[1, 3] == 1)
                                nCount++;
                            if (neighbour[1, 3] == 0 && neighbour[0, 3] == 1)
                                nCount++;
                            if (neighbour[0, 3] == 0 && neighbour[0, 2] == 1)
                                nCount++;
                            if (nCount != 1)
                                bCondition3 = true;
                        }
                        //判断P2*P4*P6=0 or Z0(p4)!=1
                        if (neighbour[1, 2] * neighbour[2, 1] * neighbour[3, 2] == 0)
                        {
                            bCondition4 = true;
                        }
                        else
                        {
                            nCount = 0;
                            if (neighbour[1, 1] == 0 && neighbour[1, 0] == 1)
                                nCount++;
                            if (neighbour[1, 0] == 0 && neighbour[2, 0] == 1)
                                nCount++;
                            if (neighbour[2, 0] == 0 && neighbour[3, 0] == 1)
                                nCount++;
                            if (neighbour[3, 0] == 0 && neighbour[3, 1] == 1)
                                nCount++;
                            if (neighbour[3, 1] == 0 && neighbour[3, 2] == 1)
                                nCount++;
                            if (neighbour[3, 2] == 0 && neighbour[2, 2] == 1)
                                nCount++;
                            if (neighbour[2, 2] == 0 && neighbour[1, 2] == 1)
                                nCount++;
                            if (neighbour[1, 2] == 0 && neighbour[1, 1] == 1)
                                nCount++;
                            if (nCount != 1)
                                bCondition4 = true;
                        }

                        if (bCondition1 && bCondition2 && bCondition3 && bCondition4)
                        {
                            result[i, j] = 0;
                            bModified = true;
                        }
                        else
                        {
                            result[i, j] = 1;
                        }
                    }
                }
            }
            // 复制细化后的图像
            //    bmpobj = newBmp;
            return result;
        }

        /// <summary> 
        /// 图像二值化1：取图片的平均灰度作为阈值，低于该值的全都为0，高于该值的全都为255 
        /// </summary> 
        /// <param name="bmp"></param> 
        /// <returns></returns> 
        public static Bitmap ConvertTo1Bpp1(Bitmap bmp)
        {
            int average = 0;
            for (int i = 0; i < bmp.Width; i++)
            {
                for (int j = 0; j < bmp.Height; j++)
                {
                    Color color = bmp.GetPixel(i, j);
                    average += color.B;
                }
            }
            average = (int)average / (bmp.Width * bmp.Height);

            for (int i = 0; i < bmp.Width; i++)
            {
                for (int j = 0; j < bmp.Height; j++)
                {
                    //获取该点的像素的RGB的颜色 
                    Color color = bmp.GetPixel(i, j);
                    int value = 255 - color.B;
                    Color newColor = value > average ? Color.FromArgb(0, 0, 0) : Color.FromArgb(255, 255, 255);
                    bmp.SetPixel(i, j, newColor);
                }
            }
            return bmp;
        }

        /// <summary> 
        /// 图像二值化2 
        /// </summary> 
        /// <param name="img"></param> 
        /// <returns></returns> 
        public static Bitmap ConvertTo1Bpp2(Bitmap img)
        {
            int w = img.Width;
            int h = img.Height;
            Bitmap bmp = new Bitmap(w, h, PixelFormat.Format1bppIndexed);
            BitmapData data = bmp.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadWrite, PixelFormat.Format1bppIndexed);
            for (int y = 0; y < h; y++)
            {
                byte[] scan = new byte[(w + 7) / 8];
                for (int x = 0; x < w; x++)
                {
                    Color c = img.GetPixel(x, y);
                    if (c.GetBrightness() >= 0.5) scan[x / 8] |= (byte)(0x80 >> (x % 8));
                }
                Marshal.Copy(scan, 0, (IntPtr)((int)data.Scan0 + data.Stride * y), scan.Length);
            }
            return bmp;
        }
    }
}
