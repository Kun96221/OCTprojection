using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Projection
{
    public partial class Camera : UserControl
    {

        #region OCT采集图像开始关闭
        public  void OCTCameraStart(string mind)
        {

            if (GlobalData.octCamera.StopCamera())
                GlobalData.octCamera.StartGetDataInit();
            //OCTShowTimer.Start();
            OCTShowTimer.Start();

            GlobalData.getPort.SerialPortWrite(mind);

        }


        public void OCTCatchStart(string mind)
        {
            GlobalData.octCamera.StartGetDataInit();

            GlobalData.getPort.SerialPortWrite(mind);

        }

        public void OCTCatchStop(string mind)
        {

            GlobalData.getPort.SerialPortWrite(mind);

            GlobalData.octCamera.StopCamera();

        }

        public void OCTCatchStop()
        {

            GlobalData.octCamera.StopCamera();

        }

        public void OCTCameraStop(string mind)
        {

            GlobalData.getPort.SerialPortWrite(mind);
            OCTShowTimer.Stop();

            GlobalData.octCamera.StopCamera();

        }

        public void OCTCameraClose()
        {

            GlobalData.octCamera.CloseCamera();

        }


        #endregion

        #region LSO采集图像开始关闭



        public void LSOImgShowStart(string mind)
        {
            LSOShowTimer.Start();
            GlobalData.getPort5.SerialPortWrite(mind);
        }
        // LSO 实时显示定时器停止
        public void LSOImgShowStop(string mind)
        {
            GlobalData.getPort5.SerialPortWrite(mind);
            LSOShowTimer.Stop();
        }


        #endregion

    }

}