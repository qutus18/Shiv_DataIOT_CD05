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
			try
			{
				COMSylvac.Open();
			}
			catch (Exception e)
			{
				System.Windows.MessageBox.Show($"Lỗi kết nối cổng COM {COMPort} - Mã lỗi: {e.ToString().Substring(0, 50)}");
				App.Current.Shutdown();
			}

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
                while (index < listValueSylvac.Count() - 5)
                {
                    /// Nếu chênh lệnh >0.10 thì đang bắt đầu vào đo then => nhảy cách 3 giá trị rồi bắt đầu lấy dữ liệu    
                    if (((listValueSylvac[index] - listValueSylvac[index - 1]) >= 0.050)&&(listValueSylvac[index + 5] > 0.15))
                    {
                        if (status != "U")
                        {
                            startCollect = index + 5;
                            status = "U";
                        }
                    }
                    /// Nếu chênh lệch nhỏ hơn -0.10 thì bắt đầu vào rãnh => xóa 3 vị trí bắt đầu từ 2 vị trí trước 
                    if ((listValueSylvac[index] - listValueSylvac[index - 1]) <= -0.050)
                    {
                        if (status != "D")
                        {
                            int lastIndex = listValueFilter.Count - 1;
                            if (lastIndex >= 2)
                            {
                                listValueFilter.RemoveRange(lastIndex - 4,3);
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
                Console.WriteLine("Read");
                for (int i = 0; i < listValueSylvac.Count; i++)
                {
                    Console.WriteLine(listValueSylvac[i]);
                }
                Console.WriteLine("----------------------");
                Console.WriteLine("Final");
                if (listValueFilter.Count > 3)
                {
                    lbdFinalValue.FloatValue = listValueFilter.Max() - listValueFilter.Min();
                    for (int i = 0; i < listValueFilter.Count(); i++)
                    {
                        Console.WriteLine(listValueFilter[i]);
                    }
                    //Console.WriteLine("--------------------MAX and MIN---------------CO RANH THEN------");
                    //Console.WriteLine(listValueFilter.Max());
                    //Console.WriteLine(listValueFilter.Min());
                }
                else
                {
                    if (listValueFilter.Count > 0 && listValueFilter.Count < 3)
                    {
                        lbdFinalValue.FloatValue = -1;
                    }
                    if (listValueFilter.Count == 0)
                    {
                        lbdFinalValue.FloatValue = listValueSylvac.Max() - listValueSylvac.Min();
                        //Console.WriteLine("--------------------MAX and MIN---------------KHONG CO RANH THEN------");
                        //Console.WriteLine(listValueSylvac.Max());
                        //Console.WriteLine(listValueSylvac.Min());
                    }
                }
                // Trả Event đầu ra
                if (SylvacChangeValueEvent != null) SylvacChangeValueEvent(lbdFinalValue.FloatValue);
            }
        }

        // Điều chỉnh 05/06 - Thêm giá trị 0 ở đầu để xử lý trường hợp không có then;
        public void Start()
        {
            listValueSylvac.Clear();
            listValueSylvac.Add(0);
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
