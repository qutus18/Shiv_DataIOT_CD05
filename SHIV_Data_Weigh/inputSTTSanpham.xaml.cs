using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SHIV_Data_Weigh
{
    /// <summary>
    /// Interaction logic for inputSTTSanpham.xaml
    /// </summary>
    public partial class inputSTTSanpham : Window
    {
        public delegate void outputBarcode(string bCode);
        public event outputBarcode STTSanphamChange;
        public event outputBarcode OrderChange;
        public event outputBarcode OperatorChange;
		public inputSTTSanpham()
        {
            InitializeComponent();
            FocusTextbox();
        }

        private void FocusTextbox()
        {
            txtInputBarcode.Focus();
        }

        private void txtInputBarcode_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtInputBarcode.Text.IndexOf((char)(13)) > 0)
            {
                if (txtInputBarcode.Text.IndexOf("-") > 0)
                {
                    if (STTSanphamChange != null) STTSanphamChange(txtInputBarcode.Text);
                    if (OrderChange != null) OrderChange(txtInputBarcode.Text);
                    if (OperatorChange != null) OperatorChange(txtInputBarcode.Text);
                    this.Close();
                }
                else
                {
                    if (txtInputBarcode.Text != "") MessageBox.Show("Wrong Input!");
                    txtInputBarcode.Text = "";
                }
            }
        }

        /// <summary>
        /// Kiểm tra trạng thái nút nhấn
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtInputBarcode_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if ((txtInputBarcode.Text.IndexOf("-") > 0) | (txtInputBarcode.Text.Length > 5))
                {
                    if (STTSanphamChange != null) STTSanphamChange(txtInputBarcode.Text);
                    if (OrderChange != null) OrderChange(txtInputBarcode.Text);
                    if (OperatorChange != null) OperatorChange(txtInputBarcode.Text);
					this.Close();
                }
                else
                {
                    if (txtInputBarcode.Text != "") MessageBox.Show("Wrong Input!");
                    txtInputBarcode.Text = "";
                }
            }
        }
    }
}
