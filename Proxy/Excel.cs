
using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Proxy
{
    class Excel
    {
        private string fileName;
        Application ObjExcel;
        Workbook ObjWorkBook;


        private string urlPattern = "http(s)?://([\\w+?\\.\\w+])+([a-zA-Z0-9\\~\\!\\@\\#\\$\\%\\^\\&\\*\\(\\)_\\-\\=\\+\\\\\\/\\?\\.\\:\\;\\\'\\,]*)?";
        public Excel(string fileName)
        {
            this.fileName = fileName;
            //Создаём приложение.
            ObjExcel = new Application();
            //Открываем книгу.                                                                                                                                                        
            ObjWorkBook = ObjExcel.Workbooks.Open(fileName, 0, false, 5, "", "", false, XlPlatform.xlWindows, "", true, false, 0, true, false, false);            
        }
        ~Excel()
        {
            //Удаляем приложение (выходим из экселя) - ато будет висеть в процессах!
            ObjExcel.Quit();
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

                    //System.Windows.Forms.Application.DoEvents();
                }

            }
            catch { }
            return urls;
        }
    }
}