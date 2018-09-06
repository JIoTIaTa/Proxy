using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Proxy
{
    public partial class Form1 : Form
    {
        ProxyServer proxy;
        List<string> urls;
        Excel excel;
        public Form1()
        {           
            InitializeComponent();
            proxy = new ProxyServer("213.141.129.97", 47881, "khorok", "khorok412");
            textBox_IP.Text = proxy.IPAddress;
            numericUpDown_port.Value = proxy.Port;
            textBox_login.Text = proxy.Login;
            textBox_password.Text = proxy.Password;
            urls = new List<string>();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            proxy.IPAddress = textBox_IP.Text;
            proxy.Port = (int)numericUpDown_port.Value;
            proxy.Login = textBox_login.Text;
            proxy.Password = textBox_password.Text;
            proxy.Create();
        }

        private void button_start_Click(object sender, EventArgs e)
        {
            var proxyParameters = proxy.Create();
            string url = "https://www.facebook.com/profile.php?id=100007026078185";
            if (excel != null)
            {
                urls = excel.Read((int)numericUpDown_Rows.Value);
                toolStripProgressBar1.Maximum = urls.Count();
                toolStripProgressBar1.Value = 0;
            }
            else
            {
                MessageBox.Show("Файл не выбран");
            }
            if (proxyParameters != null)
            {
                WebPage webPage = new WebPage(proxyParameters);
                int progress = 1;
                foreach (var item in urls)
                {
                    webPage.Connect(item);
                    toolStripProgressBar1.Value = progress;
                    progress++;
                }
            }
            else
            {
                MessageBox.Show("Не удалось подключится к Прокси.\nПроверьте параметры или соединение");
            }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                textBox_filePath.Text = openFileDialog1.FileName;
                excel = new Excel(openFileDialog1.FileName);
                button_start.Enabled = true;
            }
        }
    }
}
