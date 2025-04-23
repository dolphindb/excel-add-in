using dolphindb.data;
using DolphinDBForExcel.Ribbon;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using System.Windows.Interop;
using dolphindb;
using System.Threading.Tasks;
using DolphinDBForExcel.Forms;
using DolphinDBForExcel;
using System.Linq;
using Microsoft.Office.Interop.Excel;
using System.Data;
using ExcelDna.Integration;
using Microsoft.Office.Tools.Excel;

namespace DolphinDBForExcel
{
    class AddinViewController : Singleton<AddinViewController>
    {
        //public void ShowScriptEditorWindow()
        //{
        //    try
        //    {
        //        ScriptEditor editor = new ScriptEditor();
        //        editor.Show(ExcelWin32Window.ActivateWin);
        //    }
        //    catch (Exception e)
        //    {
        //        ShowErrorDialog(e);
        //    }
        //}

        public static void ShowErrorDialog(Exception e)
        {
            MessageBox.Show(e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public static void ShowErrorDialog(string message, string detail)
        {
            MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public void ShowLoginDialog()
        {
            using (var form = new AddServer())
            {
                //if (form.ShowDialog() != DialogResult.OK)
                //    return null;
                //return null;
            }
        }
    }

    class ConnectionController : Singleton<ConnectionController>
    {
        public static readonly string ServerInfoSavedXmlFile = "serversV2.xml";

        private List<ServerInfo> serverInfoCache = null;

        private bool serverInfoCacheInited = false;

        private DBConnection conn = new DBConnection();

        private ServerInfo currentServer;

        public void setConnection(ServerInfo sinfo)
        {
            currentServer = sinfo;
            conn.close();
        }

        public DBConnection ResetConnection(ServerInfo sinfo)
        {
            if (sinfo == null)
                return conn;
            conn.close();
            if(!conn.connect(sinfo.Host, sinfo.Port, sinfo.Username, sinfo.Password))
            {
                throw new Exception("Failed to connect to " +  sinfo.Host + ":" + sinfo.Port);
            };
            currentServer = sinfo;
            return conn;
        }

        public DBConnection getConnection() {
            if(currentServer == null)
            {
                currentServer = getCurrentServerInfo();
            }
            if (!conn.isConnected)
            {
                conn.connect(currentServer.Host, currentServer.Port, currentServer.Username, currentServer.Password);
            }
            return conn;
        }

        public ServerInfo getCurrentServerInfo() {
            if (currentServer == null)
            {
                if (serverInfoCache == null)
                {
                    serverInfoCache = LoadServerInfos();
                }
                if (serverInfoCache.Count == 0)
                {
                    throw new Exception("Please add server info.");
                }
                currentServer = serverInfoCache[0];
            }
            return currentServer;
        }

        // public DBConnection reConnect()
        // {
        //     conn.connect(currentServer.Host, currentServer.Port, currentServer.UserName, currentServer.Name);
        //     return conn;
        // }


        public void SaveServerInfos(List<ServerInfo> update)
        {
            HashSet<string> strings = new HashSet<string>();
            foreach (ServerInfo sinfo in update)
            {
                if (strings.Contains(sinfo.Name))
                {
                    throw new Exception("The server name has already existed.");
                }
                strings.Add(sinfo.Name);
            }
            string filepath = FileUtil.DataFolder.FullFilePath(ServerInfoSavedXmlFile);
            try
            {
                ServerInfosXmlSerializer.ToXmlFile(update, filepath);
            }
            catch (Exception e)
            {
                AddinViewController.ShowErrorDialog("Failed to save server in: " + filepath, e.ToString());
                update = null;
            }

            if (update != null)
            {
                serverInfoCache = ListCloner.Copy(update);
                serverInfoCacheInited = true;
            }
        }

        private List<ServerInfo> LoadServerInfosFromFile()
        {
            string filepath = FileUtil.DataFolder.FullFilePath(ServerInfoSavedXmlFile);
            List<ServerInfo> slist = null;

            try
            {
                slist = ServerInfosXmlSerializer.FromXmlFile(filepath);
            }
            catch (FileNotFoundException)
            {
            }
            catch (DirectoryNotFoundException)
            {
            }
            catch (Exception e)
            {
                AddinViewController.ShowErrorDialog("Failed to load server info:" + filepath, e.ToString());
            }

            if (slist == null)
                return new List<ServerInfo>();

            return slist;
        }

        public List<ServerInfo> LoadServerInfos()
        {
            if (!serverInfoCacheInited)
            {
                serverInfoCache = LoadServerInfosFromFile();
                serverInfoCacheInited = true;
            }

            return ListCloner.Copy(serverInfoCache);
        }
    }

    class ImportOpt
    {
        public bool overwrite = false;
        public Microsoft.Office.Interop.Excel.Range topLeft = null;
        public int maxRowsToLoadIntoExcel = -1;
    }

    class ExcelWorkspaceController : Singleton<ExcelWorkspaceController>
    {
        public static int listObjMagicKey = 0;

        //private static string GenListObjName(Worksheet sheet, string basicName)
        //{
        //    string name = basicName;

        //    if (sheet == null)
        //        return name;

        //    while (sheet.Controls.Contains(name))
        //        name = basicName + listObjMagicKey++;

        //    return name;
        //}

        //public static void DeleteListObjectInRange(Worksheet sheet, Microsoft.Office.Interop.Excel.Range r)
        //{
        //    foreach (Microsoft.Office.Interop.Excel.ListObject obj in sheet.ListObjects)
        //    {
        //        int objTopLeftRow = obj.Range.Row;
        //        int objTopLeftCol = obj.Range.Column;

        //        int objRightButtonRow = objTopLeftRow + obj.Range.Rows.Count - 1;
        //        int objRightButtonCol = objTopLeftCol + obj.Range.Columns.Count - 1;

        //        int rTopLeftRow = r.Row;
        //        int rTopLeftCol = r.Column;

        //        int rRightButtonRow = rTopLeftRow + r.Rows.Count - 1;
        //        int rRightButtonCol = rTopLeftCol + r.Columns.Count - 1;

        //        if (objRightButtonCol < rTopLeftCol)
        //            continue;
        //        if (objRightButtonRow < rTopLeftRow)
        //            continue;
        //        if (objTopLeftCol > rRightButtonCol)
        //            continue;
        //        if (objTopLeftRow > rRightButtonRow)
        //            continue;

        //        obj.Delete();
        //    }
        //}

        public static void ExportDDBTableToWorksheet(System.Data.DataTable tb, IList<DATA_TYPE> columnsSrcType, ImportOpt opt)
        {
            List<string> format = new List<string>();
            Debug.Assert(columnsSrcType.Count == tb.Columns.Count);

            foreach (var dt in columnsSrcType)
                format.Add(DDBExcelNumericFormater.GetFormat(dt));

            ExportDataTableToWorksheet(tb, opt, format);
        }



        public static void ExportDDBTableToWorksheetOverridable(System.Data.DataTable tb, IList<DATA_TYPE> columnsSrcType, ImportOpt opt)
        {
            List<string> format = new List<string>();
            Debug.Assert(columnsSrcType.Count == tb.Columns.Count);

            foreach (var dt in columnsSrcType)
                format.Add(DDBExcelNumericFormater.GetFormat(dt));
            // Set to overridable
            opt.overwrite = true;
            ExportDataTableToWorksheet(tb, opt, format);
        }

        public static void ExportDDBTableToWorksheetOverridable(System.Data.DataTable tb, IList<DATA_TYPE> columnsSrcType, ImportOpt opt, Microsoft.Office.Interop.Excel.Range topLeft)
        {
            List<string> format = new List<string>();
            Debug.Assert(columnsSrcType.Count == tb.Columns.Count);

            foreach (var dt in columnsSrcType)
                format.Add(DDBExcelNumericFormater.GetFormat(dt));
            // Set to overridable
            opt.overwrite = true;
            ExportDataTableToWorksheet(tb, opt, format);
        }

        public static void ExportDDBTableToWorksheet(BasicTable tb, ImportOpt opt)
        {
            List<string> format = new List<string>();

            for (int i = 0; i != tb.columns(); i++)
            {
                DATA_TYPE colType = tb.getColumn(i).getDataType();
                format.Add(DDBExcelNumericFormater.GetFormat(colType));
            }
            ExportDataTableToWorksheet(tb.toDataTable(), opt, format);
        }

        public static void ExportDataTableToWorksheet(System.Data.DataTable tb, ImportOpt opt, List<string> formats)
        {
            Microsoft.Office.Interop.Excel.Application app = ExcelDnaUtil.Application as Microsoft.Office.Interop.Excel.Application;
            if (tb == null || opt == null)
                return;
            if (opt.maxRowsToLoadIntoExcel >= 0)
                RemoveLastNRowsInDataTable(tb, tb.Rows.Count - opt.maxRowsToLoadIntoExcel);

            Microsoft.Office.Interop.Excel.Range topLeft = opt.topLeft ?? app.ActiveCell;
            if(topLeft == null)
            {
                throw new Exception("Must open a sheet when importing data.");
            }
            Microsoft.Office.Interop.Excel.Worksheet nativeActiveSheet = app.ActiveSheet as Microsoft.Office.Interop.Excel.Worksheet;
            int rows = tb.Rows.Count;
            int columns = tb.Columns.Count;
            int startColumn = topLeft.Column;
            int startRow = topLeft.Row;


            for (int column = 0; column < columns; ++column)
            {
                Microsoft.Office.Interop.Excel.Range format =
                    nativeActiveSheet.Range[nativeActiveSheet.Cells[startRow, startColumn + column], nativeActiveSheet.Cells[startRow + rows, startColumn + column]];
                format.NumberFormat = formats[column];
            }

            for (int i = 0; i < tb.Columns.Count; ++i)
            {
                nativeActiveSheet.Cells[startRow, startColumn + i] = tb.Columns[i].ColumnName;
            }

            Range range = nativeActiveSheet.Range[nativeActiveSheet.Cells[startRow + 1, startColumn], nativeActiveSheet.Cells[startRow + rows, startColumn + columns - 1]];

            object[,] dataTmp = new object[rows, columns];


            for (int i = 0; i < rows; ++i)
            {
                DataRow row = tb.Rows[i];
                for (int j = 0; j < columns; ++j)
                {
                    int rowIndex = i;
                    int colIndex = j;
                    if (row[j] is DateTime)
                    {
                        DateTime tmp = (DateTime)row[j];
                        string newFormat = formats[j].Replace(".", "\\.").Replace(":", "\\:").Replace("000", "fff").Replace("hh", "HH");
                        dataTmp[rowIndex, colIndex] = tmp.ToString(newFormat);
                    }else if(row[j] is TimeSpan)
                    {
                        TimeSpan tmp = (TimeSpan)row[j];
                        string newFormat = formats[j].Replace(".", "\\.").Replace(":", "\\:").Replace("000", "fff");
                        dataTmp[rowIndex, colIndex] = tmp.ToString(newFormat);
                    }else if(row[j] is string)
                    {
                        dataTmp[rowIndex, colIndex] = row[j].ToString();
                    }else if (row[j] is byte)
                    {
                        dataTmp[rowIndex, colIndex] = ((byte)row[j]).ToString("#,##0");
                    }else if(row[j] is short)
                    {
                        dataTmp[rowIndex, colIndex] = ((short)row[j]).ToString("#,##0");
                    }
                    else if (row[j] is int)
                    {
                        dataTmp[rowIndex, colIndex] = ((int)row[j]).ToString("#,##0"); 
                    }
                    else if (row[j] is long)
                    {
                        dataTmp[rowIndex, colIndex] = ((long)row[j]).ToString("#,##0");
                    }
                    else if (row[j] is double)
                    {
                        dataTmp[rowIndex, colIndex] = ((double)row[j]).ToString("0.00000000");
                    }
                    else if (row[j] is float)
                    {
                        dataTmp[rowIndex, colIndex] = ((float)row[j]).ToString("0.00000000");
                    }
                    else
                    {
                        dataTmp[rowIndex, colIndex] = row[j];
                    }
                }
            }
            range.Value2 = dataTmp;


            //Microsoft.Office.Interop.Excel.Range range = topLeft.Resize[tb.Rows.Count + 1, tb.Columns.Count];
            //range.NumberFormat = "yyyy/m/d hh:mm:ss.000";

            //Worksheet sheet = Globals.Factory.GetVstoObject(nativeActiveSheet);

            //Excel.Range range = topLeft.Resize[tb.Rows.Count + 1, tb.Columns.Count];
            //if (opt.overwrite)
            //    DeleteListObjectInRange(sheet, range);
            //if (opt.maxRowsToLoadIntoExcel >= 0)
            //    RemoveLastNRowsInDataTable(tb, tb.Rows.Count - opt.maxRowsToLoadIntoExcel);

            //ListObject list = sheet.Controls.AddListObject(range, GenListObjName(sheet, "DDBTable"));
            //list.AutoSetDataBoundColumnHeaders = true;
            //list.SetDataBinding(tb);
            //list.Disconnect();

            //if (formats != null)
            //    ApplyRangeFormat(list.ListColumns, formats);
        }


        //public static async void ExportDataTableToWorkSheetListening(System.Data.DataTable tb, ImportOpt opt, List<string> formats)
        //{
        //    if (tb == null || opt == null)
        //        return;

        //Excel.Range topLeft = opt.topLeft ?? Globals.ThisAddIn.Application.ActiveCell;
        //Excel.Worksheet nativeActiveSheet = Globals.ThisAddIn.Application.ActiveSheet as Excel.Worksheet;
        //Worksheet sheet = Globals.Factory.GetVstoObject(nativeActiveSheet);

        //Excel.Range range = topLeft.Resize[tb.Rows.Count + 1, tb.Columns.Count];

        //bool screenUpdating = Globals.ThisAddIn.Application.ScreenUpdating;
        //Globals.ThisAddIn.Application.ScreenUpdating = false;

        //Console.WriteLine(range.ToString());
        //try
        //{
        //    if (opt.overwrite)
        //        DeleteListObjectInRange(sheet, range);
        //    if (opt.maxRowsToLoadIntoExcel >= 0)
        //        RemoveLastNRowsInDataTable(tb, tb.Rows.Count - opt.maxRowsToLoadIntoExcel);

        //    ListObject list = sheet.Controls.AddListObject(range, GenListObjName(sheet, "DDBTable"));
        //    list.AutoSetDataBoundColumnHeaders = true;
        //    list.SetDataBinding(tb);
        //    list.Disconnect();

        //    if (formats != null)
        //        ApplyRangeFormat(list.ListColumns, formats);


        //}
        //finally
        //{
        //    Globals.ThisAddIn.Application.ScreenUpdating = screenUpdating;
        //}


        //}


        private static int RemoveLastNRowsInDataTable(System.Data.DataTable tb, int nRows)
        {
            if (nRows <= 0)
                return 0;

            int lastRowIndex = tb.Rows.Count - 1;
            int n = Math.Min(nRows, tb.Rows.Count);

            for (int i = 0; i < n; i++)
                tb.Rows.RemoveAt(lastRowIndex--);

            return n;
        }

        private static void ApplyRangeFormat(Microsoft.Office.Interop.Excel.ListColumns cols, List<string> formats)
        {
            int i = 0;
            foreach (Microsoft.Office.Interop.Excel.ListColumn col in cols)
            {
                if (i >= formats.Count)
                    return; 
                col.Range.NumberFormat = formats[i++];
            }
        }

    }
}
