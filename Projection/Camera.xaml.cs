using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Projection
{
    /// <summary>
    /// Camera.xaml 的交互逻辑
    /// </summary>
    public partial class Camera : System.Windows.Controls.UserControl
    {
        public Camera()
        {
            InitializeComponent();
            AllThreadInit();

            InitButton();
        }

        private void InitButton()
        {
            openCamera.IsEnabled = true;
            closeCamera.IsEnabled = false;

            OCTStartShow.IsEnabled = false;
            OCTStopShow.IsEnabled = false;

            LSOStartShow.IsEnabled = false;
            LSOStopShow.IsEnabled = false;

            OCTStartTY.IsEnabled = false;
            OCTStopTY.IsEnabled = false;
        }

        private void OpenCamera_Click(object sender, RoutedEventArgs e)
        {
            openCamera.IsEnabled = false;

            if (GlobalData.OpenPort())
            {
                if (GlobalData.OpenCamera())
                {
                    MessageBox.Show("相机打开成功！", "PROMPT");
                    closeCamera.IsEnabled = true;
                    OCTStartShow.IsEnabled = true;
                    LSOStartShow.IsEnabled = true;
                    OCTStartTY.IsEnabled = true;
                }
                else
                {
                    MessageBox.Show("相机打开失败！", "PROMPT");
                    openCamera.IsEnabled = true;
                }

            }
            else
            {
                MessageBox.Show("串口打开失败！", "PROMPT");
                openCamera.IsEnabled = true;
            }

        }

        private void CloseCamera_Click(object sender, RoutedEventArgs e)
        {
            closeCamera.IsEnabled = false;

            if (GlobalData.CloseCamera())
                if (GlobalData.ClosePort())
                {
                    openCamera.IsEnabled = true;

                    OCTStartShow.IsEnabled = false;
                    LSOStartShow.IsEnabled = false;
                    OCTStartTY.IsEnabled = false;
                }
                else
                {
                    closeCamera.IsEnabled = false;
                }
            else
            {
                closeCamera.IsEnabled = false;
            }

        }

        private void OCTShow_Click(object sender, RoutedEventArgs e)
        {
            OCTStartShow.IsEnabled = false;
            OCTStopShow.IsEnabled = true;
            OCTStartTY.IsEnabled = false;

            GlobalData.octCamera.InitData();
            if (!OCTCameraThread.IsBusy)
                OCTCameraThread.RunWorkerAsync();

            Thread.Sleep(100);
            GlobalData.cameraPage.OCTCameraStart(GlobalData.OCTCameraTriggerSignal.Test1_SHOW);

        }

        private void OCTStopShow_Click(object sender, RoutedEventArgs e)
        {
            OCTStartShow.IsEnabled = true;
            OCTStopShow.IsEnabled = false;
            OCTStartTY.IsEnabled = true;

            if (OCTCameraThread.IsBusy)
                OCTCameraThread.CancelAsync();
            Thread.Sleep(100);
            GlobalData.cameraPage.OCTCameraStop(GlobalData.OCTCameraTriggerSignal.Test1_STOP);
            GlobalData.octCamera.DeleteGPU();
        }

        private void OCTStartTY_Click(object sender, RoutedEventArgs e)
        {
            OCTStartTY.IsEnabled = false;
            OCTStopTY.IsEnabled = true;


            GlobalData.octCamera.InitData();
            if (GlobalData.octCamera.StopCamera())
            {
                if (GlobalData.octCamera.StartGetDataInit())
                {
                    OCTCameraThread.RunWorkerAsync();
                    OCTCastShowTimer.Start();
                    CatchTimer.Start();
                }
            }
            Thread.Sleep(100);

        }

        private void OCTStopTY_Click(object sender, RoutedEventArgs e)
        {
            OCTStartTY.IsEnabled = true;
            OCTStopTY.IsEnabled = false;

            OCTCameraThread.CancelAsync();
            OCTCastShowTimer.Close();

            GlobalData.octCamera.StopCamera();
            GlobalData.octCamera.DeleteGPU();

            Thread.Sleep(100);

        }

        private void LSOStartShow_Click(object sender, RoutedEventArgs e)
        {
            LSOStartShow.IsEnabled = false;
            LSOStopShow.IsEnabled = true;

            GlobalData.lsoCamera.StartLSOCamera();

            GlobalData.isCatchLSOBJ = true;
            if (!LSOCameraThread.IsBusy)
                LSOCameraThread.RunWorkerAsync();
            GlobalData.cameraPage.LSOImgShowStart("DH");

            Thread.Sleep(100);

        }

        private void LSOStopShow_Click(object sender, RoutedEventArgs e)
        {
            LSOStartShow.IsEnabled = true;
            LSOStopShow.IsEnabled = false;

            if (LSOCameraThread.IsBusy)
                LSOCameraThread.CancelAsync();

            GlobalData.cameraPage.LSOImgShowStop("DL");

            Thread.Sleep(100);
        }

        private void isSave_Click(object sender, RoutedEventArgs e)
        {
            GlobalData.isSaveOCT = isSave.IsChecked.Value;
            GlobalData.isSaveLSO = isSave.IsChecked.Value;
        }
    }
}
