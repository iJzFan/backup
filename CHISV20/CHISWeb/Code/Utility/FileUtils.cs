using Ass;
using NPOI.HSSF.UserModel;
using NPOI.HSSF.Util;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace CHIS.Code.Utility
{
    public class FileUtils
    {


        /// <summary>
        /// 导出Excel文件
        /// </summary> 
        /// <returns></returns>
        public string ExportExcel<T>(IEnumerable<T> finds) where T : class
        {
            var newFile = Global.ConfigSettings.ExportFilePath + $"{Guid.NewGuid()}.xlsx";
            using (var fs = new FileStream(newFile, FileMode.Create, FileAccess.Write))
            {
                IWorkbook workbook = new XSSFWorkbook();
                ISheet sheet1 = workbook.CreateSheet("导出表1");
                sheet1.AddMergedRegion(new CellRangeAddress(0, 0, 0, 10));
                var rowIndex = 0;
                IRow row = sheet1.CreateRow(rowIndex);
                row.Height = 30 * 80;
                row.CreateCell(0).SetCellValue("这是单元格内容，可以设置很长，看能不能自动调整列宽");
                sheet1.AutoSizeColumn(0);
                rowIndex++;


                var sheet2 = workbook.CreateSheet("Sheet2");
                var style1 = workbook.CreateCellStyle();
                style1.FillForegroundColor = HSSFColor.Blue.Index2;
                style1.FillPattern = FillPattern.SolidForeground;

                var style2 = workbook.CreateCellStyle();
                style2.FillForegroundColor = HSSFColor.Yellow.Index2;
                style2.FillPattern = FillPattern.SolidForeground;

                var cell2 = sheet2.CreateRow(0).CreateCell(0);
                cell2.CellStyle = style1;
                cell2.SetCellValue(0);

                cell2 = sheet2.CreateRow(1).CreateCell(0);
                cell2.CellStyle = style2;
                cell2.SetCellValue(1);

                cell2 = sheet2.CreateRow(2).CreateCell(0);
                cell2.CellStyle = style1;
                cell2.SetCellValue(2);

                cell2 = sheet2.CreateRow(3).CreateCell(0);
                cell2.CellStyle = style2;
                cell2.SetCellValue(3);

                cell2 = sheet2.CreateRow(4).CreateCell(0);
                cell2.CellStyle = style1;
                cell2.SetCellValue(4);

                workbook.Write(fs);
                fs.Dispose();
            }

            return newFile;
        }


        public IEnumerable<T> ImportExcel<T>(string filepath) where T : new()
        {
            List<T> rtn = new List<T>();
            IWorkbook workbook = new XSSFWorkbook(filepath);
            try
            {
                ISheet sheet = workbook.GetSheetAt(0);
                //第一行定义数据名
                IRow nameRow = sheet.GetRow(0);
                Dictionary<string, int> nameIndex = new Dictionary<string, int>();
                for (int i = 0; i < nameRow.Cells.Count; i++)
                {
                    nameIndex.Add(nameRow.GetCell(i).StringCellValue.ToUpper(), i);
                }
                //第二行定义数据中文名

                //第三行开始是数据
                for (int i = 2; i <= sheet.LastRowNum; i++)
                {
                    IRow row = sheet.GetRow(i);
                    //映射到对应的数据项目
                    var t = new T();
                    var type = t.GetType();
                    var pp = type.GetProperties();
                    foreach (var p in pp)
                    {
                        var kn = p.Name;
                        if (nameIndex.ContainsKey(kn.ToUpper()))
                        {
                            var vc = row.GetCell(nameIndex[kn.ToUpper()]);
                            PropertyInfo tp = type.GetProperty(kn);
                            if (tp != null && vc != null)
                            {
                                tp.SetValue(t, getExcelValue(tp.PropertyType, vc), null);
                            }
                        }
                    }
                    if (t != null) rtn.Add(t);
                }
            }
            finally { if (workbook != null) workbook.Close(); }
            return rtn;
        }

        /// <summary>
        /// 获取Excel的值
        /// </summary>
        private object getExcelValue(Type propertyType, ICell vc)
        {
            object v = null;
            switch (propertyType.ToString())
            {
                case "System.Int32": v = Ass.P.PIntV(vc.NumericCellValue); break;
                case "System.Decimal": v = Ass.P.PDecimal(vc.NumericCellValue); break;
                case "System.Nullable`1[System.Decimal]": v = Ass.P.PDecimalN(vc.NumericCellValue); break;
                case "System.DateTime": v = vc.DateCellValue; break;
                case "System.String": v = Ass.P.PStr(vc.ToString()); break;
            }
            return v;
        }

        [Obsolete]
        public static Stream DataTableToExcel(string fileName, DataTable data,
            List<ExcelColumnFormat> colFmt,
            string cols,
            Func<object, string, object> tranfunc = null,
            bool isColumnWritten = true, string sheetName = "Sheet1", string tableTitle = null)
        {
            if (data.Rows.Count == 0) { return null; }//data为空，直接退出
            int i = 0;
            int j = 0;
            int count = 0;
            IWorkbook workbook = null;
            ISheet sheet = null;
            NPOIMemoryStream ms = null;

            try
            {
                //转换对齐
                Func<System.Drawing.ContentAlignment?, HorizontalAlignment> _transHAlignment = alignment =>
                {
                    switch (alignment)
                    {
                        case System.Drawing.ContentAlignment.BottomRight:
                        case System.Drawing.ContentAlignment.MiddleRight:
                        case System.Drawing.ContentAlignment.TopRight:
                            return HorizontalAlignment.Right;
                        case System.Drawing.ContentAlignment.BottomCenter:
                        case System.Drawing.ContentAlignment.MiddleCenter:
                        case System.Drawing.ContentAlignment.TopCenter:
                            return HorizontalAlignment.Center;
                    }
                    return HorizontalAlignment.Left;
                };
                //设置值的格式
                Action<IRow, int, object, string> _val = (row, columnIndex, v, colCode) =>
                {
                    string rlt = "";
                    string vstr = Ass.P.PStr(v);
                    string tv = null;
                    if (tranfunc != null) tv = Ass.P.PStr(tranfunc(v, colCode));

                    ExcelColumnFormat fmt = null;
                    if (tv != null && tv != vstr) { rlt = tv; }
                    else if (colFmt.Any(m => m.ColumnNameCode == colCode))
                    {
                        rlt = vstr;
                        string v0 = "";
                        fmt = colFmt.Find(m => m.ColumnNameCode == colCode);
                        if (v != null && fmt.ToStringFormat.IsNotEmpty())
                        {
                            v0 = string.Format("{0:" + fmt.ToStringFormat + "}", v);
                            if (v0 != vstr) rlt = v0;
                        }
                    }
                    else rlt = vstr;

                    //设置值
                    var cell = row.CreateCell(j);
                    var t = rlt.GetStringType();
                    double dbval = 0; DateTime dtval;
                    if (double.TryParse(rlt, out dbval)) { cell.SetCellValue(dbval); }
                    else if (DateTime.TryParse(rlt, out dtval)) cell.SetCellValue(rlt);
                    else cell.SetCellValue(rlt);
                };


                using (ms = new NPOIMemoryStream())
                {

                    if (!GetWorkbook(fileName, out workbook)) { return null; }
                    List<string> colList =cols.IsEmpty()?new List<string>(): (cols + "").Split(',').ToList();//获取列名集合 
                    sheet = workbook.CreateSheet(sheetName);

                    //表标题
                    if (tableTitle.IsNotEmpty())
                    {
                        IRow row = sheet.CreateRow(count++);
                        ICell cell = row.CreateCell(0);
                        XSSFCellStyle ztStyle = (XSSFCellStyle)workbook.CreateCellStyle();
                        IFont ztFont = workbook.CreateFont();
                        ztFont.FontHeightInPoints = 14;
                        ztFont.Underline = FontUnderlineType.DoubleAccounting;
                        ztStyle.SetFont(ztFont);
                        ztStyle.Alignment = HorizontalAlignment.Center;
                        cell.CellStyle = ztStyle;
                        cell.SetCellValue(tableTitle);
                        var colnum = colList.Count > 0 ? colList.Count : data.Columns.Count;//列数
                        sheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, colnum - 1));//合并单元格
                    }

                    Dictionary<int, ICellStyle> _columeCellStyles = new Dictionary<int, ICellStyle>();
                    //写入DataTable的列名
                    if (isColumnWritten == true)
                    {
                        IRow row = sheet.CreateRow(count++);
                        if (colList.Count > 0) //设定了输出的列
                        {
                            for (j = 0; j < colList.Count(); j++)
                            {
                                var key = colList[j];
                                var keyname = colFmt.Any(m => m.ColumnNameCode == key) ? colFmt.Find(m => m.ColumnNameCode == key).ColumnName : key;
                                var cell = row.CreateCell(j);
                                var cellfmt = colFmt.Find(m => m.ColumnNameCode == key);
                                cell.SetCellValue(keyname);

                                //设置对齐与数据格式                                
                                var cStyle = workbook.CreateCellStyle(); bool bfmt = false;
                                if (cellfmt.Alignment != null)
                                {
                                    bfmt = true;
                                    cStyle.Alignment = _transHAlignment(cellfmt.Alignment);
                                }
                                if (cellfmt.DataFormat.IsNotEmpty())
                                {
                                    bfmt = true;
                                    short? dfmt = null; IDataFormat format = workbook.CreateDataFormat();
                                    switch (cellfmt.DataFormat)
                                    {                                       
                                        case "Price": dfmt = format.GetFormat("#,##0.00"); break;
                                        case "Date": dfmt = HSSFDataFormat.GetBuiltinFormat("yyyy-mm-dd"); break;
                                        case "DateTime": dfmt = 0x16;break;// HSSFDataFormat.GetBuiltinFormat("yyyy-mm-dd hh:mm:ss"); break;
                                        case "Id":dfmt = HSSFDataFormat.GetBuiltinFormat("@");break;
                                    }
                                    cStyle.DataFormat = dfmt.Value;
                                }
                                if (bfmt) _columeCellStyles.Add(j, cStyle);

                                //设置列宽
                                if (cellfmt.ColumnWidth.HasValue) sheet.SetColumnWidth(j, (int)((cellfmt.ColumnWidth.Value + 0.72) * 256));
                            }
                        }
                        else
                        {
                            for (j = 0; j < data.Columns.Count; j++)
                            {
                                string key = data.Columns[j].ColumnName;
                                if (colFmt.Any(m => m.ColumnNameCode == key))//改列名为中文
                                {
                                    var cell = row.CreateCell(j);
                                    var cellfmt = colFmt.Find(m => m.ColumnNameCode == key);
                                    cell.SetCellValue(cellfmt.ColumnName);
                                    //设置对齐
                                    if (cellfmt.Alignment != null) sheet.GetColumnStyle(j).Alignment = _transHAlignment(cellfmt.Alignment);
                                    //设置列宽
                                    if (cellfmt.ColumnWidth.HasValue) sheet.SetColumnWidth(j, cellfmt.ColumnWidth.Value);

                                }
                                else
                                {
                                    row.CreateCell(j).SetCellValue(data.Columns[j].ColumnName);
                                }
                            }
                        }
                    }

                    //写入数据
                    if (colList.Count > 0)
                    {
                        for (i = 0; i < data.Rows.Count; i++, count++)
                        {
                            IRow row = sheet.CreateRow(count);
                            for (j = 0; j < colList.Count; j++)
                            {
                                _val(row, j, data.Rows[i][colList[j]], colList[j]);
                            }
                        }
                    }
                    else
                    {
                        for (i = 0; i < data.Rows.Count; i++, count++)
                        {
                            IRow row = sheet.CreateRow(count);
                            for (j = 0; j < data.Columns.Count; j++)
                            {
                                _val(row, j, data.Rows[i][j], data.Columns[j].ColumnName);
                            }
                        }
                    }

                    //设置列格式
                    foreach (int colIndex in _columeCellStyles.Keys)
                    {
                        sheet.SetDefaultColumnStyle(colIndex, _columeCellStyles[colIndex]);
                    }


                    workbook.Write(ms); //写入到excel
                    ms.Flush();
                    ms.Position = 0;
                    return ms;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Exception: " + ex.Message);
                return null;
            }
        }


        /// <summary>
        /// 创建工作薄
        /// </summary>
        /// <param name="path">excel路径</param>
        /// <param name="workbook">创建的工作薄</param>
        /// <returns>创建工作薄是否成功</returns>
        private static bool GetWorkbook(string fileName, out IWorkbook workbook)
        {
            if (fileName.IndexOf("xlsx") > 0)
            {
                workbook = new XSSFWorkbook();//07 及以上
            }
            else if (fileName.IndexOf("xls") > 0)
            {
                workbook = new HSSFWorkbook();//03
            }

            else
            {
                workbook = null;
                return false;
            }
            return true;
        }



    }
}
