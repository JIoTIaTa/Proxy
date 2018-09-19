
using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AngleSharp.Css.Values;

namespace Proxy
{
    class Excel
    {
        Application ObjWorkApplication;
        Workbook ObjWorkBook;

        public event Action<object> BookLoaded;
        public event Action<object> BookClosed;

        private string urlPattern = "http(s)?://([\\w+?\\.\\w+])+([a-zA-Z0-9\\~\\!\\@\\#\\$\\%\\^\\&\\*\\(\\)_\\-\\=\\+\\\\\\/\\?\\.\\:\\;\\\'\\,]*)?";

        public Excel()
        {
            CreateExcelApplication();
        }
        public void LoadBook(string fileName)
        {
            //CloseExcelApplication();
            try
            {
                Task task = Task.Run(() =>
                {
                    //Открываем книгу.                                                                                                                                                        
                    ObjWorkBook = ObjWorkApplication.Workbooks.Open(fileName, 0, false, 5, "", "", false, XlPlatform.xlWindows, "", true, false, 0, true, false, false);
                });
                task.Wait();
                BookLoaded?.Invoke(this);
            }
            catch { }
        }
        
        ~Excel()
        {
            //Удаляем приложение (выходим из экселя) - а то будет висеть в процессах!
            CloseExcelApplication();
        }

        private void CreateExcelApplication()
        {
            //Создаём приложение.
            if (ObjWorkApplication == null)
            {
                ObjWorkApplication = new Application();
                //ObjWorkApplication.Visible = true;
            }
        }
        private void CloseExcelApplication()
        {
            if (ObjWorkApplication != null)
            {
                ObjWorkApplication.Workbooks.Close();
                BookClosed.Invoke(this);
                ObjWorkApplication.Quit();
            }
        }

        public void CloseCurrentBook()
        {
            try
            {
                ObjWorkBook.Save();
            }
            catch
            {
            }
            finally
            {
                ObjWorkBook.Close();
                BookClosed.Invoke(this);
            }

            
        }
        public List<string> Read(int rowsCount)
        {
            List<string> urls = new List<string>();
            
            Worksheet ObjWorkSheet;
            ObjWorkSheet = (Worksheet)ObjWorkBook.Sheets[1];

            try
            {

                for (int i = 1; i <= rowsCount; i++)
                {
                    string text = ((Range)(ObjWorkSheet.Cells[i, 1])).Text.ToString();

                    if (Regex.IsMatch(text, urlPattern))
                    {
                        urls.Add(text);
                    }
                    else
                    {
                        urls.Add(null);
                    }
                }
            }
            catch { }
            return urls;
        }

        public void  WriteLogs(List<string> logs)
        {
            Worksheet ObjWorkSheet;
            ObjWorkSheet = (Worksheet)ObjWorkBook.Sheets[1];

            try
            {
                int row = 1;
                foreach (var item in logs)
                {
                    if (item.Contains("NotFound"))
                    {
                        ObjWorkSheet.Cells[row, 1].Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.Red);
                    }
                    else if (item.Contains("OK"))
                    {
                        ObjWorkSheet.Cells[row, 1].Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.Green);
                    }
                    else if (item.Contains("No address"))
                    {
                        ObjWorkSheet.Cells[row, 1].Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.DeepSkyBlue);
                    }
                    else
                    {
                        ObjWorkSheet.Cells[row, 1].Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.Yellow);
                    }
                    row++;
                }

                try
                {
                    ObjWorkBook.Save();
                }
                catch { }
            }
            catch(Exception exception) { }
        }
    }
}