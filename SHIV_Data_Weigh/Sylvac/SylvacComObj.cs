using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Ports;

namespace SHIV_Data_Weigh
{
    public class SylvacComObj
    {
        private SerialPort COMSylvac;
        private string inputFloatSum;
        private string inputTemp;
        private List<float> listValueSylvac = new List<float>();
        private bool isRunning;
        // Khai báo đối tượng hiển thị
        public SylvacObj lbdCurrentValue { get; set; }
        public SylvacObj lbdFinalValue { get; set; }
        // Khai báo Event
        public delegate void SylvacCalReturn(float Value);
        public event SylvacCalReturn SylvacChangeValueEvent;

        public SylvacComObj(string COMPort)
        {
            // Khởi tạo cổng COM
            COMSylvac = new SerialPort(COMPort, 4800, Parity.Even, 7, StopBits.Two);
            COMSylvac.DataReceived += COMReceiveProcess;
            COMSylvac.Open();
            WriteDefault();
            // Khởi tạo dữ liệu đọc
            inputFloatSum = "";
            inputTemp = "";
            // Khởi tạo list dữ liệu 
            listValueSylvac.Clear();
            // Khởi tạo giá trị ban đầu
            lbdCurrentValue = new SylvacObj();
            lbdFinalValue = new SylvacObj();
            // 
            isRunning = false;
        }

        private void WriteDefault()
        {
            COMSylvac.Write("MM\r");
            COMSylvac.Write("RES2\r");
            COMSylvac.Write("FACT1\r");
            COMSylvac.Write("NOR\r");
            COMSylvac.Write("OUT1\r");
        }

        private void COMReceiveProcess(object sender, SerialDataReceivedEventArgs e)
        {
            // Đọc dữ liệu hiện tại về 
            inputTemp = COMSylvac.ReadExisting();
            if (inputTemp.Length > 7) inputTemp = inputTemp.Substring(0, 7);
            //Console.WriteLine(inputTemp);
            // Kiểm tra chuỗi nhận về 
            if ((inputTemp.Contains("+")) || inputTemp.Contains("-"))
            {
                // Xử lý nếu chuyển sang giá trị float mới
                ProcessFloatInput();
                // Bắt đầu đọc giá trị float mới
                inputFloatSum = inputTemp;
            }
            else inputFloatSum += inputTemp;
        }

        private void ProcessFloatInput()
        {
            if (inputFloatSum == "") return;
            float tempFloat = -1;
            if (float.TryParse(inputFloatSum, out tempFloat))
            {
                if (isRunning)
                {
                    //if (tempFloat < 0.1) Console.WriteLine(tempFloat.ToString("#.##"));
                    listValueSylvac.Add(tempFloat);
                    lbdCurrentValue.FloatValue = tempFloat;
                }
            }
            else return;
        }

        private void CalFinalValue()
        {
            if (listValueSylvac.Count() < 10) return;
            else
            {
                List<float> listValueFilter = new List<float>();
                int index = 1;
                int startCollect = listValueSylvac.Count();
                string status = "N";
                // Filter chênh lệch >0.05
                while (index < listValueSylvac.Count() - 3)
                {
                    /// Nếu chênh lệnh >0.05 thì đang bắt đầu vào đo then => nhảy cách 3 giá trị rồi bắt đầu lấy dữ liệu 
                    /// Nếu chênh lệch nhỏ hơn -0.05 thì bắt đầu vào rãnh => xóa 2 vị trí trước 
                    if (((listValueSylvac[index] - listValueSylvac[index - 1]) >= 0.25)&&(listValueSylvac[index + 3] > 0.15))
                    {
                        if (status != "U")
                        {
                            startCollect = index + 3;
                            status = "U";
                        }
                    }
                    if ((listValueSylvac[index] - listValueSylvac[index - 1]) <= -0.15)
                    {
                        if (status != "D")
                        {
                            int lastIndex = listValueFilter.Count - 1;
                            if (lastIndex > 1)
                            {
                                listValueFilter.RemoveRange(lastIndex - 2, 2);
                            }
                            else listValueFilter.Clear();
                            startCollect = listValueSylvac.Count();
                            status = "D";
                        }
                    }
                    if (index >= startCollect) listValueFilter.Add(listValueSylvac[index]);
                    index += 1;
                }
                // Return Final Value
                for (int i = 0; i < listValueSylvac.Count; i++)
                {
                    Console.WriteLine(listValueSylvac[i]);
                }
                Console.WriteLine("----------------------");
                Console.WriteLine("----------------------");
                for (int i = 0; i < listValueFilter.Count; i++)
                {
                    Console.WriteLine(listValueFilter[i]);
                }
                if (listValueFilter.Count > 5)
                    lbdFinalValue.FloatValue = listValueFilter.Max() - listValueFilter.Min();
                else lbdFinalValue.FloatValue = -1;
                // Trả Event đầu ra
                if (SylvacChangeValueEvent != null) SylvacChangeValueEvent(lbdFinalValue.FloatValue);
            }
        }

        public void Start()
        {
            isRunning = true;
        }

        public void Stop()
        {
            isRunning = false;
            CalFinalValue();
        }

        public void Close()
        {
            COMSylvac.DataReceived -= COMReceiveProcess;
            COMSylvac.Close();
        }
    }
}
