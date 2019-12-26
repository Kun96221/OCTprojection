using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MvCamCtrl.NET;
using System.Runtime.InteropServices;
using System.Windows;

namespace Projection
{
    public class LSOCameraCtr
    {

        MyCamera.MV_CC_DEVICE_INFO_LIST m_pDeviceList;
        private MyCamera m_pMyCamera;

        #region 相机配置属性变量
        public static uint LSOWidth = 512;
        public static uint LSOHeight = 512;
        public static uint nDataSize = LSOWidth * LSOHeight;
        public static uint AcquisitionLineRate = 10000;     //线速度
        public static int ntimeOut = 3000;
        public static float ExposureTime = 80.0f;           //曝光时间 
        public int TriggerType = 9;
        // ch:用于从驱动获取图像的缓存 | en:Buffer for getting image from driver
        byte[] m_pBufForDriver = new byte[3072 * 2048 * 3];

        // ch:用于保存图像的缓存 | en:Buffer for saving image
        byte[] m_pBufForSaveImage = new byte[3072 * 2048 * 3 * 3 + 2048];
        #endregion

        // 判断相机配置是否成功，成功则开始捕获
        public bool StartLSOCamera()
        {
            if (LSOCameraConfig())
                return Start();
            else return false;
        }

        // 寻找LSO相机设备
        private void DeviceListAcq()
        {
            int nRet;
            // ch:创建设备列表 en:Create Device List
            System.GC.Collect();
            nRet = MyCamera.MV_CC_EnumDevices_NET(MyCamera.MV_GIGE_DEVICE | MyCamera.MV_USB_DEVICE, ref m_pDeviceList);
            if (0 != nRet)
            {
                ShowErrorMsg("Enumerate devices fail!", 0);
                return;
            }

            // ch:存入所有设备名
            for (int i = 0; i < m_pDeviceList.nDeviceNum; i++)
            {
                MyCamera.MV_CC_DEVICE_INFO device = (MyCamera.MV_CC_DEVICE_INFO)Marshal.PtrToStructure(m_pDeviceList.pDeviceInfo[i], typeof(MyCamera.MV_CC_DEVICE_INFO));
                if (device.nTLayerType == MyCamera.MV_GIGE_DEVICE)
                {
                    IntPtr buffer = Marshal.UnsafeAddrOfPinnedArrayElement(device.SpecialInfo.stGigEInfo, 0);
                    MyCamera.MV_GIGE_DEVICE_INFO gigeInfo = (MyCamera.MV_GIGE_DEVICE_INFO)Marshal.PtrToStructure(buffer, typeof(MyCamera.MV_GIGE_DEVICE_INFO));

                }
                else if (device.nTLayerType == MyCamera.MV_USB_DEVICE)
                {
                    IntPtr buffer = Marshal.UnsafeAddrOfPinnedArrayElement(device.SpecialInfo.stUsb3VInfo, 0);
                    MyCamera.MV_USB3_DEVICE_INFO usbInfo = (MyCamera.MV_USB3_DEVICE_INFO)Marshal.PtrToStructure(buffer, typeof(MyCamera.MV_USB3_DEVICE_INFO));
                }
            }
        }

        //打开设备
        public bool Open()
        {
            m_pDeviceList = new MyCamera.MV_CC_DEVICE_INFO_LIST();
            DeviceListAcq();
            if (m_pDeviceList.nDeviceNum == 0)
            {
                ShowErrorMsg("No device, please select", 0);
                return false;
            }
            int nRet = -1;

            // ch:获取选择的设备信息 | en:Get selected device information
            MyCamera.MV_CC_DEVICE_INFO device =
                (MyCamera.MV_CC_DEVICE_INFO)Marshal.PtrToStructure(m_pDeviceList.pDeviceInfo[0],
                                                              typeof(MyCamera.MV_CC_DEVICE_INFO));

            // ch:打开设备 | en:Open device
            if (null == m_pMyCamera)
            {
                m_pMyCamera = new MyCamera();
                if (null == m_pMyCamera)
                {
                    return false;
                }
            }

            nRet = m_pMyCamera.MV_CC_CreateDevice_NET(ref device);
            if (MyCamera.MV_OK != nRet)
            {
                return false;
            }

            nRet = m_pMyCamera.MV_CC_OpenDevice_NET();
            if (MyCamera.MV_OK != nRet)
            {
                m_pMyCamera.MV_CC_DestroyDevice_NET();
                ShowErrorMsg("Device open fail!", nRet);
                return false;
            }

            return true;

            // ch:设置采集连续模式 | en:Set Continues Aquisition Mode
            //m_pMyCamera.MV_CC_SetEnumValue_NET("AcquisitionMode", 2);// ch:工作在连续模式 | en:Acquisition On Continuous Mode
            //m_pMyCamera.MV_CC_SetEnumValue_NET("TriggerMode", 0);    // ch:连续模式 | en:Continuous

            //bnGetParam_Click(null, null);// ch:获取参数 | en:Get parameters

            // ch:控件操作 | en:Control operation
            //SetCtrlWhenOpen();
        }

