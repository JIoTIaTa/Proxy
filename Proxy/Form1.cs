using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.IO;
using System.Runtime.CompilerServices;
using Google.Apis.Download;
using Proxy.GoogleDriveAPI;
using Proxy.Parser;
using Proxy.Parser.Facebook;

namespace Proxy
{
    public partial class Form1 : Form
    {
        ProxyServer proxy;
        List<string> urls;
        Excel excel;
        //WebFile webFile;
        private string urlRegex = "http(s)?://([\\w+?\\.\\w+])+([a-zA-Z0-9\\~\\!\\@\\#\\$\\%\\^\\&\\*\\(\\)_\\-\\=\\+\\\\\\/\\?\\.\\:\\;\\\'\\,]*)?";
        private string ipRegex = @"(?<First>2[0-4]\d|25[0-5]|[01]?\d\d?)\.(?<Second>2[0-4]\d|25[0-5]|[01]?\d\d?)\.(?<Third>2[0-4]\d|25[0-5]|[01]?\d\d?)\.(?<Fourth>2[0-4]\d|25[0-5]|[01]?\d\d?)";
        List<string> logs;
        WebRequest webPage;
        FileParametrs fileParametrs;
        private ParserWorker<string[]> parser;
        private GDrive gDrive;
        private string gDriveFileId = null;
        private Action<string> openNewExcelBook;


        public Form1()
        {           
            InitializeComponent();

            #region Сериализация

            string userDocumentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            string proxySettingsFileName = "proxy.dat";
            string proxySettingsFullPath = Path.Combine(userDocumentsPath, "Proxy Master", proxySettingsFileName);
            if (Serializator.Read(proxy, proxySettingsFullPath) != null)
            {
                proxy = Serializator.Read(proxy, proxySettingsFullPath);
            }
            else
            {
                proxy = new ProxyServer("213.141.129.97", 47881, "khorok", "khorok412");
            }
            string formSettingsFileName = "parameters.dat";
            string formSettingsFullPath = Path.Combine(userDocumentsPath, "Proxy Master", formSettingsFileName);
            if (Serializator.Read(fileParametrs, formSettingsFullPath) != null)
            {
                fileParametrs = Serializator.Read(fileParametrs, formSettingsFullPath);
            }
            else
            {
                fileParametrs = new FileParametrs("FileName", "C:\\file", 100, 30 * 60000, null);
            }


            #endregion
            textBox_IP.Text = proxy.IPAddress;
            numericUpDown_port.Value = proxy.Port;
            textBox_login.Text = proxy.Login;
            textBox_password.Text = proxy.Password;
            urls = new List<string>();
            //webFile = new WebFile();
            logs = new List<string>();
            webPage = new WebRequest();
            webPage.NewLog += WebPage_NewLog;
            excel = new Excel();
            excel.BookLoaded += Excel_BookLoaded;
            excel.BookClosed += ExcelOnBookClosed;
            openNewExcelBook += OpenNewExcelBook;

            //temp
            textBox_fileName.Text = fileParametrs.FileUrl;
            textBox_filePath.Text = fileParametrs.LocalPath;
            numericUpDown_Rows.Value = fileParametrs.RowsCount;
            numericUpDown_tickValue.Value = fileParametrs.TimerInterval / 60000;

            try
            {
                gDrive = new GDrive();
                gDrive.ErrorMessage += GDrive_ErrorMessage;
                gDrive.DownloadProgres += DownloadCompleted;
                gDrive.UploadCompleted += GDriveOnUploadCompleted;
            }
            catch (Exception exception)
            {
                if (exception.Message == "Object reference not set to an instance of an object.")
                {
                    MessageBox.Show("Не удалось соединиться со службой gDrive.\nПроверьте наличие файла client_secret.json в User/Documents/ProxyMaster и перезапутстите приложение","Ошибка авторизации gDrive");
                }
                else
                {
                    MessageBox.Show(exception.Message);
                }
                tabControl1.SelectedIndex = 1;
                tabControl1.TabPages[0].Enabled = false;
            }
        }

        private void GDriveOnUploadCompleted(string fileId, string fileName)
        {
            string userDocumentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            string localFileName = Path.Combine(userDocumentsPath, "Proxy Master", fileName);
            gDrive.DownloadFileFromDrive(fileId, localFileName);
            openNewExcelBook.Invoke(localFileName);
            //loadGDriveFile(textBox_fileName.Text); // відкрить апку і загрузить ще раз новий файл
        }

