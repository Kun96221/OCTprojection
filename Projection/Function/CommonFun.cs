using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Timers;
using System.Drawing;
using System.Drawing.Imaging;
using System.ComponentModel;
using System.Windows.Threading;
using System.Threading;
using System.IO;

namespace Projection
{
    class CommonFun
    {
        // 显示器每英寸显示像素个数
        public static double DpiX = 96;
        public static double DpiY = 96;

        public static int LSOWedth = 512;
        public static int LSOHeight = 512;
        public static IntPtr ArrayToIntptr(byte[] source)
        {
            if (source == null)
                return IntPtr.Zero;
            byte[] da = source;
            IntPtr ptr = Marshal.AllocHGlobal(da.Length);
            Marshal.Copy(da, 0, ptr, da.Length);
            return ptr;
        }
        public static IntPtr ArrayToIntptr(float[] source)
        {
            if (source == null)
                return IntPtr.Zero;
            float[] da = source;
            IntPtr ptr = Marshal.AllocHGlobal(da.Length * 4);
            Marshal.Copy(da, 0, ptr, da.Length);
            return ptr;
        }

        public static ImageSource BecomeImg(byte[] byteImg, int width, int height)
        {
            Bitmap temp = new Bitmap(width, height);
            temp = ToGrayBitmap(byteImg, width, height);
            ColorPalette tempPalette = temp.Palette;
            for (int j = 0; j < 256; j++)
            {
                tempPalette.Entries[j] = System.Drawing.Color.FromArgb(j, j, j);
            }
            temp.Palette = tempPalette;
            IntPtr hBitmap = temp.GetHbitmap();
            ImageSource wpfBitmap = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                                        hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            wpfBitmap.Freeze();
            temp.Dispose();
            DeleteObject(hBitmap);
            return wpfBitmap;
        }

        public static Bitmap ToGrayBitmap(byte[] rawValues, int width, int height)
        {
            //// 申请目标位图的变量，并将其内存区域锁定  
            Bitmap bmp = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);//u8类型
            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, width, height),
            ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
            //// 获取图像参数  
            int stride = bmpData.Stride;  // 扫描线的宽度  
            int offset = stride - width;  // 显示宽度与扫描线宽度的间隙     
            IntPtr iptr = bmpData.Scan0;  // 获取bmpData的内存起始位置  

            //int scanBytes = stride * height;// 用stride宽度，表示这是内存区域的大小 
            //创建放入图像的像素数据，使用2，因为它是16bpp  
            int scanBytes = width * height * 1;
            //// 用Marshal的Copy方法，将刚才得到的内存字节数组复制到BitmapData中  
            System.Runtime.InteropServices.Marshal.Copy(rawValues, 0, iptr, scanBytes);
            bmp.UnlockBits(bmpData);  // 解锁内存区域  
            //// 下面的代码是为了修改生成位图的索引表，从伪彩修改为灰度  
            ColorPalette tempPalette;
            using (Bitmap tempBmp = new Bitmap(1, 1, System.Drawing.Imaging.PixelFormat.Format8bppIndexed))
            {
                tempPalette = tempBmp.Palette;
            }
            for (int i = 0; i < 256; i++)
            {
                tempPalette.Entries[i] = System.Drawing.Color.FromArgb(i, i, i);
            }
            bmp.Palette = tempPalette;
            return bmp;
        }


        public static BitmapSource IntPtr2BitmapSource(IntPtr Img, int Width, int Height)
        {
            if (Img == IntPtr.Zero) return null;
            else return BitmapSource.Create(Width, Height, DpiX, DpiY, PixelFormats.Gray8,
                BitmapPalettes.Gray256, Img, Width * Height, Width);
        }

        public static IntPtr SubBackground(IntPtr srcImg, byte[] Backgroud, double min, double max)
        {
            byte[] Src = new byte[LSOWedth * LSOHeight];

            byte[] Dst = new byte[LSOWedth * LSOHeight];

            byte[] DstTrans = new byte[LSOWedth * LSOHeight];
            double temp;
            IntPtr DstPtr = Marshal.AllocHGlobal(LSOWedth * LSOHeight);


            Marshal.Copy(srcImg, Src, 0, LSOWedth * LSOHeight);

            for (int i = 0; i < LSOWedth * LSOHeight; i++)
            {

                temp = 255 * ((double)Src[i] - (double)Backgroud[i] - min) / (max - min);
                if (temp >= 0 && temp <= 255)
                    Dst[i] = (byte)temp;
                else if (temp > 255)
                    Dst[i] = 255;
                else
                    Dst[i] = 0;
            }

            Marshal.Copy(Dst, 0, DstPtr, LSOWedth * LSOHeight);

            return DstPtr;
        }

        #region 转置方法
        public static IntPtr TransPosition(IntPtr srcImg, int pixel, int line)
        {
            byte[] Src = new byte[pixel * line];
            byte[] Dst = new byte[pixel * line];
            IntPtr DstPtr = Marshal.AllocHGlobal(pixel * line);
            if (srcImg != IntPtr.Zero)
            {
                Marshal.Copy(srcImg, Src, 0, pixel * line);
            }
            for (int j = 0; j < pixel; j++)
            {
                for (int i = 0; i < line; i++)
                {
                    Dst[line * i + j] = Src[i + pixel * j];
                }
            }
            Marshal.Copy(Dst, 0, DstPtr, pixel * line);
            return DstPtr;
        }

        public static IntPtr TransPosition(byte[] srcImg, int pixel, int line)
        {
            byte[] Dst = new byte[pixel * line];
            IntPtr DstPtr = Marshal.AllocHGlobal(pixel * line);
            for (int j = 0; j < pixel; j++)
            {
                for (int i = 0; i < line; i++)
                {
                    Dst[line * i + j] = srcImg[i + pixel * j];
                }
            }
            Marshal.Copy(Dst, 0, DstPtr, pixel * line);
            return DstPtr;
        }

        #endregion


        [DllImport("gdi32")]
        public static extern int DeleteObject(IntPtr o);
    }
}
