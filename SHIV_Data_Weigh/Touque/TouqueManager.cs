using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace SHIV_Data_Weigh
{
    public class TouqueManager
    {
        public delegate void ListDataSentOut(ObservableCollection<TouqueData> CurrentList);
        public event ListDataSentOut EventReceiveDataComplete;
        public TouqueMObj Touque_001, Touque_002;
        public ObservableCollection<TouqueData> ListDataTouque = new ObservableCollection<TouqueData>();
        public TouqueJobInfomation JobInfo = new TouqueJobInfomation(1, Settings.Default.TouqueEnd);

        /// <summary>
        /// Khai báo,
        /// Khởi tạo 2 đối tượng súng lực
        /// </summary>
        public TouqueManager()
        {
            // Get IP data from Setting
            string ipOne = Settings.Default.TouqueIP_001;
            string ipTwo = Settings.Default.TouqueIP_002;
            int portOne = Settings.Default.TouquePort_001;
            int portTwo = Settings.Default.TouquePort_002;
            //
            int touqueUsed = Settings.Default.TouqueSelect;
            //
            if (touqueUsed == 1)
            {
                Touque_001 = new TouqueMObj(ipOne, portOne);
                Touque_001.ReceiveTCPEvent += ProcessTouqueData;
            }
            if (touqueUsed == 2)
            {
                Touque_002 = new TouqueMObj(ipTwo, portTwo);
                Touque_002.ReceiveTCPEvent += ProcessTouqueData;
            }
            //MessageBox.Show("Connect!");
            // Khai báo Job
        }

        /// <summary>
        /// Xử lý dữ liệu nhận về từ 2 thiết bị cân lực
        /// Tự động đẩy dữ liệu vào List Data
        /// Gửi Event hoàn thành khi hoàn tất 
        /// Reset List Data khi bắt đầu lấy dữ liệu lại
        /// </summary>
        /// <param name="StringReceive"></param>
        private void ProcessTouqueData(string StringReceive)
        {
            // Kiểm tra chuỗi nhận về, lấy giá trị cân lực, cho vào list
            var temp = GetDataTouque(StringReceive);
            if (CheckJobBegin(temp.Header))
                App.Current.Dispatcher.BeginInvoke(new Action(delegate
                {
                    ListDataTouque.Clear();
                }));
            if (temp.Header != "null")
            {
                App.Current.Dispatcher.BeginInvoke(new Action(delegate
                {
                    ListDataTouque.Add(temp);
                }));
            }
            if (CheckJobEnd(temp.Header))
            {
                if (EventReceiveDataComplete != null)
                    EventReceiveDataComplete(ListDataTouque);

            }
            // Kiểm tra dữ liệu đạt ngưỡng gửi ra => gửi dữ liệu ra Excel
            Application.Current.Dispatcher.BeginInvoke(new Action(delegate
            {
                CheckDataAndSentExcel(temp);
            }));

        }

        private async void CheckDataAndSentExcel(TouqueData temp)
        {
            var limitValue = Settings.Default.TouqueStartCollect;
            int currentValue = int.Parse(temp.Header.Substring(temp.Header.IndexOf("C") + 1));
            if (currentValue >= limitValue)
            {
                string tempSent = temp.Value.ToString("0.00");
                System.Windows.Clipboard.Clear();  // Always clear the clipboard first
                System.Windows.Clipboard.SetText(tempSent);
                await Task.Delay(100);
                System.Windows.Forms.SendKeys.SendWait("^v");  // Paste
                //System.Windows.Forms.SendKeys.SendWait("{ENTER}");
            }
        }

        /// <summary>
        /// Kiểm tra bắt đầu ID = 1, Count = 1
        /// </summary>
        /// <param name="header"></param>
        /// <returns></returns>
        private bool CheckJobBegin(string header)
        {
            int ID;
            int EndC;
            try
            {
                ID = int.Parse(header.Substring(0, 2));
            }
            catch { ID = -1; }
            try
            {
                EndC = int.Parse(header.Substring(4));
            }
            catch
            {
                EndC = -1;
            }
            if ((ID == 1) && (EndC == 1))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Kiểm tra kết thúc ID = NumberJobPart, Count = finishNumber
        /// </summary>
        /// <param name="header"></param>
        /// <returns></returns>
        private bool CheckJobEnd(string header)
        {
            int ID;
            int EndC;
            try
            {
                ID = int.Parse(header.Substring(0, 2));
            }
            catch { ID = -1; }
            try
            {
                EndC = int.Parse(header.Substring(4));
            }
            catch
            {
                EndC = -1;
            }
            if ((ID == JobInfo.numberOfJobPart) && (EndC == JobInfo.finishNumber))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Lấy giá trị Header và Value từ chuỗi trả về từ súng lực
        /// 02310061            010001020103LOC#1                    04VN00112345123451234512345050106005070003080003091101111120005301300061014000570150005731600000170005018000201900015202019-04-03:11:49:37212019-03-04:15:03:2222123
        /// </summary>
        /// <param name="stringReceive"></param>
        /// <returns></returns>
        private TouqueData GetDataTouque(string stringReceive)
        {
            float valueReturn;
            string headerReturn;
            // GetData
            var tempGet = stringReceive;
            while (true)
            {
                if (tempGet.IndexOf("15") >= 0)
                {
                    tempGet = tempGet.Substring(tempGet.IndexOf("15"));
                    if (tempGet[9] == '6') break;
                    tempGet = tempGet.Substring(2);
                    Console.WriteLine("try " + tempGet);
                }
                else
                {
                    valueReturn = 0;
                    break;
                }
            }
            valueReturn = float.Parse(tempGet.Substring(2, 6)) / 100;
            // GetHeader
            tempGet = stringReceive;
            while (true)
            {
                if (tempGet.IndexOf("05") >= 0)
                {
                    tempGet = stringReceive.Substring(tempGet.IndexOf("05"));
                    if ((tempGet[5] == '6') && (tempGet[10] == '7')) break;
                    tempGet = tempGet.Substring(2);
                }
                else
                {
                    headerReturn = "null";
                    break;
                }
            }
            headerReturn = tempGet.Substring(tempGet.IndexOf("05") + 2, 2) + "C" + tempGet.Substring(17, 4);
            return new TouqueData(headerReturn, valueReturn);
        }

        /// <summary>
        /// Hàm đổi JOB trên súng lực, dựa theo thứ tự súng, và ID của JOB gửi vào
        /// </summary>
        /// <param name="TouqueIndex"></param>
        /// <param name="JobID"></param>
        public void ChangeJob(int TouqueIndex, string JobID)
        {
            if ((Touque_001 != null) && (Touque_002 != null))
            {
                switch (TouqueIndex)
                {
                    case (1):
                        Touque_001.JobID = JobID;
                        Touque_001.ChangeJob = true;
                        break;
                    case (2):
                        Touque_002.JobID = JobID;
                        Touque_002.ChangeJob = true;
                        break;
                    default:
                        break;
                }
                // Cập nhật JobInfo
                JobInfo = GetJobInfo(JobID);
            }
            else
            {
                MessageBox.Show("Không có đối tượng súng lực!");
            }
        }

        private TouqueJobInfomation GetJobInfo(string jobID)
        {
            return new TouqueJobInfomation(2, 2);
        }
    }
}
