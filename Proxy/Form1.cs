using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.IO;
using System.Net;
using Ninject;
using Proxy.GoogleDriveAPI;
using Proxy.Ninject;
using Proxy.Parser;
using Proxy.Parser.Facebook;

namespace Proxy
{
    public partial class Form1 : Form
    {
        ProxyServer proxy;
        List<string> urls;
        Excel excel;
        private string urlRegex = "http(s)?://([\\w+?\\.\\w+])+([a-zA-Z0-9\\~\\!\\@\\#\\$\\%\\^\\&\\*\\(\\)_\\-\\=\\+\\\\\\/\\?\\.\\:\\;\\\'\\,]*)?";
        private string ipRegex = @"(?<First>2[0-4]\d|25[0-5]|[01]?\d\d?)\.(?<Second>2[0-4]\d|25[0-5]|[01]?\d\d?)\.(?<Third>2[0-4]\d|25[0-5]|[01]?\d\d?)\.(?<Fourth>2[0-4]\d|25[0-5]|[01]?\d\d?)";
        List<string> logs;
        WebRequestLoader webPage;
        SerialazebleParametrs _serialazebleParametrs;
        private ParserWorker<string[]> parser;
        private GDrive gDrive;
        private string gDriveFileId = null;
        private Action<string> openNewExcelBook;
        private Action<bool> appInProcessing;
        /// <summary>
        /// Загальний параметр номеру посилання для відправки через таймер
        /// </summary>
        private int urlNumberToSend = 0;

        private IKernel parserDiNinjectKernel;



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
            if (Serializator.Read(_serialazebleParametrs, formSettingsFullPath) != null)
            {
                _serialazebleParametrs = Serializator.Read(_serialazebleParametrs, formSettingsFullPath);
            }
            else
            {
                _serialazebleParametrs = new SerialazebleParametrs("FileName", "C:\\file", 100, 30 * 60000, null, 10*1000, "213.141.129.97", 47881, "khorok", "khorok412");
            }


            #endregion
            textBox_IP.Text = proxy.IPAddress;
            numericUpDown_port.Value = proxy.Port;
            textBox_login.Text = proxy.Login;
            textBox_password.Text = proxy.Password;
            urls = new List<string>();
            logs = new List<string>();
            webPage = new WebRequestLoader();
            webPage.NewLog += WebPage_NewLog;
            excel = new Excel();
            excel.BookLoaded += Excel_BookLoaded;
            excel.BookClosed += ExcelOnBookClosed;
            openNewExcelBook += OpenNewExcelBook;
            appInProcessing += AppInProcessing;

            //temp
            textBox_fileName.Text = _serialazebleParametrs.FileUrl1;
            textBox_filePath.Text = _serialazebleParametrs.LocalPath;
            numericUpDown_Rows.Value = _serialazebleParametrs.RowsCount;
            numericUpDown_tickValue.Value = _serialazebleParametrs.AllReqeustsTimeInterval / 60000;
            numericUpDown_requestInterval.Value = _serialazebleParametrs.OneRequestTimeInterval / 1000;

            try
            {
                string client_secretPath = Path.Combine(userDocumentsPath, "Proxy Master", "client_secret.json");
                gDrive = new GDrive(client_secretPath);
                gDrive.ErrorMessage += GDrive_ErrorMessage;
                gDrive.DownloadProgres += DownloadCompleted;
                gDrive.UpdateCompleted += GDriveOnUpdateCompleted;
                toolStripStatusLabel1.Text = "Давай пробовать загрузить файл";
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
            //Конфіги впровадження залежностей для парсера
            parserDiNinjectKernel = new StandardKernel(new ParserNjConfig(proxy.IPAddress, proxy.Port, proxy.Login, proxy.Password));
        }

        private void AppInProcessing(bool obj)
        {
            button_start.Enabled = !obj;
            button1.Enabled = !obj;
            button2.Enabled = !obj;
            textBox_IP.Enabled = !obj;
            numericUpDown_port.Enabled = !obj;
            textBox_login.Enabled = !obj;
            textBox_password.Enabled = !obj;
            textBox_fileName.Enabled = !obj;
        }

        private void GDriveOnUpdateCompleted(string fileId)
        {
            string userDocumentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            string localFileName = Path.Combine(userDocumentsPath, "Proxy Master", textBox_fileName.Text);
            gDrive.DownloadFileFromDrive(fileId, localFileName);
        }

        private void ExcelOnBookClosed(object obj)
        {
            button_start.Enabled = false;
        }

        private void OpenNewExcelBook(string obj)
        {
            toolStripStatusLabel1.Text = "Подгружаем локальный файл";
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
            if (timer_response.Interval > 1)
            {
                timer_response.Interval = 1;
            }
            timer_response.Start();
        }

        //Івент винонання переходу по посиланню
        private void WebPage_NewLog(object arg1, string newLog)
        {
            logs.Add(newLog);
            if (newLog.Contains("NotFound"))
            {
                string message = $"{newLog.Replace("NotFound", "забанен")}";
                message = $"Шеф, все пропало\n Клиент {newLog.Substring(25)}";
                notifyIcon1.BalloonTipText = message;
                notifyIcon1.ShowBalloonTip(1000);
            }
            toolStripProgressBar1.Value++;
            toolStripStatusLabel1.Text = $"Отправлено {toolStripProgressBar1.Value} из {toolStripProgressBar1.Maximum}";
            if (toolStripProgressBar1.Value == toolStripProgressBar1.Maximum)
            {
                finishTask();
            }
        }