        //开始采集
        public bool Start()
        {
            int nRet;

            // ch:开始采集 | en:Start Grabbing
            nRet = m_pMyCamera.MV_CC_StartGrabbing_NET();
            if (MyCamera.MV_OK != nRet)
            {
                ShowErrorMsg("Trigger Fail!", nRet);
                return false;
            }
            return true;

            // ch:控件操作 | en:Control Operation
            // SetCtrlWhenStartGrab();

            // ch:标志位置位true | en:Set position bit true


            // ch:显示 | en:Display
            //nRet = m_pMyCamera.MV_CC_Display_NET(GlobalData.getCameraPage.LSOImg.Handle);
            //if (MyCamera.MV_OK != nRet)
            //{
            //    ShowErrorMsg("Display Fail！", nRet);
            //}
        }

        //停止采集
        public void Stop()
        {
            int nRet = -1;
            // ch:停止采集 | en:Stop Grabbing
            nRet = m_pMyCamera.MV_CC_StopGrabbing_NET();
            if (nRet != MyCamera.MV_OK)
            {
                ShowErrorMsg("Stop Grabbing Fail!", nRet);
            }

            // ch:标志位设为false | en:Set flag bit false

            // ch:控件操作 | en:Control Operation
            // SetCtrlWhenStopGrab();

        }

        //关闭设备
        public bool Close()
        {
            // ch:关闭设备 | en:Close Device
            int nRet;

            nRet = m_pMyCamera.MV_CC_CloseDevice_NET();
            if (MyCamera.MV_OK != nRet)
            {
                return false;
            }

            nRet = m_pMyCamera.MV_CC_DestroyDevice_NET();
            if (MyCamera.MV_OK != nRet)
            {
                return false;
            }

            return true;
            // ch:控件操作 | en:Control Operation
            // SetCtrlWhenClose();

            // ch:取流标志位清零 | en:Reset flow flag bit
        }

        //获取一帧数据
        public IntPtr GetOneFrame()
        {
            int nRet;

            m_pBufForDriver = new byte[nDataSize];
            IntPtr pData = Marshal.UnsafeAddrOfPinnedArrayElement(m_pBufForDriver, 0);
            MyCamera.MV_FRAME_OUT_INFO_EX stFrameInfo = new MyCamera.MV_FRAME_OUT_INFO_EX();

            nRet = m_pMyCamera.MV_CC_GetOneFrameTimeout_NET(pData, nDataSize, ref stFrameInfo, ntimeOut);
            if (MyCamera.MV_OK != nRet) return IntPtr.Zero;

            return pData;

        }

