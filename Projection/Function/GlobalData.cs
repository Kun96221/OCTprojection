using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AllClassDll;
using Port;
using System.Windows;

namespace Projection
{
    class GlobalData
    {
        public static GlobalData Instance { get; private set; } = null;

        static GlobalData()
        {
            Instance = new GlobalData();
        }

        private GlobalData()
        {
            _isOCTCatch = 0;
            isCatchLSOBJ = false;
            isSaveLSO = false;
            isSaveOCT = false;
            _OCTCameraTriggerSignal = new OCTCameraTriggerSignal();
            _octCastCompute = new OCTCastCompute();
            _lsoCompute = new LSOCompute();
            _lsoCompute.LSOInitExcu(512, 512, 1.3f, 0.7f, 0.7f);

            SaveImgFloat = new float[512][];
            SaveImg = new byte[512][];
        }

        #region  初始化、打开、关闭相机与串口
        public static bool PortInit()
        {
            _port = new MySerialPort("COM3");               //COM3  OCT相机触发信号传输串口
            _port5 = new MySerialPort("COM5");              //COM6  LSO相机触发信号传输串口

            return true;

        }

        public static bool CameraInit()
        {
            _lsoCamera = new LSOCameraCtr();
            _octCamera = new MyCamera();
            

            return true;

        }

        public static bool OpenPort()
        {
            if (_port.OpenPort())
                if (_port5.OpenPort())
                    return true;
                else
                {
                    MessageBox.Show("COM5打开失败！", "PROMPT");
                    return false;
                }
            else
            {
                MessageBox.Show("COM3打开失败！", "PROMPT");
                return false;
            }
        }

        public static bool ClosePort()
        {
            return _port.ClosePort() && _port5.ClosePort();
        }

        public static bool OpenCamera()
        {
            if (_octCamera.InitCamera())
            {
                if (_octCamera.OpenCamera())
                    if (_lsoCamera.Open())
                        return true;
                    else
                    {
                        MessageBox.Show("LSO相机打开失败！", "PROMPT");
                        return false;
                    }
                else
                {
                    MessageBox.Show("OCT相机打开失败！", "PROMPT");
                    return false;
                }

            }
            else
            {
                MessageBox.Show("OCT相机初始化失败！", "PROMPT");
                return false;

            }
        }

        public static bool CloseCamera()
        {
            if (_octCamera.CloseCamera())
                if (_lsoCamera.Close())
                    return true;
                else
                {
                    MessageBox.Show("LSO相机关闭失败！", "PROMPT");
                    return false;
                }
            else
            {
                MessageBox.Show("OCT相机关闭失败！", "PROMPT");
                return false;
            }

        }

        #endregion

        public static void PageInit()
        {
            _cameraPage = new Camera();
        }

        private static MySerialPort _port;
        public static MySerialPort getPort { get { return _port; } }

        private static MySerialPort _port5;
        public static MySerialPort getPort5 { get { return _port5; } }

        private static MyCamera _octCamera;
        public static MyCamera octCamera { get { return _octCamera; } set { _octCamera = value; } }

        private static LSOCompute _lsoCompute;
        public static LSOCompute lsoCompute { get { return _lsoCompute; } set { _lsoCompute = value; } }

        private static LSOCameraCtr _lsoCamera;
        public static LSOCameraCtr lsoCamera { get { return _lsoCamera; } set { _lsoCamera = value; } }

        private static Camera _cameraPage;
        public static Camera cameraPage { get { return _cameraPage; } }

        private static OCTCastCompute _octCastCompute;
        public static OCTCastCompute OctCastCompute { get { return _octCastCompute; } set { _octCastCompute = value; } }


        private static OCTCameraTriggerSignal _OCTCameraTriggerSignal;
        public static OCTCameraTriggerSignal OCTCameraTriggerSignal { get { return _OCTCameraTriggerSignal; } set { _OCTCameraTriggerSignal = value; } }

        private static int _isOCTCatch;
        public static int isOCTCatch { get { return _isOCTCatch; } set { _isOCTCatch = value; } }

