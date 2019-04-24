using System;
using System.IO.Ports;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Timers;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace SHIV_Data_Weigh
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private BalanceObj Balance;
        private TouqueManager Touques;

        public MainWindow()
        {
            InitializeComponent();
            ExcelInitial();
            DataInitial();
        }

        private void ExcelInitial()
        {
            //throw new NotImplementedException();
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
        /// Ghi giá trị cân ra ô focus bằng bàn phím
        /// </summary>
        /// <param name="balanceValue"></param>
        private async void WriteToKeyBoard(float balanceValue)
        {
            System.Windows.Clipboard.Clear();  // Always clear the clipboard first
            string stringWrite = balanceValue.ToString("#.##");
            System.Windows.Clipboard.SetText(stringWrite);
            await Task.Delay(100);
            System.Windows.Forms.SendKeys.SendWait("^v");  // Paste
            //System.Windows.Forms.SendKeys.SendWait("{ENTER}");
            //Console.WriteLine("Confirm\n");
        }

        private void LblWeighRead_MouseDown(object sender, MouseButtonEventArgs e)
        {
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
        }

        private void DataInitial()
        {
            // Khai báo thiết bị cân mỡ
            string tempStringCOM = Settings.Default.ComPort;
            Balance = new BalanceObj(tempStringCOM, 15, 100);
            Balance.BalanceChangeValueEvent += ProcessBalanceValueReturn;
            // Hiển thị đối tượng cân mỡ
            lblWeighRead.DataContext = Balance.LbdWeighRead;
            lblWeighTakeOut.DataContext = Balance.LbdWeighTakeOut;
            // Khai báo thiết bị súng lực 
            Touques = new TouqueManager();
            Touques.EventReceiveDataComplete += ProcessTouquesListData;
            lvDataBinding.DataContext = Touques.ListDataTouque;
            // Khởi tạo Data hiển thị List
        }

        private void ProcessBalanceValueReturn(float Value)
        {
            WriteToKeyBoard(Value);
        }

        private void ProcessTouquesListData(ObservableCollection<TouqueData> CurrentList)
        {
            Dispatcher.BeginInvoke(new Action(delegate
            {

            }));
            //MessageBox.Show("Hoàn thành bắn lực!");
        }
    }
}
