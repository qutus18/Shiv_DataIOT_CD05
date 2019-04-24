using System;
using System.IO.Ports;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Threading;

namespace SHIV_Data_Weigh
{
    public class BalanceObj
    {
        #region Khai báo
        // Khai báo đối tượng hiển thị giá trị cân
        public WeighObj LbdWeighRead { get; set; }
        public WeighObj LbdWeighTakeOut { get; set; }
        // Khai báo cổng COM
        private SerialPort COM_Kern;
        private float oldWeighValue;
        private System.Timers.Timer wZeroTimer;
        private float[] valueArr;
        private string blanceStatus = "None";
        private float lowRangeValue = 5;
        private float highRangeValue = 10;
        private int countZero;
        private int countReceive = 0;
        private int countReceiveOld = 0;
        private int countErrorCOM = 0;
        private bool messageShow;
        private float limitValue;

        // Khai báo Event
        public delegate void BalanceCalReturn(float Value);
        public event BalanceCalReturn BalanceChangeValueEvent;
        #endregion

        /// <summary>
        /// Khởi tạo đối tượng Cân mỡ
        /// </summary>
        /// <param name="COMPort">Tên cổng Com dạng string</param>
        /// <param name="numberValue">Số lượng phần tử của mảng trích mẫu giá trị cân</param>
        /// <param name="limitWeigh">Khoảng thay đổi trả ra lớn nhất của cân</param>
        public BalanceObj(string COMPort, int numberValue, float limitWeigh)
        {
            // Khai báo đối tượng cân
            string tempStringCOM = COMPort;
            COM_Kern = new SerialPort(tempStringCOM, 9600, Parity.None, 8, StopBits.One);
            COM_Kern.DataReceived += ProcessWeighData;
            COM_Kern.Open();
            // Khai báo mảng lưu giá trị cân
            valueArr = new float[numberValue];
            // Khởi tạo giá trị cân tối đa
            limitValue = limitWeigh;
            // Khởi tạo đối tượng hiển thị cân
            LbdWeighRead = new WeighObj();
            LbdWeighTakeOut = new WeighObj();
            // Khởi tạo mảng lưu trữ mặc định
            for (int i = 0; i < valueArr.Length; i++)
            {
                valueArr[i] = 0;
            }
            // Timer kiểm tra trạng thái Stable 0 (không gửi dữ liệu về PC)
            wZeroTimer = new System.Timers.Timer(500);
            wZeroTimer.Elapsed += CheckWeighValue;
            wZeroTimer.Start();

        }

        /// <summary>
        /// Xử lý giá trị trả về từ cân mỡ
        /// Đẩy vào mảng lưu trích mẫu
        /// Xử lý nếu phát hiện cân đang ở trong trạng thái:
        /// 1. Mới nhấc túi mỡ ra (1)
        /// 2. Mới đặt túi mỡ vào (2)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ProcessWeighData(object sender, SerialDataReceivedEventArgs e)
        {
            countReceive += 1;
            string temp = COM_Kern.ReadLine();
            //Console.WriteLine("READ " + temp);
            if (temp.IndexOf("g") >= 0)
            {
                float wValue = 0;
                try
                {
                    wValue = float.Parse((temp.Substring(temp.LastIndexOf("  ") + 1)).Replace("g", ""));
                }
                catch { wValue = (float)-9.999; }
                LbdWeighRead.Weigh = wValue;
                PushToArray(valueArr, wValue);
                // Kiểm tra
                var tempCheck = CheckDataStatus();
                // Nếu trạng thái dữ liệu - Không có vật trên cân
                if (tempCheck == 1)
                {
                    CalculateTakeOut(1);
                }
                // Nếu trạng thái dữ liệu - Vừa đặt vật vào cân
                if (tempCheck == 2)
                {
                    CalculateTakeOut(2);
                }
            }
        }

        /// <summary>
        /// Kiểm tra trạng thái dữ liệu cân gửi về
        /// </summary>
        /// <returns></returns>
        private int CheckDataStatus()
        {
            if (valueArr.Average() < lowRangeValue)
            {
                if ((blanceStatus == "None") || (blanceStatus == "High"))
                {
                    blanceStatus = "Low";
                    return 1;
                }
            }
            // Giá trị lớn hơn khoảng giới hạn high, và giá trị chênh lệch giữa max-min < giá trị low
            if ((valueArr.Average() > highRangeValue) && ((valueArr.Max() - valueArr.Min()) < lowRangeValue))
            {
                if ((blanceStatus == "None") || (blanceStatus == "Low"))
                {
                    blanceStatus = "High";
                    return 2;
                }
            }

            return 0;
        }

