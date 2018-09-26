using System;
using System.Collections.Generic;
using System.Linq;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace Proxy.BookWorker
{
    class OpenXmlWorker: IBookWorker

    {
    private string fileName;

    public event Action<object> BookLoaded;
    public event Action<object> BookClosed;
    public event Action<object, string> ErrorMessage;


    public void LoadBook(string fileName)
    {
        this.fileName = fileName;
        try
        {
            using (SpreadsheetDocument spreadsheetDocument = SpreadsheetDocument.Open(fileName, false)) {}
            if (BookLoaded != null) BookLoaded.Invoke(this);
            }
        catch (Exception exception)
        {
            if (ErrorMessage != null) ErrorMessage.Invoke(this, exception.Message);
        }
        
    }

    public void CloseBook()
    {
        if (BookClosed != null) BookClosed.Invoke(this);
    }

    public List<string> Read(int rowsCount)
    {
        throw new NotImplementedException();
    }

    public List<string> Read()
    {
        return ReadExcelFileDOM(this.fileName);
    }

    public Dictionary<string, string> ReadWithCellsReference()
    {
        return ReadExcelFileWithCellsreference(this.fileName);
    }

    public void Write()
    {
        throw new NotImplementedException();
    }

    public void Write(List<string> rows)
    {
        throw new NotImplementedException();
    }

    public void Write(Dictionary<string, string> valueCellDictionary)
    {
        throw new NotImplementedException();
    }
    private List<string> ReadExcelFileDOM(string fileName)
    {
        List<string> resultList = new List<string>();
        using (SpreadsheetDocument spreadsheetDocument = SpreadsheetDocument.Open(fileName, false))
        {
            WorkbookPart workbookPart = spreadsheetDocument.WorkbookPart;
            WorksheetPart worksheetPart = workbookPart.WorksheetParts.First();
            SheetData sheetData = worksheetPart.Worksheet.Elements<SheetData>().First();
            string text;

            foreach (Row r in sheetData.Elements<Row>())
            {
                foreach (Cell c in r.Elements<Cell>())
                {
                    if (c.CellValue != null)
                    {
                        text = GetCellValue(fileName, 1, c.CellReference);
                        resultList.Add(text);
                    }
                }
            }
        }

        return resultList;
    }

    private Dictionary<string, string> ReadExcelFileWithCellsreference(string fileName)
    {
        Dictionary<string, string> result = new Dictionary<string, string>();
        using (SpreadsheetDocument spreadsheetDocument = SpreadsheetDocument.Open(fileName, false))
        {
            WorkbookPart workbookPart = spreadsheetDocument.WorkbookPart;
            WorksheetPart worksheetPart = workbookPart.WorksheetParts.First();
            SheetData sheetData = worksheetPart.Worksheet.Elements<SheetData>().First();
            string value;
            string cellRef;
            foreach (Row r in sheetData.Elements<Row>())
            {
                foreach (Cell c in r.Elements<Cell>())
                {
                    if (c.CellValue != null)
                    {
                        value = GetCellValue(fileName, 1, c.CellReference);
                        cellRef = c.CellReference;
                        if (!result.ContainsKey(cellRef))
                        {
                            result.Add(cellRef, value);
                        }
                    }
                }
            }
        }

        return result;
    }

    public string GetCellValue(string fileName, string sheetName, string addressName)
    {
        string value = null;

        // Open the spreadsheet document for read-only access.
        using (SpreadsheetDocument document =
            SpreadsheetDocument.Open(fileName, false))
        {
            // Retrieve a reference to the workbook part.
            WorkbookPart wbPart = document.WorkbookPart;

            // Find the sheet with the supplied name, and then use that 
            // Sheet object to retrieve a reference to the first worksheet.
            Sheet theSheet = wbPart.Workbook.Descendants<Sheet>().Where(s => s.Name == sheetName).FirstOrDefault();

            // Throw an exception if there is no sheet.
            if (theSheet == null)
            {
                throw new ArgumentException("sheetName");
            }

            // Retrieve a reference to the worksheet part.
            WorksheetPart wsPart =
                (WorksheetPart) (wbPart.GetPartById(theSheet.Id));

            // Use its Worksheet property to get a reference to the cell 
            // whose address matches the address you supplied.
            Cell theCell = wsPart.Worksheet.Descendants<Cell>().Where(c => c.CellReference == addressName)
                .FirstOrDefault();

            // If the cell does not exist, return an empty string.
            if (theCell != null)
            {
                value = theCell.InnerText;

                // If the cell represents an integer number, you are done. 
                // For dates, this code returns the serialized value that 
                // represents the date. The code handles strings and 
                // Booleans individually. For shared strings, the code 
                // looks up the corresponding value in the shared string 
                // table. For Booleans, the code converts the value into 
                // the words TRUE or FALSE.
                if (theCell.DataType != null)
                {
                    switch (theCell.DataType.Value)
                    {
                        case CellValues.SharedString:

                            // For shared strings, look up the value in the
                            // shared strings table.
                            var stringTable =
                                wbPart.GetPartsOfType<SharedStringTablePart>()
                                    .FirstOrDefault();

                            // If the shared string table is missing, something 
                            // is wrong. Return the index that is in
                            // the cell. Otherwise, look up the correct text in 
                            // the table.
                            if (stringTable != null)
                            {
                                value =
                                    stringTable.SharedStringTable
                                        .ElementAt(int.Parse(value)).InnerText;
                            }

                            break;

                        case CellValues.Boolean:
                            switch (value)
                            {
                                case "0":
                                    value = "FALSE";
                                    break;
                                default:
                                    value = "TRUE";
                                    break;
                            }

                            break;
                    }
                }
            }
        }

        return value;
    }

    public string GetCellValue(string fileName, int sheetId, string addressName)
    {
        string value = null;

        // Open the spreadsheet document for read-only access.
        using (SpreadsheetDocument document =
            SpreadsheetDocument.Open(fileName, false))
        {
            // Retrieve a reference to the workbook part.
            WorkbookPart wbPart = document.WorkbookPart;

            // Find the sheet with the supplied name, and then use that 
            // Sheet object to retrieve a reference to the first worksheet.
            Sheet theSheet = wbPart.Workbook.Descendants<Sheet>().Where(s => s.SheetId == sheetId).FirstOrDefault();

            // Throw an exception if there is no sheet.
            if (theSheet == null)
            {
                throw new ArgumentException("SheetId");
            }

            // Retrieve a reference to the worksheet part.
            WorksheetPart wsPart =
                (WorksheetPart) (wbPart.GetPartById(theSheet.Id));

            // Use its Worksheet property to get a reference to the cell 
            // whose address matches the address you supplied.
            Cell theCell = wsPart.Worksheet.Descendants<Cell>().Where(c => c.CellReference == addressName)
                .FirstOrDefault();

            // If the cell does not exist, return an empty string.
            if (theCell != null)
            {
                value = theCell.InnerText;

                // If the cell represents an integer number, you are done. 
                // For dates, this code returns the serialized value that 
                // represents the date. The code handles strings and 
                // Booleans individually. For shared strings, the code 
                // looks up the corresponding value in the shared string 
                // table. For Booleans, the code converts the value into 
                // the words TRUE or FALSE.
                if (theCell.DataType != null)
                {
                    switch (theCell.DataType.Value)
                    {
                        case CellValues.SharedString:

                            // For shared strings, look up the value in the
                            // shared strings table.
                            var stringTable =
                                wbPart.GetPartsOfType<SharedStringTablePart>()
                                    .FirstOrDefault();

                            // If the shared string table is missing, something 
                            // is wrong. Return the index that is in
                            // the cell. Otherwise, look up the correct text in 
                            // the table.
                            if (stringTable != null)
                            {
                                value =
                                    stringTable.SharedStringTable
                                        .ElementAt(int.Parse(value)).InnerText;
                            }

                            break;

                        case CellValues.Boolean:
                            switch (value)
                            {
                                case "0":
                                    value = "FALSE";
                                    break;
                                default:
                                    value = "TRUE";
                                    break;
                            }

                            break;
                    }
                }
            }
        }

        return value;
    }

    public void EdidCellColorByValue(Dictionary<string, string> cellrefTextDictionary)
    {
        Dictionary<string, System.Drawing.Color> cellrefColorsDictionary =
            new Dictionary<string, System.Drawing.Color>();
        foreach (var item in cellrefTextDictionary)
        {
            if (item.Value.Contains("NotFound"))
            {
                cellrefColorsDictionary.Add(item.Key, System.Drawing.Color.Red);
            }
            else if (item.Value.Contains("OK"))
            {
                cellrefColorsDictionary.Add(item.Key, System.Drawing.Color.Green);
            }
            else if (item.Value.Contains("No address"))
            {
                cellrefColorsDictionary.Add(item.Key, System.Drawing.Color.White);
            }
            else
            {
                cellrefColorsDictionary.Add(item.Key, System.Drawing.Color.Yellow);
            }
        }
        SetCellBorderStyle(cellrefColorsDictionary);
    }

    private void SetCellBorderStyle(Dictionary<string, System.Drawing.Color> cells)
    {
        using (SpreadsheetDocument spreadsheetDocument = SpreadsheetDocument.Open(this.fileName, true))
        {
            WorkbookPart workbookPart = spreadsheetDocument.WorkbookPart;
            WorksheetPart worksheetPart = workbookPart.WorksheetParts.First();

            foreach (var item in cells)
            {
                Cell cell = GetCell(worksheetPart, item.Key);

                CellFormat cellFormat = cell.StyleIndex != null
                    ? GetCellFormat(workbookPart, cell.StyleIndex).CloneNode(true) as CellFormat
                    : new CellFormat();
                cellFormat.FillId = InsertFill(workbookPart,
                    GenerateFill(HexConverter(item.Value), ColorToUInt(item.Value)));
                cellFormat.BorderId = InsertBorder(workbookPart, GenerateBorder(System.Drawing.Color.Black));
                cell.StyleIndex = InsertCellFormat(workbookPart, cellFormat);
            }
        }
    }

    public Fill GenerateFill(string foreGroundHexColor, uint backgroundUint32Color)
    {
        Fill fill = new Fill();

        PatternFill patternFill = new PatternFill() {PatternType = PatternValues.Solid};
        ForegroundColor foregroundColor1 = new ForegroundColor() {Rgb = foreGroundHexColor};
        BackgroundColor backgroundColor1 = new BackgroundColor() {Indexed = (UInt32Value) 64U};

        patternFill.Append(foregroundColor1);
        patternFill.Append(backgroundColor1);

        fill.Append(patternFill);

        return fill;
    }

    public Border GenerateBorder(System.Drawing.Color color)
    {
        Border border2 = new Border();

        LeftBorder leftBorder2 = new LeftBorder() {Style = BorderStyleValues.Thin};
        Color color1 = new Color() {Indexed = (UInt32Value) ColorToUInt(color)};

        leftBorder2.Append(color1);

        RightBorder rightBorder2 = new RightBorder() {Style = BorderStyleValues.Thin};
        Color color2 = new Color() {Indexed = (UInt32Value) ColorToUInt(color)};

        rightBorder2.Append(color2);

        TopBorder topBorder2 = new TopBorder() {Style = BorderStyleValues.Thin};
        Color color3 = new Color() {Indexed = (UInt32Value) ColorToUInt(color)};

        topBorder2.Append(color3);

        BottomBorder bottomBorder2 = new BottomBorder() {Style = BorderStyleValues.Thin};
        Color color4 = new Color() {Indexed = (UInt32Value) ColorToUInt(color)};

        bottomBorder2.Append(color4);
        DiagonalBorder diagonalBorder2 = new DiagonalBorder();

        border2.Append(leftBorder2);
        border2.Append(rightBorder2);
        border2.Append(topBorder2);
        border2.Append(bottomBorder2);
        border2.Append(diagonalBorder2);

        return border2;
    }

    public uint InsertBorder(WorkbookPart workbookPart, Border border)
    {
        Borders borders = workbookPart.WorkbookStylesPart.Stylesheet.Elements<Borders>().First();
        borders.Append(border);
        return (uint) borders.Count++;
    }

    public uint InsertFill(WorkbookPart workbookPart, Fill fill)
    {
        Fills fills = workbookPart.WorkbookStylesPart.Stylesheet.Elements<Fills>().First();
        fills.Append(fill);
        return (uint) fills.Count++;
    }

    public Cell GetCell(WorksheetPart workSheetPart, string cellAddress)
    {
        return workSheetPart.Worksheet.Descendants<Cell>()
            .SingleOrDefault(c => cellAddress.Equals(c.CellReference));
    }

    public CellFormat GetCellFormat(WorkbookPart workbookPart, uint styleIndex)
    {
        return workbookPart.WorkbookStylesPart.Stylesheet.Elements<CellFormats>().First().Elements<CellFormat>()
            .ElementAt((int) styleIndex);
    }

    public uint InsertCellFormat(WorkbookPart workbookPart, CellFormat cellFormat)
    {
        CellFormats cellFormats = workbookPart.WorkbookStylesPart.Stylesheet.Elements<CellFormats>().First();
        cellFormats.Append(cellFormat);
        return (uint) cellFormats.Count++;
    }

    private String HexConverter(System.Drawing.Color c)
    {
        String rtn = String.Empty;
        try
        {
            rtn = "#" + c.R.ToString("X2") + c.G.ToString("X2") + c.B.ToString("X2");
        }
        catch 
        {
            //doing nothing
        }

        return rtn;
    }

    private String RGBConverter(System.Drawing.Color c)
    {
        String rtn = String.Empty;
        try
        {
            rtn = "RGB(" + c.R.ToString() + "," + c.G.ToString() + "," + c.B.ToString() + ")";
        }
        catch
        {
            //doing nothing
        }

        return rtn;
    }

    private uint ColorToUInt(System.Drawing.Color color)
    {
        return (uint) ((color.A << 24) | (color.R << 16) |
                       (color.G << 8) | (color.B << 0));
    }
    }
}
