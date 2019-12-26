using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Timers;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Threading;
using System.Windows.Media;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Runtime.InteropServices;
using AllClassDll;
using System.Windows.Controls;

namespace Projection
{

    public partial class Camera : UserControl
    {
        public BackgroundWorker OCTCameraThread;               //相机采集线程
        public BackgroundWorker LSOCameraThread;               //相机采集线程
        public System.Timers.Timer OCTShowTimer;
        public System.Timers.Timer LSOShowTimer;
        public System.Timers.Timer CatchTimer;

        public System.Timers.Timer OCTCastShowTimer;

        #region OCT相关指针变量
        public IntPtr temp;                                 //GetOCTClu函数返回的指针
        public IntPtr temp2;
        public IntPtr ssbcTemp;
        public IntPtr OCTCastImg;
        public IntPtr OCTCastImgDst;
        public IntPtr OCTCastImgResize;
        public IntPtr BloodStructImg;

        #endregion

        #region LSO相关指针变量
        public IntPtr LSOCamDatas;                                           //LSO从相机获取数据指针
        public IntPtr LSOBJDatas;                                            //LSO开始时获取的第一帧数据 为背景数据
        public IntPtr LSOImgIntPtr;                                          //减背景、转置后数据存放的指针
        public IntPtr LSOSubBG;                                              //减背景后数据存放的指针
        byte[] backGroundTemp;                                               //减背景算法用到的数据 存放的时LSO背景数据

        // LSO图像长宽
        public static int LSOImgWidth = (int)LSOCameraCtr.LSOWidth;
        public static int LSOImgHeight = (int)LSOCameraCtr.LSOHeight;

        static int inTimer1 = 0;                                             //线程锁变量

        #endregion



        byte[] ys;                                            //用于存放GetOCTClu（OCTCameraCtrl里实时显示函数）返回的图像指针 复制数据
        float[] orinFloat;                                    //用于存放GetData（OCTCameraCtrl里采集函数）返回的图像指针 原始光谱数据
        byte[] _UCharCast50Trans;
        byte[] _UCharCast50IMG;


        static int inTimer = 0;                                 //线程锁变量
        static int inTimerCast = 0;                             //线程锁变量
        static int OCTCatchs = 0;                               //开始采集模式时 定时器计算次数
        static int OCTCastImgNum = 0;                           //test测试投影图采集计数
        static int OCTCastTotal = 320;                           //test测试模式采集总数
        static int octOrinPixel = 2048;                         //采集原始光谱数据时 pixel值

        static int afterSSBC_Pixel = 885;
        static int afterSSBC_Line = 320;

        Bitmap OCTStructreuAveImg;
        Bitmap OCT50CastIMG;


        public void AllThreadInit()
        {
            OCTCameraThread = new BackgroundWorker();
            OCTCameraThread.WorkerReportsProgress = true;
            OCTCameraThread.WorkerSupportsCancellation = true;
            OCTCameraThread.DoWork += OCTCameraThread_DoWork;

            LSOCameraThread = new BackgroundWorker();
            LSOCameraThread.WorkerReportsProgress = true;
            LSOCameraThread.WorkerSupportsCancellation = true;
            LSOCameraThread.DoWork += LSOCameraThread_DoWork;


            OCTShowTimer = new System.Timers.Timer(1000.0 / 60);
            OCTShowTimer.Elapsed += OCTImgShow;
            OCTShowTimer.AutoReset = true;

            OCTCastShowTimer = new System.Timers.Timer(1000.0 / 60);
            OCTCastShowTimer.Elapsed += OCTCastShow;
            OCTCastShowTimer.AutoReset = true;

            LSOShowTimer = new System.Timers.Timer(1000.0 / 60);
            LSOShowTimer.Elapsed += LSOImgShow;
            LSOShowTimer.AutoReset = true;


            CatchTimer = new System.Timers.Timer(500);
            CatchTimer.Elapsed += CatchInit;
            //CatchTimer.AutoReset = true;

            // Line 512   N-Frame 50
            temp = new IntPtr();
            temp2 = new IntPtr();
            LSOCamDatas = Marshal.AllocHGlobal(LSOImgHeight * LSOImgWidth);
            LSOBJDatas = Marshal.AllocHGlobal(LSOImgHeight * LSOImgWidth);
            LSOImgIntPtr = Marshal.AllocHGlobal(LSOImgHeight * LSOImgWidth);
            LSOSubBG = Marshal.AllocHGlobal(LSOImgHeight * LSOImgWidth);

            //temp2= Marshal.AllocHGlobal(octOrinPixel * afterSSBC_Line);

            OCTCastImgDst = Marshal.AllocHGlobal(afterSSBC_Line * OCTCastTotal);
            OCTCastImgResize = Marshal.AllocHGlobal(afterSSBC_Line * afterSSBC_Line);
            _UCharCast50Trans = new byte[afterSSBC_Pixel * afterSSBC_Line * OCTCastTotal];
            backGroundTemp = new byte[LSOImgHeight * LSOImgWidth];

        }

