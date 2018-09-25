using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.IO;
using System.Net;
using Microsoft.Office.Core;
using Ninject;
using Proxy.BookWorker;
using Proxy.GoogleDriveAPI;
using Proxy.Ninject;
using Proxy.Parser;
using Proxy.Parser.Facebook;

namespace Proxy
{
    public partial class Form1 : Form
    {
        List<string> urls;
        IBookWorker bookWorker;
        private string ipRegex = @"(?<First>2[0-4]\d|25[0-5]|[01]?\d\d?)\.(?<Second>2[0-4]\d|25[0-5]|[01]?\d\d?)\.(?<Third>2[0-4]\d|25[0-5]|[01]?\d\d?)\.(?<Fourth>2[0-4]\d|25[0-5]|[01]?\d\d?)";
        List<string> logs;
        SerialazebleParametrs parametrs;
        private ParserWorker<string[]> parser;
        private GDrive gDrive;
        private Action<string> openNewExcelBook;
        private Action<bool> AppInProcessing;
        /// <summary>
        /// Загальний параметр номеру посилання для відправки через таймер
        /// </summary>
        private int urlNumberToSend = 0;
        private readonly string userDocumentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);

        private IKernel parserDIKernel;
             



        public Form1(SerialazebleParametrs serialazebleParametrs, IBookWorker bookWorker, GDrive gDrive)
        {
            InitializeComponent();

            this.parametrs = serialazebleParametrs ?? throw new ArgumentException("serialazebleParametrs load ERROR");
            AppInProcessing += appInProcessing;

            this.bookWorker = bookWorker ?? throw new ArgumentException("bookWorker load ERROR");
            this.bookWorker.BookLoaded += BookWorkerBookLoaded;
            this.bookWorker.BookClosed += BookWorkerOnBookClosed;
            openNewExcelBook += OpenNewExcelBook;

            this.gDrive = gDrive ?? throw new ArgumentException("gDrive load ERROR");
            gDrive.ErrorMessage += GDrive_ErrorMessage;
            gDrive.DownloadProgres += DownloadCompleted;
            gDrive.UpdateCompleted += GDriveOnUpdateCompleted;

            //Конфіги впровадження залежностей для парсера
            parserDIKernel = new StandardKernel(new ParserNjConfig(parametrs.IPAddress, parametrs.Port, parametrs.Login, parametrs.Password));

            #region Встановення параметрів форми за десеріалізованими даними

            string userDocumentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            textBox_IP.Text = parametrs.IPAddress;
            numericUpDown_port.Value = parametrs.Port;
            textBox_login.Text = parametrs.Login;
            textBox_password.Text = parametrs.Password;

            textBox_fileName.Text = parametrs.FileUrl1;
            textBox_filePath.Text = parametrs.LocalPath;
            numericUpDown_Rows.Value = parametrs.RowsCount;
            numericUpDown_tickValue.Value = parametrs.AllReqeustsTimeInterval / 60000;
            numericUpDown_requestInterval.Value = parametrs.OneRequestTimeInterval / 1000;
            #endregion

            urls = new List<string>();
            logs = new List<string>();
            
            
        }

        private void appInProcessing(bool obj)
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

        private void BookWorkerOnBookClosed(object obj)
        {
            button_start.Enabled = false;
        }