        private void ExcelOnBookClosed(object obj)
        {
            button_start.Enabled = false;
        }

        private void OpenNewExcelBook(string obj)
        {
            toolStripStatusLabel1.Text = "Подгружаем локальный файл";
            //excel.CreateExcelApplication();
            excel.LoadBook(obj);
        }

        //Івент завантаження книги з gDrive
        private void DownloadCompleted(string filePath)
        {
            textBox_filePath.Text = filePath;
            openNewExcelBook.Invoke(filePath);
        }

        private void GDrive_ErrorMessage(string obj)
        {
            toolStripStatusLabel1.Text = obj;
        }

        //Івент, коли книга відкрилась
        private void Excel_BookLoaded(object obj)
        {
            button_start.Enabled = true;
            toolStripStatusLabel1.Text = "Готов к запуску";
        }

        private void button_start_Click(object sender, EventArgs e)
        {
            timer_response.Enabled = true;
        }

        //Івент винонання переходу по посиланню
        private void WebPage_NewLog(object arg1, string arg2)
        {
            logs.Add(arg2);
            toolStripProgressBar1.Value++;
            toolStripStatusLabel1.Text = $"Отправлено {toolStripProgressBar1.Value} из {toolStripProgressBar1.Maximum}";
            if (toolStripProgressBar1.Value == toolStripProgressBar1.Maximum)
            {
                string message = $"Следующий заход в {nextConnect()}";
                toolStripStatusLabel1.Text = message;
                notifyIcon1.BalloonTipText = message;
                notifyIcon1.ShowBalloonTip(1000);
                writeLogs(); // записать в .txt
                writeLogsToExcel(); // записать в xlsx
                uploadTogDrive();
            }
        }

        private void uploadTogDrive()
        {
            //if (gDriveFileId != null) // заміним файл, якщо його прогружали і залишився ID
            //{
            //    gDrive.UploadFileToDrive(textBox_fileName.Text, gDriveFileId, textBox_filePath.Text, ContentType.spreadsheet);
            //}
            //else
            //{
            //excel.CloseExcelApplication(); // відпустить апку excel
            excel.CloseCurrentBook();
            gDrive.UploadFileToDrive(textBox_fileName.Text, textBox_filePath.Text, ContentType.spreadsheet);
            //}
        }

        //Запис логів в .txt
        private void writeLogs()
        {
            
            using (StreamWriter sw = new StreamWriter(@"Logs.txt", true, Encoding.Default))
            {                
                var time = DateTime.Now;
                sw.WriteLine($"Дата: {time.Day}.{time.Month}.{time.Year} | Время: {time.Hour}:{time.Minute}" + Environment.NewLine);
                foreach (var item in logs)
                {
                    sw.WriteLine(item + Environment.NewLine);
                }
            }
        }
        //Правка книги excel по логам
        private void writeLogsToExcel()
        {
            excel.WriteLogs(logs);
        }
        //Прорахунок наступного проходу
        private string nextConnect()
        {
            var time = DateTime.Now;
            int minInterval = timer_response.Interval / 60000;
            int dayMinuteTime = time.Hour * 60 + time.Minute;
            int nextConnect = dayMinuteTime + minInterval;
            int hour = nextConnect / 60;
            int min = nextConnect % 60;
            string resultTime = $"{hour}:{min}";
            return resultTime;
        }


        private void button1_Click_1(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                textBox_filePath.Text = openFileDialog1.FileName;
                openNewExcelBook.Invoke(openFileDialog1.FileName);
            }
        }

        private void textBox_fileUrl_TextChanged(object sender, EventArgs e)
        {
            if (!textBox_fileName.Text.Contains(".xls") || !textBox_fileName.Text.Contains(".xlsx"))
            {
                textBox_fileName.ForeColor = Color.Orange;
                toolStripStatusLabel1.Text = "Неправильное название Excel файла";
            }
            else
            {
                textBox_fileName.ForeColor = Color.Blue;
                toolStripStatusLabel1.Text = "Жми Загрузить!";
            }
        }

        private void timer_response_Tick(object sender, EventArgs e)
        {
            timer_response.Interval = 30 * 60000;
            Start();            
        }

