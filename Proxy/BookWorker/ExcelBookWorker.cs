﻿
using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Proxy.BookWorker;

namespace Proxy
{
    class ExcelBookWorker : IBookWorker
    {
        Application ObjWorkApplication;
        Workbook ObjWorkBook;

        public event Action<object> BookLoaded;
        public event Action<object> BookClosed;
        public event Action<object, string> ErrorMessage;


        public ExcelBookWorker()
        {
            CreateExcelApplication();
        }

        public void CreateApplication()
        {
            throw new NotImplementedException();
        }

        public void CloseApplication()
        {
            throw new NotImplementedException();
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

        public List<string> Read()
        {
            throw new NotImplementedException();
        }

        public Dictionary<string, string> ReadWithCellsReference()
        {
            throw new NotImplementedException();
        }

        public void Write()
        {
            throw new NotImplementedException();
        }

        public void Write(List<string> rows)
        {
            throw new NotImplementedException();
        }

        ~ExcelBookWorker()
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

        public void CloseBook()
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
                    
                    urls.Add(text);
                }
            }
            catch { }
            return urls;
        }
        /// <summary>
        /// Зафарбовка кольорами в залежнсоті від тексту
        /// </summary>
        /// <param name="rows"></param>
        public void EdidCellColorByValue(Dictionary<string, string> rows)
        {
            Worksheet ObjWorkSheet;
            ObjWorkSheet = (Worksheet)ObjWorkBook.Sheets[1];

            try
            {
                int row = 1;
                foreach (var item in rows.Values)
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
            catch { }
        }

        public void Write(Dictionary<string, string> valueCellDictionary)
        {
            throw new NotImplementedException();
        }

        public void SetCellBorderStyle(Dictionary<string, string> cellText)
        {
            throw new NotImplementedException();
        }
    }
}