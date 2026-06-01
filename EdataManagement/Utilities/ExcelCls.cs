using EdataManagement.Controllers;
using EdataManagement.Property;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Excel;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace EdataManagement.Utilities
{
    public class ExcelCls
    {

        Microsoft.Office.Interop.Excel.Application xlApp = null;
        Microsoft.Office.Interop.Excel.Workbook xlWorkbook = null;
        Microsoft.Office.Interop.Excel.Worksheet xlWorksheet = null;
        Microsoft.Office.Interop.Excel.Range xlRange = null;

        ExcelControllers _controls = new ExcelControllers();

        public string ExcelValue(ExcelProperty dataProp)
        {
            try
            {
                dataProp = _controls.infoPOAllValue(dataProp);

                if (string.IsNullOrEmpty(dataProp.keyColumn) || string.IsNullOrEmpty(dataProp.valueColumn))
                {
                    return string.Empty;
                }

                xlApp = new Microsoft.Office.Interop.Excel.Application();
                xlApp.Visible = true;  // แสดง Excel Application , ใช้จริงซ่อน = faluse

                
                xlWorkbook = xlApp.Workbooks.Open(dataProp.pathPOall, Type.Missing, true);

                if (xlWorkbook.ReadOnly)
                {
                    Console.WriteLine("The workbook is opened in read-only mode.");
                }

                // เลือก worksheet ที่ต้องการ
                xlWorksheet = xlWorkbook.Sheets[1];
                xlRange = xlWorksheet.UsedRange;

                // โค้ดนี้สำหรับทดสอบว่าถูกต้องหรือไม่ คุณต้องแก้ไขตามที่เห็นสมควร
                int startRow = dataProp.startRow;
                string keyColumnName = dataProp.keyColumnName;
                string valueColumnName = dataProp.valueColumnName;
                string input = dataProp.input;

                // ค้นหาในคอลัมน์ keyColumn
               var keyColumnRange = xlRange.Columns[ExcelColumnNameToIndex(dataProp.keyColumn), Type.Missing];

                // ค้นหาในคอลัมน์ valueColumn
                var valueColumnRange = xlRange.Columns[ExcelColumnNameToIndex(dataProp.valueColumn), Type.Missing];

                // ดำเนินการค้นหา
               var keyCell = keyColumnRange.Find(input, Type.Missing, XlFindLookIn.xlValues, XlLookAt.xlWhole, XlSearchOrder.xlByRows, XlSearchDirection.xlNext, false, Type.Missing, Type.Missing);

                if (keyCell != null)
                {
                    int rowFound = keyCell.Row;

                    // คำทำงานที่ต้องการกับข้อมูลที่พบ
                    string valueFound = (xlRange.Cells[rowFound, ExcelColumnNameToIndex(dataProp.valueColumn)] as Range).Value?.ToString();
                    return valueFound;
                }


                // หาแถวที่มีค่าตรงกับ keyColumn
                //Range keyColumnCells = xlWorksheet.Range[xlWorksheet.Cells[startRow, 1], xlWorksheet.Cells[xlWorksheet.Rows.Count, 1]].Find(keyColumn, Type.Missing, XlFindLookIn.xlValues, XlLookAt.xlWhole, XlSearchOrder.xlByRows, XlSearchDirection.xlNext, false, Type.Missing, Type.Missing);

                // นำค่าจาก valueColumn ของแถวที่พบ keyColumn ไปใส่ในตัวแปร result
                //string result = xlWorksheet.Cells[keyColumnCells.Row, ExcelColumnNameToIndex(valueColumn)].Text;

                return string.Empty;
            }
            catch (Exception ex)
            {
                // ตรวจสอบว่ามี InnerException หรือไม่
                if (ex.InnerException != null)
                {
                    // ดำเนินการตามที่คุณต้องการกับ InnerException
                    Console.WriteLine("Inner Exception: " + ex.InnerException.Message);
                }

                // ดำเนินการตามที่คุณต้องการกับ Exception ที่ถูกจับได้
                Console.WriteLine("Error: " + ex.Message);

                // คืนค่าเปล่าๆ หรือทำอะไรก็ตามที่คุณต้องการ
                return string.Empty;
            }
            finally
            {
                // ทำความสะอาดทรัพยากร
                if (xlRange != null) Marshal.ReleaseComObject(xlRange);
                if (xlWorksheet != null) Marshal.ReleaseComObject(xlWorksheet);
                if (xlWorkbook != null)
                {
                    xlWorkbook.Close();
                    Marshal.ReleaseComObject(xlWorkbook);
                }
                if (xlApp != null)
                {
                    xlApp.Quit();
                    Marshal.ReleaseComObject(xlApp);
                }
            }
        }



        static int ExcelColumnNameToIndex(string columnName)
        {
            int index = 0;

            foreach (char c in columnName)
            {
                index = index * 26 + (int)c - (int)'A' + 1;
            }

            return index;
        }

    }
}