        //配置相机
        public bool LSOCameraConfig()
        {
            if (MyCamera.MV_OK != m_pMyCamera.MV_CC_SetEnumValue_NET("BinningHorizontal", 2048 / LSOWidth)) return false;
            if (MyCamera.MV_OK != m_pMyCamera.MV_CC_SetBoolValue_NET("ReverseX", true)) return false;
            if (MyCamera.MV_OK != m_pMyCamera.MV_CC_SetIntValue_NET("Height", LSOHeight)) return false;
            if (MyCamera.MV_OK != m_pMyCamera.MV_CC_SetIntValue_NET("AcquisitionLineRate", AcquisitionLineRate)) return false;
            switch (TriggerType)
            {
                case 0:
                    if (MyCamera.MV_OK != m_pMyCamera.MV_CC_SetEnumValue_NET("TriggerSelector", 6)) return false;
                    if (MyCamera.MV_OK != m_pMyCamera.MV_CC_SetEnumValue_NET("TriggerMode", 0)) return false;
                    if (MyCamera.MV_OK != m_pMyCamera.MV_CC_SetEnumValue_NET("TriggerSelector", 9)) return false;
                    if (MyCamera.MV_OK != m_pMyCamera.MV_CC_SetEnumValue_NET("TriggerMode", 0)) return false;
                    break;
                case 6:
                    if (MyCamera.MV_OK != m_pMyCamera.MV_CC_SetEnumValue_NET("TriggerSelector", 6)) return false;
                    if (MyCamera.MV_OK != m_pMyCamera.MV_CC_SetEnumValue_NET("TriggerMode", 1)) return false;
                    if (MyCamera.MV_OK != m_pMyCamera.MV_CC_SetEnumValue_NET("TriggerSelector", 9)) return false;
                    if (MyCamera.MV_OK != m_pMyCamera.MV_CC_SetEnumValue_NET("TriggerMode", 0)) return false;
                    break;
                case 9:
                    if (MyCamera.MV_OK != m_pMyCamera.MV_CC_SetEnumValue_NET("TriggerSelector", 6)) return false;
                    if (MyCamera.MV_OK != m_pMyCamera.MV_CC_SetEnumValue_NET("TriggerMode", 0)) return false;
                    if (MyCamera.MV_OK != m_pMyCamera.MV_CC_SetEnumValue_NET("TriggerSelector", 9)) return false;
                    if (MyCamera.MV_OK != m_pMyCamera.MV_CC_SetEnumValue_NET("TriggerMode", 1)) return false;
                    break;
                default:
                    break;
            }
            if (MyCamera.MV_OK != m_pMyCamera.MV_CC_SetFloatValue_NET("ExposureTime", ExposureTime)) return false;
            return true;
        }

        //错误信息显示
        private void ShowErrorMsg(string csMessage, int nErrorNum)
        {
            string errorMsg;
            if (nErrorNum == 0)
            {
                errorMsg = csMessage;
            }
            else
            {
                errorMsg = csMessage + ": Error =" + String.Format("{0:X}", nErrorNum);
            }

            switch (nErrorNum)
            {
                case MyCamera.MV_E_HANDLE: errorMsg += " Error or invalid handle "; break;
                case MyCamera.MV_E_SUPPORT: errorMsg += " Not supported function "; break;
                case MyCamera.MV_E_BUFOVER: errorMsg += " Cache is full "; break;
                case MyCamera.MV_E_CALLORDER: errorMsg += " Function calling order error "; break;
                case MyCamera.MV_E_PARAMETER: errorMsg += " Incorrect parameter "; break;
                case MyCamera.MV_E_RESOURCE: errorMsg += " Applying resource failed "; break;
                case MyCamera.MV_E_NODATA: errorMsg += " No data "; break;
                case MyCamera.MV_E_PRECONDITION: errorMsg += " Precondition error, or running environment changed "; break;
                case MyCamera.MV_E_VERSION: errorMsg += " Version mismatches "; break;
                case MyCamera.MV_E_NOENOUGH_BUF: errorMsg += " Insufficient memory "; break;
                case MyCamera.MV_E_UNKNOW: errorMsg += " Unknown error "; break;
                case MyCamera.MV_E_GC_GENERIC: errorMsg += " General error "; break;
                case MyCamera.MV_E_GC_ACCESS: errorMsg += " Node accessing condition error "; break;
                case MyCamera.MV_E_ACCESS_DENIED: errorMsg += " No permission "; break;
                case MyCamera.MV_E_BUSY: errorMsg += " Device is busy, or network disconnected "; break;
                case MyCamera.MV_E_NETER: errorMsg += " Network error "; break;
            }

            MessageBox.Show(errorMsg, "PROMPT");
        }

    }
}