        /// <summary>
        /// Thêm giá trị vào cuối mảng, đẩy giá trị đầu tiên ra
        /// </summary>
        /// <param name="valueArr"></param>
        /// <param name="wValue"></param>
        private void PushToArray(float[] valueArr, float wValue)
        {
            Array.ConstrainedCopy(valueArr, 1, valueArr, 0, valueArr.Length - 1);
            valueArr[valueArr.Length - 1] = wValue;
        }

        /// <summary>
        /// Nếu kiểm tra thấy túi mỡ vừa được nhấc ra, xử lý kiểm tra lượng mỡ
        /// </summary>
        private void CalculateTakeOut(int high)
        {
            switch (high)
            {
                case 1: // Kiểm tra vừa lấy vật ra
                    LbdWeighTakeOut.Weigh = 0;
                    Console.WriteLine("Giá trị cũ: " + oldWeighValue.ToString());
                    break;
                case 2: // Kiểm tra vừa đặt vật vào
                    if (oldWeighValue > lowRangeValue) LbdWeighTakeOut.Weigh = -valueArr.Average() + oldWeighValue;
                    oldWeighValue = LbdWeighRead.Weigh;
                    Console.WriteLine("Giá trị mới: " + oldWeighValue.ToString());
                    if (Math.Abs(LbdWeighTakeOut.Weigh) < limitValue)
                    {
                        //App.Current.Dispatcher.Invoke(() => WriteToKeyBoard());
                        if (BalanceChangeValueEvent != null) BalanceChangeValueEvent(LbdWeighTakeOut.Weigh);
                    }
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Ghi giá trị cân thay đổi ra bàn phím - không sử dụng
        /// </summary>
        private async void WriteToKeyBoard()
        {
            System.Windows.Clipboard.Clear();  // Always clear the clipboard first
            string stringWrite = LbdWeighTakeOut.Weigh.ToString("#.##");
            System.Windows.Clipboard.SetText(stringWrite);
            await Task.Delay(100);
            System.Windows.Forms.SendKeys.SendWait("^v");  // Paste
            //System.Windows.Forms.SendKeys.SendWait("{ENTER}");
            //Console.WriteLine("Confirm\n");
        }

        /// <summary>
        /// Kiểm tra data mỗi khi timer đạt 1s
        /// Bổ xung kiểm tra cổng COM
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckWeighValue(object sender, ElapsedEventArgs e)
        {
            if (valueArr[(valueArr.Length - 1)] < lowRangeValue) countZero += 1;
            else countZero = 0;
            if (countZero > 1)
            {
                countZero = 0;
                for (int i = 0; i < valueArr.Length; i++)
                {
                    if (valueArr[i] > lowRangeValue) valueArr[i] = 0;
                }
                if ((blanceStatus == "None") || (blanceStatus == "High"))
                {
                    blanceStatus = "Low";
                    CalculateTakeOut(1); // Nếu cân Stable ở trạng thái 0 trong 2s, thì xử lý giống với vừa lấy vật ra
                }
            }

            // Xử lý kiểm tra tín hiệu cổng COM
            if ((countReceive == countReceiveOld) && (!messageShow))
            {
                //Console.WriteLine("count");
                ErrorTimeOutCOM();
                if (countReceive > 10000) countReceive = 0;
                countReceiveOld = countReceive;
            }
        }

        /// <summary>
        /// Reset cổng COM khi gặp lỗi - Chưa hoàn thành
        /// </summary>
        private void ErrorTimeOutCOM()
        {
            countErrorCOM += 1;
            //if (countErrorCOM < 5) ReconnectCOM(ref COM_Kern);
            //else
            //{
            //    messageShow = true;
            //    MessageBoxResult temp = MessageBox.Show("Check COM Port and Click OK", "ERROR!!!", MessageBoxButton.OKCancel);
            //    if (temp == MessageBoxResult.OK) countErrorCOM = 0;
            //    if (temp == MessageBoxResult.Cancel) Dispatcher.Invoke(new Action(delegate { Application.Current.Shutdown(); }));
            //    messageShow = false;
            //}
        }

        private void ReconnectCOM(ref SerialPort cOM_Kern)
        {
            cOM_Kern.Dispose();
            cOM_Kern.Open();
        }

    }
}
