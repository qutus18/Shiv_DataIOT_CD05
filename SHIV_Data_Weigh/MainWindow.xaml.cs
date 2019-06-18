using System;
using System.IO.Ports;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Timers;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Media;

namespace SHIV_Data_Weigh
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private BalanceObj Balance;
        private TouqueManager Touques;
        private SylvacComObj Sylvac;
        private Sylvac.USRIOT UsrDevice = new Sylvac.USRIOT();
        private bool isStart;

        public MainWindow()
        {
			LampWD tempLamp = new LampWD();
			tempLamp.Left = 0;
			tempLamp.Top = 0;
			tempLamp.Show();
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
            this.Dispatcher.Invoke(new Action(delegate
            {
                System.Windows.Clipboard.Clear();  // Always clear the clipboard first
                string stringWrite = balanceValue.ToString("###.##");
                System.Windows.Clipboard.SetText(stringWrite);
            }));
            await Task.Delay(100);
            this.Dispatcher.Invoke(new Action(delegate
            {
                System.Windows.Forms.SendKeys.SendWait("^v");
                //System.Windows.Forms.SendKeys.SendWait("{ENTER}");
            }));
            //Console.WriteLine("Confirm\n");
        }

        private void LblWeighRead_MouseDown(object sender, MouseButtonEventArgs e)
        {
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                Sylvac.Close();

            }
            catch
            { }
        }

        private void DataInitial()
        {
            var useSylvac = Settings.Default.UseSylvac;
            var comSylvac = Settings.Default.ComSylvac;
            var useBalance = Settings.Default.UseBalance;
            var useTouque = Settings.Default.UseTouque;
            var comBalance = Settings.Default.ComPort;
            // địa chỉ IP wifi kết nối
            UsrDevice.Address = Settings.Default.USR424IP;
            if (useBalance)
            {
                // Khai báo thiết bị cân mỡ
                Balance = new BalanceObj(comBalance, 15, 100, false);
                Balance.BalanceChangeValueEvent += ProcessBalanceValueReturn;
                // Hiển thị đối tượng cân mỡ
                lblWeighRead.DataContext = Balance.LbdWeighRead;
                lblWeighTakeOut.DataContext = Balance.LbdWeighTakeOut;
            }
            if (useTouque)
            {
                // Khai báo thiết bị súng lực 
                Touques = new TouqueManager();
                Touques.EventReceiveDataComplete += ProcessTouquesListData;
                lvDataBinding.DataContext = Touques.ListDataTouque;
            }
            if (useSylvac)
            {
                // Khai báo thiết bị Sylvac
                Sylvac = new SylvacComObj("COM12");
                Sylvac.SylvacChangeValueEvent += ProcessSylvacEvent;
                lblSylvacInput.DataContext = Sylvac.lbdCurrentValue;
                lblSylvacFinal.DataContext = Sylvac.lbdFinalValue;

            }
            //kết nối với USR-IO424T
            //UsrDevice.ConnectTCP();
            // Event Start and Stop value Sylvac
            UsrDevice.OnchangeIOevent += UsrDeviceInputChangeProcess;

        }

        private void UsrDeviceInputChangeProcess(string info)
        {
            switch (info)
            {
                case "Input1":
                    if (!isStart)
                    {
                        isStart = true;
                        //lblSylvacInput.Foreground = Brushes.Red;
                        Sylvac.Start();
                    }
                    break;
                case "Input2":
                    if (isStart)
                    {
                        isStart = false;
                        //lblSylvacInput.Foreground = Brushes.Black;
                        Sylvac.Stop();
                    }
                    else
                    {
                        if (Balance != null) Balance.CalculateTakeOutManual();
                    }
                    break;
                default:
                    break;
            }
        }

        private async void ProcessSylvacEvent(float Value)
        {
            this.Dispatcher.Invoke(new Action(delegate
            {
                System.Windows.Clipboard.Clear();  // Always clear the clipboard first
                string stringWrite = (Value * 1000).ToString("###.##");
                System.Windows.Clipboard.SetText(stringWrite);
            }));
            await Task.Delay(100);
            this.Dispatcher.Invoke(new Action(delegate
            {
                System.Windows.Forms.SendKeys.SendWait("^v");
            }));
        }

        private void ProcessBalanceValueReturn(float Value)
        {
            WriteToKeyBoard((float)Value);
        }

        private void ProcessTouquesListData(ObservableCollection<TouqueData> CurrentList)
        {
            Dispatcher.BeginInvoke(new Action(delegate
            {

            }));
            //MessageBox.Show("Hoàn thành bắn lực!");
        }

        private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            switch ((e.Command as RoutedUICommand).Name)
            {
                case "Exit":
                    this.Close();
                    break;
                case "StartSylvac":
                    Sylvac.Start();
                    break;
                case "StopSylvac":
                    Sylvac.Stop();
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Nút nhấn lấy dữ liệu cân bằng tay
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EventF1Push_Process(object sender, ExecutedRoutedEventArgs e)
        {
            Balance.CalculateTakeOutManual();
        }

        private void MenuItem_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void MenuItemStart_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!isStart)
            {
                isStart = true;
                //lblSylvacInput.Foreground = Brushes.Red;
                Sylvac.Start();
            }
        }

        private void MenuItemStop_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (isStart)
            {
                isStart = false;
                //lblSylvacInput.Foreground = Brushes.Black;
                Sylvac.Stop();
            }
        }
    }


}