        private void OpenNewExcelBook(string obj)
        {
            toolStripStatusLabel1.Text = "Подгружаем локальный файл";
            bookWorker.LoadBook(obj);
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
        private void BookWorkerBookLoaded(object obj)
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
                calculatePassResult();
            }
        }

        private void calculatePassResult()
        {
            AppInProcessing.Invoke(false);
            urlNumberToSend = 0;
            string message = $"Следующий заход в {calculateNextConnect()}";
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
            bookWorker.CloseBook();
            if (parametrs.gDriveFileId != null) // заміним файл, якщо його прогружали і залишився ID
            {
                gDrive.UpdateFileAtDrive(parametrs.gDriveFileId, textBox_filePath.Text, ContentType.spreadsheet);
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
        //Правка книги bookWorker по логам
        private void writeLogsToExcel()
        {
            bookWorker.Write(logs);
        }
        //Прорахунок наступного проходу
        private string calculateNextConnect()
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
            openFileDialog1.Filter = "ExcelBookWorker Files(.xls)|*.xls|ExcelBookWorker Files(.xlsx)| *.xlsx";
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
            AppInProcessing.Invoke(true);
            toolStripStatusLabel1.Text = "Шерстим таблицу";
            try
            {
                //urls = bookWorker.Read((int) numericUpDown_Rows.Value);
                urls = bookWorker.Read();
            }
            catch (Exception e) { toolStripStatusLabel1.Text = e.Message; }
            finally
            {
                toolStripProgressBar1.Maximum = urls.Count();
                toolStripProgressBar1.Value = 0;
            }
            toolStripStatusLabel1.Text = "Начиаем спам";
            #region Перехід на посилання через ParserWorker
            parser = parserDIKernel.Get<ParserWorker<string[]>>();
            parser.OnNewRequestResult += WebPage_NewLog;
            //parser.Start(urls); // для найшвидшого проходу по всім посиланням
            urlNumberToSend = 0;
            timer_OneResponse.Start();
            #endregion
        }

        #region Події зміни регуляторів на формі
        private void numericUpDown_tickValue_ValueChanged(object sender, EventArgs e)
        {
            timer_response.Interval = (int)(numericUpDown_tickValue.Value * 60000);
        }
        private void textBox_IP_TextChanged(object sender, EventArgs e)
        {
            if (Regex.IsMatch(textBox_IP.Text, ipRegex))
            {
                textBox_IP.ForeColor = Color.Green;
                parametrs.IPAddress = textBox_IP.Text;
            }
            else
            {
                textBox_IP.ForeColor = Color.Red;
            }
        }
        private void numericUpDown_port_ValueChanged(object sender, EventArgs e)
        {
            parametrs.Port = (int)numericUpDown_port.Value;
        }
        private void textBox_login_TextChanged(object sender, EventArgs e)
        {
            parametrs.Login = textBox_login.Text;
        }
        private void textBox_password_TextChanged(object sender, EventArgs e)
        {
            parametrs.Password = textBox_password.Text;
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            parametrs.IPAddress = textBox_IP.Text;
            parametrs.Port = (int)numericUpDown_port.Value;
            parametrs.Login = textBox_login.Text;
            parametrs.Password = textBox_password.Text;

            parametrs.FileUrl1 = textBox_fileName.Text;
            parametrs.LocalPath = textBox_filePath.Text;
            parametrs.RowsCount = (int)numericUpDown_Rows.Value;
            parametrs.AllReqeustsTimeInterval = (int)numericUpDown_tickValue.Value * 60000;
            parametrs.OneRequestTimeInterval = (int)numericUpDown_requestInterval.Value * 1000;

            string formSettingsFileName = "parameters.dat";
            string formSettingsFullPath = Path.Combine(userDocumentsPath, "Proxy Master", formSettingsFileName);

            Serializator.Write(parametrs, formSettingsFullPath);
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
        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            timer_OneResponse.Interval = (int)numericUpDown_requestInterval.Value * 1000;
        }
        #endregion

        private void loadGDriveFile(string fileName)
        {
                toolStripStatusLabel1.Text = "Поиск файла на gDrive...";
                Dictionary<string, string> files = gDrive.GetUserFiles();
                string fileId = null;
                if (files.TryGetValue(fileName, out fileId))
                {
                    //fileName = $"{fileName}.xlsx";
                    textBox_fileName.ForeColor = Color.Green;
                    toolStripStatusLabel1.Text = "Загрузка файла с gDrive...";
                    parametrs.gDriveFileId = fileId;
                    try
                    {
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