        private void textBox_filePath_TextChanged(object sender, EventArgs e)
        {
            if (textBox_filePath.Text.Contains(".xlsx") || textBox_filePath.Text.Contains(".xls"))
            {
                
            }
        }
        private void Start()
        {
            toolStripStatusLabel1.Text = "Шерстим таблицу";
            var proxyParameters = proxy.Create();
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
                toolStripStatusLabel1.Text = "Начиаем спам";
                #region Перехід на посилання через WebRequest
                //WebRequest webPage = new WebRequest(proxyParameters);
                //webPage.NewLog += WebPage_NewLog;
                //foreach (var item in urls)
                //{
                //    webPage.Connect(item);
                //}
                #endregion

                #region Перехід на посилання через ParserWorker
                parser = new ParserWorker<string[]>(new FacebookParser(), proxyParameters);
                parser.OnNewRequestResult += WebPage_NewLog;
                parser.Start(urls);
                #endregion
            }
            else
            {
                MessageBox.Show("Не удалось подключится к Прокси.\nПроверьте параметры или соединение");
            }
        }

        private void numericUpDown_tickValue_ValueChanged(object sender, EventArgs e)
        {
            timer_response.Interval = (int)(numericUpDown_tickValue.Value * 60000);
        }

        private void textBox_IP_TextChanged(object sender, EventArgs e)
        {
            if (Regex.IsMatch(textBox_IP.Text, ipRegex))
            {
                textBox_IP.ForeColor = Color.Green;
                proxy.IPAddress = textBox_IP.Text;
            }
            else
            {
                textBox_IP.ForeColor = Color.Red;
            }
        }

        private void numericUpDown_port_ValueChanged(object sender, EventArgs e)
        {
            proxy.Port = (int)numericUpDown_port.Value;
        }

        private void textBox_login_TextChanged(object sender, EventArgs e)
        {
            proxy.Login = textBox_login.Text;
        }

        private void textBox_password_TextChanged(object sender, EventArgs e)
        {
            proxy.Password = textBox_password.Text;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            string userDocumentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            string proxySettingsFileName = "proxy.dat";
            string proxySettingsFullPath = Path.Combine(userDocumentsPath, "Proxy Master", proxySettingsFileName);

            Serializator.Write(proxy, proxySettingsFullPath);

            fileParametrs.FileUrl = textBox_fileName.Text;
            fileParametrs.LocalPath = textBox_filePath.Text;
            fileParametrs.RowsCount = (int)numericUpDown_Rows.Value;
            fileParametrs.TimerInterval = (int)numericUpDown_tickValue.Value * 60000;
            fileParametrs.gDriveFileId = gDriveFileId;
            string formSettingsFileName = "parameters.dat";
            string formSettingsFullPath = Path.Combine(userDocumentsPath, "Proxy Master", formSettingsFileName);

            Serializator.Write(fileParametrs, formSettingsFullPath);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            notifyIcon1.BalloonTipTitle = "Proxy Master";
            notifyIcon1.Text = "Proxy Master";
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.Show();
            notifyIcon1.Visible = false;
            WindowState = FormWindowState.Normal;
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                this.Hide();
                notifyIcon1.BalloonTipText = "Приложение свернуто";
                notifyIcon1.Visible = true;
                notifyIcon1.ShowBalloonTip(1000);
            }
            else if (WindowState == FormWindowState.Normal)
            {
                notifyIcon1.Visible = false;
            }
        }

        private void ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }


        private void button2_Click(object sender, EventArgs e)
        {
            loadGDriveFile(textBox_fileName.Text);
        }

        private void loadGDriveFile(string fileName)
        {
                toolStripStatusLabel1.Text = "Поиск файла на gDrive...";
                Dictionary<string, string> files = gDrive.GetUserFiles();
                string fileId = null;
                if (files.TryGetValue(fileName, out fileId))
                {
                    textBox_fileName.ForeColor = Color.Green;
                    toolStripStatusLabel1.Text = "Загрузка файла с gDrive...";
                    gDriveFileId = fileId;
                    try
                    {
                    //string localFileName = $"{Environment.CurrentDirectory.ToString()}\\{fileName}";

                        string userDocumentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                        string localFileName = Path.Combine(userDocumentsPath, "Proxy Master", fileName);
                        gDrive.DownloadFileFromDrive(fileId, localFileName);
                    }
                    catch (Exception e)
                    {
                        toolStripStatusLabel1.Text = e.Message;
                    }
                }
                else
                {
                textBox_fileName.ForeColor = Color.Red;
                toolStripStatusLabel1.Text = $"{fileName} не найден";
                }
            
        }
    }
}