        // LSO 实时显示定时器
        public void LSOImgShow(object sender, ElapsedEventArgs e)
        {
            System.Timers.Timer tmr = sender as System.Timers.Timer;
            if (Interlocked.Exchange(ref inTimer1, 1) == 0)
            {
                if (IntPtr.Zero != LSOImgIntPtr)
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        image1.Source = GetBitmapSourceLSOImg();

                    }));
                Interlocked.Exchange(ref inTimer1, 0);
            }
        }

        public void OCTImgShow(object sender, ElapsedEventArgs e)
        {
            if (Interlocked.Exchange(ref inTimer, 1) == 0)
            {

                temp = GlobalData.octCamera.GetOCTClu();

                #region 非测试模式case5的实时显示
                if (temp != IntPtr.Zero)
                {
                    OCTStructreuAveImg = new Bitmap(afterSSBC_Line, afterSSBC_Pixel);

                    ys = new byte[afterSSBC_Pixel * afterSSBC_Line];
                    Marshal.Copy(temp, ys, 0, afterSSBC_Pixel * afterSSBC_Line);
                    OCTStructreuAveImg = CommonFun.ToGrayBitmap(ys, afterSSBC_Line, afterSSBC_Pixel);
                    ColorPalette tempPalette = OCTStructreuAveImg.Palette;
                    for (int j = 0; j < 256; j++)
                    {
                        tempPalette.Entries[j] = System.Drawing.Color.FromArgb(j, j, j);
                    }
                    OCTStructreuAveImg.Palette = tempPalette;
                    IntPtr hBitmap = OCTStructreuAveImg.GetHbitmap();
                    ImageSource wpfBitmap = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                                                hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                    wpfBitmap.Freeze();
                    OCTStructreuAveImg.Dispose();
                    CommonFun.DeleteObject(hBitmap);
                    image.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        image.Source = wpfBitmap;
                    }), DispatcherPriority.Normal);
                }


                #endregion
                Interlocked.Exchange(ref inTimer, 0);
            }


        }

        public void OCTCastShow(object sender, ElapsedEventArgs e)
        {
            System.Timers.Timer tmr = sender as System.Timers.Timer;


            if (Interlocked.Exchange(ref inTimerCast, 1) == 0)
            {
                if (OCTCastImg != IntPtr.Zero )
                {
                    GlobalData.OctCastCompute.CastCompute(OCTCastImg, OCTCastImgDst, afterSSBC_Pixel, afterSSBC_Line, OCTCastTotal, 2.0f);
                    //ReSize.ReSize2D(OCTCastImgDst, OCTCastTotal, afterSSBC_Line, afterSSBC_Line, afterSSBC_Line, OCTCastImgResize);


                    #region 保存OCT投影图像 一张 512*512
                    if (GlobalData.isSaveOCT)
                    {
                        string adress = "C:\\Users\\admin\\Desktop\\OCTCast512x512.txt";
                        IntPtr saveAdress = Marshal.StringToHGlobalAnsi(adress);
                        GlobalData.octCamera.SaveByteData(saveAdress, OCTCastImgDst, afterSSBC_Line * afterSSBC_Line);
                        GlobalData.isSaveOCT = false;
                    }

                    #endregion


                    OCT50CastIMG = new Bitmap(afterSSBC_Line, afterSSBC_Line);
                    _UCharCast50IMG = new byte[afterSSBC_Line * afterSSBC_Line];
                    Marshal.Copy(OCTCastImgDst, _UCharCast50IMG, 0, afterSSBC_Line * afterSSBC_Line);

                    OCT50CastIMG = CommonFun.ToGrayBitmap(_UCharCast50IMG, afterSSBC_Line, afterSSBC_Line);
                    ColorPalette tempPalette = OCT50CastIMG.Palette;
                    for (int j = 0; j < 256; j++)
                    {
                        tempPalette.Entries[j] = System.Drawing.Color.FromArgb(j, j, j);
                    }
                    OCT50CastIMG.Palette = tempPalette;
                    IntPtr hBitmap = OCT50CastIMG.GetHbitmap();
                    ImageSource castBitmap = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                                                hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                    castBitmap.Freeze();
                    OCT50CastIMG.Dispose();
                    CommonFun.DeleteObject(hBitmap);
                    image.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        image.Source = castBitmap;

                    }), DispatcherPriority.Normal);
                    OCTCastImg = IntPtr.Zero;
                    CatchTimer.Start();
                    //OCTCastShowTimer.Stop();
                }
                Interlocked.Exchange(ref inTimerCast, 0);

            }



        }

        public void OCTCameraThread_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            while (!worker.CancellationPending)
            {
                if (GlobalData.isOCTCatch == 1)
                {
                    if (GlobalData.OCTCount < GlobalData.catchNum)
                    {
                        temp2=GlobalData.octCamera.GetData(1);
                        orinFloat = new float[octOrinPixel * afterSSBC_Line];

                        if (temp2 != IntPtr.Zero)
                        {
                            Marshal.Copy(temp2, orinFloat, 0, octOrinPixel * afterSSBC_Line);
                            GlobalData.SaveImgFloat[GlobalData.OCTCount] = orinFloat;
                        }
             
                        GlobalData.OCTCount++;
                    }
                    if (GlobalData.OCTCount == GlobalData.catchNum)
                    {
                        GlobalData.isOCTCatch = 0;
                        GlobalData.OCTCount = 0;
                        OCTCatchStop(GlobalData.OCTCameraTriggerSignal.SQUARE_WAVE_STOP);
                        IntPtr oringFloatIntPtr = new IntPtr();

                        for (int i = 0; i < GlobalData.catchNum; i++)
                        {

                            oringFloatIntPtr = CommonFun.ArrayToIntptr(GlobalData.SaveImgFloat[i]);
                            ssbcTemp = GlobalData.octCamera.Ssbc(oringFloatIntPtr);

                            ys = new byte[afterSSBC_Pixel * afterSSBC_Line];
                            if (ssbcTemp != IntPtr.Zero)
                                Marshal.Copy(ssbcTemp, ys, 0, afterSSBC_Pixel * afterSSBC_Line);
                            GlobalData.SaveImg[i] = ys;
                            Array.Copy(GlobalData.SaveImg[i], 0, _UCharCast50Trans, i * afterSSBC_Pixel * afterSSBC_Line, GlobalData.SaveImg[i].Length);

                        }
                        OCTCastImg = CommonFun.ArrayToIntptr(_UCharCast50Trans);

                        GlobalData.octCamera.ResetCount();
                    }
                }
                else
                {
                    GlobalData.octCamera.GetData(-1);
                }
            }
        }

        public void LSOCameraThread_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            while (!worker.CancellationPending)
            {
                if (GlobalData.isCatchLSOBJ == true)
                {
                    LSOBJDatas = GlobalData.lsoCamera.GetOneFrame();
                    if(LSOBJDatas!=IntPtr.Zero)
                    {
                        Marshal.Copy(LSOBJDatas, backGroundTemp, 0, LSOImgHeight * LSOImgWidth);
                        LSOBJDatas = IntPtr.Zero;
                        GlobalData.isCatchLSOBJ = false;
                    }
                }
                else
                {
                    LSOCamDatas = GlobalData.lsoCamera.GetOneFrame();

                    if (LSOCamDatas != IntPtr.Zero)
                    {
                        Marshal.Copy(backGroundTemp, 0, LSOSubBG,  LSOImgHeight * LSOImgWidth);

                        //LSOSubBG = CommonFun.SubBackground(LSOCamDatas, backGroundTemp, 0, 50.0);
                        //LSOImgIntPtr = CommonFun.TransPosition(LSOSubBG, LSOImgHeight, LSOImgWidth);
                        LSOImgIntPtr = GlobalData.lsoCompute.LSOExcutex(LSOCamDatas, LSOSubBG, LSOImgHeight, LSOImgWidth);
                        if (GlobalData.isSaveLSO)
                        {
                            string adress = "C:\\Users\\admin\\Desktop\\LSO512x512.txt";
                            IntPtr saveAdress = Marshal.StringToHGlobalAnsi(adress);
                            GlobalData.octCamera.SaveByteData(saveAdress, LSOImgIntPtr, LSOImgHeight * LSOImgWidth);
                            GlobalData.isSaveLSO = false;
                        }

                        // 将LSO图像、LSO当前帧数、OCT当前帧数传入队列，以供跟踪线程使用
                        GlobalData.LSOCount++;
                    }

                }

            }
        }

        public void CatchInit(object sender, ElapsedEventArgs e)
        {
            if (OCTCatchs == 2)
            {
                if (Interlocked.Exchange(ref inTimer, 1) == 0)
                {
                    GlobalData.OCTCount = 0;
                    GlobalData.LSOCount = 0;
                    GlobalData.isOCTCatch = 1;
                    GlobalData.catchNum = 320;
                    GlobalData.saveOCTNum = 50;
                    OCTCatchStart(GlobalData.OCTCameraTriggerSignal.Test1_CATCH);
                    CatchTimer.Stop();
                }
            }

            OCTCatchs++;
            if (OCTCatchs > 2)
            {
                OCTCatchs = 0;
            }
            Interlocked.Exchange(ref inTimer, 0);
        }

        public BitmapSource GetBitmapSourceLSOImg()
        {
                return CommonFun.IntPtr2BitmapSource(LSOImgIntPtr, (int)LSOCameraCtr.LSOWidth, (int)LSOCameraCtr.LSOHeight);

        }



    }



}
