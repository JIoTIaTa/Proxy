
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

        private string urlPattern = "http(s)?://([\\w+?\\.\\w+])+([a-zA-Z0-9\\~\\!\\@\\#\\$\\%\\^\\&\\*\\(\\)_\\-\\=\\+\\\\\\/\\?\\.\\:\\;\\\'\\,]*)?";
        public Excel()
        {
            //Создаём приложение.
            ObjWorkApplication = new Application();
        }
        public void LoadBook(string fileName)
        {
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
            ObjWorkApplication.Quit();
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
                ObjWorkBook.Save();
                ObjWorkApplication.Save();

            }
            catch(Exception exception) { }
        }
    }
}