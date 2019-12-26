using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.IO.Ports;
using System.Windows.Threading;
using System.Threading;
using System.Collections;
//abc
namespace Port
{

    public enum SendMode
    {
        Character,  //字符
        Hex         //十六进制
    }
    public enum ReceiveMode
    {
        Character,  //字符显示
        Hex,        //十六进制
        Decimal,    //十进制
        Octal,      //八进制
        Binary      //二进制
    }

    #region 字符串转换类
    public static class Utilities
    {
        public static string BytesToText(List<byte> bytesBuffer, ReceiveMode mode, Encoding encoding)
        {
            string result = "";

            if (mode == ReceiveMode.Character)
            {
                return encoding.GetString(bytesBuffer.ToArray<byte>());
            }

            foreach (var item in bytesBuffer)
            {
                switch (mode)
                {
                    case ReceiveMode.Hex:
                        result += Convert.ToString(item, 16).ToUpper() + " ";
                        break;
                    case ReceiveMode.Decimal:
                        result += Convert.ToString(item, 10) + " ";
                        break;
                    case ReceiveMode.Octal:
                        result += Convert.ToString(item, 8) + " ";
                        break;
                    case ReceiveMode.Binary:
                        result += Convert.ToString(item, 2) + " ";
                        break;
                    default:
                        break;
                }
            }

            return result;
        }

        public static string ToSpecifiedText(string text, SendMode mode, Encoding encoding)
        {
            string result = "";
            switch (mode)
            {
                case SendMode.Character:
                    text = text.Trim();

                    // 转换成字节
                    List<byte> src = new List<byte>();

                    string[] grp = text.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    foreach (var item in grp)
                    {
                        src.Add(Convert.ToByte(item, 16));
                    }

                    // 转换成字符串
                    result = encoding.GetString(src.ToArray<byte>());
                    break;

                case SendMode.Hex:

                    byte[] byteStr = encoding.GetBytes(text.ToCharArray());

                    foreach (var item in byteStr)
                    {
                        result += Convert.ToString(item, 16).ToUpper() + " ";
                    }
                    break;
                default:
                    break;
            }

            return result.Trim();
        }

    }
    #endregion

    #region 自定义串口类
    public class MySerialPort
    {
        public SerialPort serialPort = new SerialPort();  //初始化C#自带的端口类对象

        private DispatcherTimer checkTimer = new DispatcherTimer(); //初始化一个定时器，保证缓冲区没满也可以及时处理数据

        private string myPort = "COM3";  //串口参数
        private string myPort2 = "COM5";  //串口参数
        private int myBt = 115200;        //通讯波特率
        private Parity Check = Parity.None;  //奇偶检验
        private int sendDataBit = 8;         //发送数据位数
        private StopBits stopBits = StopBits.One; //停止位
        private Encoding encoding = Encoding.Default; //字节编码方式
        private SendMode sendMode = SendMode.Character; //发送模式
        private ReceiveMode receiveMode = ReceiveMode.Character; //接收模式

        public string temp;
        public MySerialPort()
        {
            serialPort.DataReceived += SerialPort_DataReceived;
            InitCheckTimer();
            serialPort.PortName = myPort;
            serialPort.BaudRate = myBt;
            serialPort.Parity = Check;
            serialPort.DataBits = sendDataBit;
            serialPort.StopBits = stopBits;
            serialPort.Encoding = encoding;

        }

        public MySerialPort(int i)
        {
            serialPort.DataReceived += SerialPort_DataReceived;
            InitCheckTimer();
            serialPort.PortName = myPort2;
            serialPort.BaudRate = myBt;
            serialPort.Parity = Check;
            serialPort.DataBits = sendDataBit;
            serialPort.StopBits = stopBits;
            serialPort.Encoding = encoding;
        }

        public MySerialPort(string com)
        {
            serialPort.DataReceived += SerialPort_DataReceived;
            InitCheckTimer();
            serialPort.PortName = com;
            serialPort.BaudRate = myBt;
            serialPort.Parity = Check;
            serialPort.DataBits = sendDataBit;
            serialPort.StopBits = stopBits;
            serialPort.Encoding = encoding;
        }

        public bool OpenPort(string myPort, int myBt, Parity Check, int sendDataBit, StopBits stopBits, Encoding encoding)
        {
            serialPort.PortName = myPort;
            serialPort.BaudRate = myBt;
            serialPort.Parity = Check;
            serialPort.DataBits = sendDataBit;
            serialPort.StopBits = stopBits;
            serialPort.Encoding = encoding;

            bool flag = false;
            try
            {
                serialPort.Open();
                serialPort.DiscardInBuffer();
                serialPort.DiscardOutBuffer(); //清空输入缓冲区和输出缓冲区

                flag = true;
            }
            catch (Exception ex)
            {
                //等到需要的时候再加上弹窗
            }

            return flag;
        }
        public bool OpenPort()
        {
            bool flag = false;
            try
            {
                serialPort.Open();
                serialPort.DiscardInBuffer();
                serialPort.DiscardOutBuffer(); //清空输入缓冲区和输出缓冲区

                flag = true;
            }
            catch (Exception ex)
            {
                //等到需要的时候再加上弹窗
            }



            return flag;
        }
        public bool ClosePort()
        {
            bool flag = false;

            try
            {
                serialPort.Close();
                flag = true;
            }
            catch (Exception ex)
            {

            }

            return flag;
        }
        public bool SerialPortWrite(string textData)
        {
            SerialPortWrite(textData, false);
            return false;
        }
        private string appendContent = "\r\n";
        private bool SerialPortWrite(string textData, bool reportEnable)
        {
            if (serialPort == null)
            {
                return false;
            }

            if (serialPort.IsOpen == false)
            {

                return false;
            }

            try
            {             
                if (sendMode == SendMode.Character)
                {
                    serialPort.Write(textData + appendContent);
                }
                else if (sendMode == SendMode.Hex)
                {
                    string[] grp = textData.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    List<byte> list = new List<byte>();

                    foreach (var item in grp)
                    {
                        list.Add(Convert.ToByte(item, 16));
                    }

                    serialPort.Write(list.ToArray(), 0, list.Count);
                }

                if (reportEnable)
                {
                    // 报告发送成功的消息，提示用户。
                    //  Information(string.Format("成功发送：{0}。", textData));
                }
            }
            catch (Exception ex)
            {
                // Alert(ex.Message);
                return false;
            }

            return true;
        }