        public static int OCTCount;                 //OCT图像采集时计数

        public static int LSOCount;                 //LSO图像采集时计数

        public static int catchNum;                //图像在有限模式时候最大的采集图片数量

        public static int saveOCTNum;             //保存图片数量

        public static bool isCatchLSOBJ;            //是否采集LSO背景

        public static bool isSaveOCT;                  //是否保存OCT图片

        public static bool isSaveLSO;                   //是否保存LSO图片

        public static float[][] SaveImgFloat;             //OCT相机图像保存float原始数据的数组

        public static byte[][] SaveImg;             //OCT相机图像保存数组

    }

    public class OCTCameraTriggerSignal
    {
        public string SCAN512_SHOW = "CJFX 000 128 001 0112 2.200 1.000 1.500 1.500 3.0 0.0 1.0 2.0 000 01 0512";
        public string SCAN512_STOP = "FXNO 000 128 001 0112 2.200 1.000 1.500 1.500 3.0 0.0 1.0 2.0 000 01 0512";
        public string SCAN512_CATCH = "CJFX 000 128 001 0112 2.200 1.000 2.000 1.000 3.0 0.0 1.0 2.0 001 01 0512 1";

        public string BLOOD3x3_SHOW = "CJFX 000 320 004 0112 1.800 1.200 1.500 1.500 3.0 0.0 1.0 2.0 000 01 0320";
        public string BLOOD3x3_STOP = "FXNO 000 320 004 0112 1.800 1.200 1.500 1.500 3.0 0.0 1.0 2.0 000 01 0320";
        public string BLOOD3x3_CATCH = "CJFX 000 320 004 0112 1.800 1.200 1.800 1.200 3.0 0.0 1.0 2.0 001 01 0320 1";

        public string BLOOD6x6_SHOW = "CJFX 000 500 002 0112 2.200 1.000 1.500 1.500 3.0 0.0 1.0 2.0 000 01 0500";
        public string BLOOD6x6_STOP = "FXNO 000 500 002 0112 2.200 1.000 1.500 1.500 3.0 0.0 1.0 2.0 000 01 0500";
        public string BLOOD6x6_CATCH = "CJFX 000 500 002 0112 2.200 1.000 2.000 1.000 3.0 0.0 1.0 2.0 001 01 0500 1";

        public string AVERAGE_X_SHOW = "CJFX 000 128 001 0112 3.000 0.000 1.500 1.500 3.0 0.0 1.0 2.0 000 01 2000";
        public string AVERAGE_X_STOP = "FXNO 000 128 001 0112 3.000 0.000 1.500 1.500 3.0 0.0 1.0 2.0 000 01 2000";
        public string AVERAGE_X_CATCH = "CJSG 000 005 020 0112 3.000 0.000 2.000 1.000 3.0 0.0 1.0 2.0 001 01 2000 1";

        public string AVERAGE_Y_SHOW = "CJFX 000 128 001 0112 3.000 0.000 1.500 1.500 3.0 0.0 1.0 2.0 000 01 2000";
        public string AVERAGE_Y_STOP = "FXNO 000 128 001 0112 3.000 0.000 1.500 1.500 3.0 0.0 1.0 2.0 000 01 2000";
        public string AVERAGE_Y_CATCH = "CJSG 000 003 030 0112 3.000 0.000 2.000 1.000 3.0 0.0 1.0 2.0 001 01 2000 1";

        public string Test1_SHOW = "CJFX 000 320 001 0112 3.000 0.000 1.500 1.500 3.0 0.0 1.0 2.0 000 01 0320 0";
        public string Test1_STOP = "FXNO 000 320 001 0112 3.000 0.000 1.500 1.500 3.0 0.0 1.0 2.0 000 01 0320 0";
        public string Test1_CATCH = "CJFX 000 320 001 0112 3.000 0.000 3.000 2.000 3.0 0.0 1.0 2.0 001 01 0320 1";

        public string SQUARE_WAVE_STOP = "QUIT";
    };

}