        private void finishTask()
        {
            appInProcessing.Invoke(false);
            urlNumberToSend = 0;
            string message = $"Следующий заход в {nextConnect()}";
            toolStripStatusLabel1.Text = message;
            notifyIcon1.BalloonTipText = message;
            notifyIcon1.ShowBalloonTip(1000);
            writeLogsToTxt(); // записать в .txt
            writeLogsToExcel(); // записать в xlsx
            uploadTogDrive();
            logs.Clear();
            toolStripStatusLabel1.Text = message;
        }

        private void uploadTogDrive()
        {
            excel.CloseCurrentBook();
            if (gDriveFileId != null) // заміним файл, якщо його прогружали і залишився ID
            {
                gDrive.UpdateFileAtDrive(gDriveFileId, textBox_filePath.Text, ContentType.spreadsheet);
            }
            else
            {
                gDrive.UploadFileToDrive(textBox_fileName.Text, textBox_filePath.Text, ContentType.spreadsheet);
            }
        }

        //Запис логів в .txt
        private void writeLogsToTxt()
        {

            string userDocumentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            string localFileName = Path.Combine(userDocumentsPath, "Proxy Master", @"Logs.txt");
            using (StreamWriter sw = new StreamWriter(localFileName, true, Encoding.Default))
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
            string fullMin = "";
            fullMin = min % 10 < 1 ? $"0{min}" : min.ToString();
            string resultTime = $"{hour}:{fullMin}";
            return resultTime;
        }


        private void button1_Click_1(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "Excel Files(.xls)|*.xls|Excel Files(.xlsx)| *.xlsx";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                textBox_filePath.Text = openFileDialog1.FileName;
                openNewExcelBook.Invoke(openFileDialog1.FileName);
            }
        }


        private void timer_response_Tick(object sender, EventArgs e)
        {
            timer_response.Interval = (int)numericUpDown_tickValue.Value * 60000;
            StartRequest();            
        }

        private void StartRequest()
        {
            appInProcessing.Invoke(true);
            toolStripStatusLabel1.Text = "Шерстим таблицу";
            var proxyParameters = proxy.Create();
            try
            {
                urls = excel.Read((int) numericUpDown_Rows.Value);
            }
            catch (Exception e)
            {

            }
            finally
            {
                toolStripProgressBar1.Maximum = urls.Count();
                toolStripProgressBar1.Value = 0;
            }
            if (proxyParameters != null)
            {
                toolStripStatusLabel1.Text = "Начиаем спам";
                #region Перехід на посилання через WebRequestLoader
                //WebRequestLoader webPage = new WebRequestLoader(proxyParameters);
                //webPage.NewLog += WebPage_NewLog;
                //foreach (var item in urls)
                //{
                //    webPage.Connect(item);
                //}
                #endregion

                #region Перехід на посилання через ParserWorker
                //parser = new ParserWorker<string[]>(new FacebookParser(), proxyParameters);
                parser = parserDiNinjectKernel.Get<ParserWorker<string[]>>();
                parser.OnNewRequestResult += WebPage_NewLog;
                //parser.Start(urls); // для найшвидшого проходу по всім посиланням
                urlNumberToSend = 0;
                timer_OneResponse.Start();
                #endregion
            }
            else
            {
                MessageBox.Show("Не удалось подключится к Прокси.\nПроверьте параметры или соединение");
                appInProcessing.Invoke(false);
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

            _serialazebleParametrs.FileUrl1 = textBox_fileName.Text;
            _serialazebleParametrs.LocalPath = textBox_filePath.Text;
            _serialazebleParametrs.RowsCount = (int)numericUpDown_Rows.Value;
            _serialazebleParametrs.AllReqeustsTimeInterval = (int)numericUpDown_tickValue.Value * 60000;
            _serialazebleParametrs.gDriveFileId = gDriveFileId;
            _serialazebleParametrs.OneRequestTimeInterval = (int)numericUpDown_requestInterval.Value * 1000;
            string formSettingsFileName = "parameters.dat";
            string formSettingsFullPath = Path.Combine(userDocumentsPath, "Proxy Master", formSettingsFileName);

            Serializator.Write(_serialazebleParametrs, formSettingsFullPath);
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
                notifyIcon1.BalloonTipText = "Я пока поработаю здесь";
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
                    fileName = $"{fileName}.xlsx";
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

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            timer_OneResponse.Interval = (int)numericUpDown_requestInterval.Value * 1000;
        }

        private void timer_requestIntervval_Tick(object sender, EventArgs e)
        {
            timer_OneResponse.Interval = (int)numericUpDown_requestInterval.Value * 1000;
           
            parser.Start(urls[urlNumberToSend]);
            if (urlNumberToSend >= urls.Count-1)
            {
                timer_OneResponse.Stop();
                urlNumberToSend = 0;
            }
            else
            {
                urlNumberToSend++;
            }
        }
    }
}