        private List<byte> receiveBuffer = new List<byte>();

        // 一个阈值，当接收的字节数大于这么多字节数之后，就将当前的buffer内容交由数据处理的线程
        // 分析。这里存在一个问题，假如最后一次传输之后，缓冲区并没有达到阈值字节数，那么可能就
        // 没法启动数据处理的线程将最后一次传输的数据处理了。这里应当设定某种策略来保证数据能够
        // 在尽可能短的时间内得到处理。
        private const int THRESH_VALUE = 128;

        Queue recQueue = new Queue();//接收数据过程中，将接收数据传入队列中

        public string Moniter()
        {
            string recData;
            while (true)
            {
                List<byte> recBuffer = (List<byte>)recQueue.Dequeue();//出列Dequeue（全局）
                byte dt = recBuffer.Last<byte>();
                //Encoding.Default.GetString
                recData = Utilities.BytesToText(recBuffer, receiveMode, serialPort.Encoding);
                //int number=int.Parse(recData);
                string[] splitData = recData.Split(new char[] { ' ' });
                string result = splitData[splitData.Length - 2];
                return result;
            }
        }

        private bool shouldClear = true;

        /// <summary>
        /// 更新：采用一个缓冲区，当有数据到达时，把字节读取出来暂存到缓冲区中，缓冲区到达定值
        /// 时，在显示区显示数据即可。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SerialPort_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            System.IO.Ports.SerialPort sp = sender as System.IO.Ports.SerialPort;

            if (sp != null)
            {
                // 临时缓冲区将保存串口缓冲区的所有数据
                int bytesToRead = sp.BytesToRead;
                byte[] tempBuffer = new byte[bytesToRead];

                // 将缓冲区所有字节读取出来
                sp.Read(tempBuffer, 0, bytesToRead);

                // 检查是否需要清空全局缓冲区先
                if (shouldClear)
                {
                    receiveBuffer.Clear();
                    shouldClear = false;
                }

                // 暂存缓冲区字节到全局缓冲区中等待处理
                receiveBuffer.AddRange(tempBuffer);

                if (receiveBuffer.Count >= THRESH_VALUE)
                {
                    //Dispatcher.Invoke(new Action(() =>
                    //{
                    //    recvDataRichTextBox.AppendText("Process data.\n");
                    //}));
                    // 进行数据处理，采用新的线程进行处理。
                    Thread dataHandler = new Thread(new ParameterizedThreadStart(ReceivedDataHandler));
                    dataHandler.Start(receiveBuffer);
                }

                // 启动定时器，防止因为一直没有到达缓冲区字节阈值，而导致接收到的数据一直留存在缓冲区中无法处理。
                StartCheckTimer();
            }
        }


        private void CheckTimer_Tick(object sender, EventArgs e)
        {
            // 触发了就把定时器关掉，防止重复触发。
            StopCheckTimer();

            // 只有没有到达阈值的情况下才会强制其启动新的线程处理缓冲区数据。
            if (receiveBuffer.Count < THRESH_VALUE)
            {
                //recvDataRichTextBox.AppendText("Timeout!\n");
                // 进行数据处理，采用新的线程进行处理。
                Thread dataHandler = new Thread(new ParameterizedThreadStart(ReceivedDataHandler));
                dataHandler.Start(receiveBuffer);
            }
        }


        private void ReceivedDataHandler(object obj)
        {
            List<byte> recvBuffer = new List<byte>();
            recvBuffer.AddRange((List<byte>)obj);
            Moniter();
            if (recvBuffer.Count == 0)
            {
                return;
            }

            // 必须应当保证全局缓冲区的数据能够被完整地备份出来，这样才能进行进一步的处理。
            shouldClear = true;
            string re = Utilities.BytesToText(recvBuffer, receiveMode, serialPort.Encoding);
            if (re == "STOPSTOP")
            {
                temp = re;
            }
            //this.Dispatcher.Invoke(new Action(() =>
            //{
            //    if (showReceiveData)
            //    {
            //        // 根据显示模式显示接收到的字节.
            //        recvDataRichTextBox.AppendText(Utilities.BytesToText(recvBuffer, receiveMode, serialPort.Encoding));
            //        recvDataRichTextBox.ScrollToEnd();
            //    }
            //    dataRecvStatusBarItem.Visibility = Visibility.Collapsed;
            //}));---------------------------------------------------------------接收字符串消息

            // TO-DO：
            // 处理数据，比如解析指令等等
        }

        private const int TIMEOUT = 50;
        private void InitCheckTimer()
        {
            // 如果缓冲区中有数据，并且定时时间达到前依然没有得到处理，将会自动触发处理函数。
            checkTimer.Interval = new TimeSpan(0, 0, 0, 0, TIMEOUT);
            checkTimer.IsEnabled = false;
            checkTimer.Tick += CheckTimer_Tick;
        }

        private void StartCheckTimer()
        {
            checkTimer.IsEnabled = true;
            checkTimer.Start();
        }

        private void StopCheckTimer()
        {
            checkTimer.IsEnabled = false;
            checkTimer.Stop();
        }
    }
    #endregion

}

