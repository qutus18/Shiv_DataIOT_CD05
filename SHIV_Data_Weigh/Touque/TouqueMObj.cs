using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Windows;
using System.IO;
using System.Threading;
using System.Timers;
using System.Windows.Threading;

namespace SHIV_Data_Weigh
{
	public class TouqueMObj : ObservableObject
	{
		private TcpClient SocketTCP;
		private Stream TouqueTCP;
		private Thread ReceiveTCPThread;
		public string JobID { get; set; }
		public bool ChangeJob { get; set; }
		private DispatcherTimer timerSentCommand;
		public delegate void ReceiveDelegate(string StringReceive);
		public event ReceiveDelegate ReceiveTCPEvent;

		/// <summary>
		/// Hàm khởi tạo 
		/// Mở kết nối tới cổng TCP
		/// Tạo Thread nhận dữ liệu từ cổng TCP
		/// Khai báo timer 1s để gửi lệnh duy trì kết nối
		/// </summary>
		/// <param name="ipAddress"></param>
		/// <param name="port"></param>
		public TouqueMObj(string ipAddress, int port)
		{
			SocketTCP = new TcpClient();
			//SocketTCP.Connect(ipAddress, port);
			try
			{
				var result = SocketTCP.BeginConnect(ipAddress, port, null, null);
				var success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(1));
				if (!success)
				{
					MessageBox.Show("Không kết nối được đến thiết bị: " + ipAddress + " - Port: " + port.ToString());
					App.Current.Shutdown();
				}
				SocketTCP.EndConnect(result);
			}
			catch (Exception e)
			{
				MessageBox.Show("Lỗi kết nối đến thiết bị súng lực với mã lỗi: " + e.ToString());
				App.Current.Shutdown();
			}
			if (SocketTCP.Connected)
			{
				Console.WriteLine("Khởi tạo kết nối - IP :" + ipAddress + " - Port: " + port.ToString());
				TouqueTCP = SocketTCP.GetStream();
				ReceiveTCPThread = new Thread(ReceiveDataTCP);
				ReceiveTCPThread.IsBackground = true;
				ReceiveTCPThread.Start();
				timerSentCommand = new DispatcherTimer();
				timerSentCommand.Tick += ProcessTimer;
				timerSentCommand.Interval = new TimeSpan(0, 0, 1);
				timerSentCommand.Start();
			}
			else
			{
				//MessageBox.Show("Không kết nối được đến thiết bị: " + ipAddress + " - Port: " + port.ToString());
			}

		}

		/// <summary>
		/// Thread nhận dữ liệu về từ cổng TCP, chu kỳ 100ms
		/// Điều kiện chuỗi ký tự trả về có chứa '0'
		/// </summary>
		private void ReceiveDataTCP()
		{
			SentOverTCP("00200001            \00"); // 0200001 là lệnh mở truyền thông với thiết bị súng lực
			byte[] data;
			while (true)
			{
				//Thread.Sleep(100);
				data = new byte[1024];
				TouqueTCP.Read(data, 0, data.Length);
				string tempStringReceive = System.Text.Encoding.UTF8.GetString(data).Replace("\r", "").Replace("\n", "").Replace("\r\n", "").Replace("?", "").Replace("\0", "");
				// Kiểm tra có phải là chuối trả giá trị súng lực về không?
				if ((tempStringReceive.IndexOf("0061") >= 0))
				{
					ProcessTCPReceiveData(tempStringReceive);
				}
			}
		}

		/// <summary>
		/// Gửi liên tục lệnh duy trì kết nối
		/// Option gửi lệnh chuyển Job
		/// </summary>
		/// <param name="connect"></param>
		/// <param name="changeJob"></param>
		public async void CycleCommand()
		{
			SentOverTCP("00209999            \00"); // 0209999 là lệnh duy trì kết nối với súng lực
			await Task.Delay(100);
			SentOverTCP("00200060            \00"); // 0209999 là lệnh duy trì kết nối với súng lực
			await Task.Delay(100);
			if (ChangeJob)
			{
				SentOverTCP("");
				ChangeJob = false;
			}
		}

		/// <summary>
		/// Hàm hỗ trợ gửi string v qua TCP
		/// </summary>
		/// <param name="v"></param>
		private void SentOverTCP(string v)
		{
			byte[] tempSent = Encoding.ASCII.GetBytes(v);
			TouqueTCP.Write(tempSent, 0, tempSent.Length);
			TouqueTCP.Flush();
		}

		/// <summary>
		/// Kiểm tra chuỗi nhận về
		/// Nếu là giá trị lực thì gửi qua Event
		/// </summary>
		private void ProcessTCPReceiveData(string tempStringReceive)
		{
			Console.WriteLine(tempStringReceive);
			// Bổ xung thêm điều kiện kiểm tra OK/NG (09110 => OK)
			if ((tempStringReceive.IndexOf("0061") >= 0) && ((tempStringReceive.IndexOf("09110") >= 0)))
			{
				Console.WriteLine("Nhận tín hiệu đo lực gửi về! " + tempStringReceive);
				if (ReceiveTCPEvent != null) ReceiveTCPEvent(tempStringReceive);
			}
		}

		/// <summary>
		/// Xử lý gửi nhận Command theo timer 1s
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ProcessTimer(object sender, EventArgs e)
		{
			CycleCommand();
		}

		/// <summary>
		/// Hàm xử lý chuyển mã Job của súng lực
		/// Gửi vào mã Job => Đổi mã súng lực sau 1s qua hàm CycleCommand
		/// </summary>
		/// <param name="inputJobID"></param>
		public void ChangeJobMethod(string inputJobID)
		{
			JobID = inputJobID;
			ChangeJob = true;
		}
	}
}
