using dolphindb.data;
using DolphinDBForExcel.Forms;
using DolphinDBForExcel.Ribbon;
using Microsoft.Office.Core;
using Microsoft.Office.Tools.Excel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using System.Windows.Interop;
using Excel = Microsoft.Office.Interop.Excel;
using OfficeTools = Microsoft.Office.Tools;
using dolphindb;

namespace DolphinDBForExcel
{
    class AddinViewController: Singleton<AddinViewController>
    {
        public IRibbonExtensibility CreateAddinRibbon()
        {
            return new AddinRibbon();
        }

        public void ShowScriptEditorWindow()
        {
            try
            {
                ScriptEditor editor = new ScriptEditor();
                editor.Show(ExcelWin32Window.ActivateWin);
            }
            catch(Exception e)
            {
                ShowErrorDialog(e);
            }
        }

        public static void ShowErrorDialog(Exception e)
        {  
            MessageBox.Show(e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public static void ShowErrorDialog(string message,string detail)
        {
            MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public DBConnection ShowLoginDialog()
        {
            using (var form = new LoginForm())
            {
                if (form.ShowDialog() != DialogResult.OK)
                    return null;

                return form.Connection;
            }
        }
    }

    class ConnectionController : Singleton<ConnectionController>
    {
        public static readonly string ServerInfoSavedXmlFile = "servers.xml";

        private List<ServerInfo> serverInfoCache = null;

        private bool serverInfoCacheInited = false;

        public DBConnection ResetConnection(DBConnection conn,ServerInfo sinfo)
        {
            if (sinfo == null)
                return conn;

            AddinBackend.ResetConnection(conn, sinfo.Host, sinfo.Port);

            return conn;
        }

        public DBConnection ResetConnection(DBConnection conn, ServerInfo sinfo,string username,string password)
        {
            if (sinfo == null)
                return conn;

            AddinBackend.ResetConnection(conn, sinfo.Host, sinfo.Port);
            AddinBackend.RunScript(conn, "login(" + username + ',' + password + ");");

            return conn;
        }


        public void SaveServerInfos(List<ServerInfo> update)
        {
            string filepath = FileUtil.DataFolder.FullFilePath(ServerInfoSavedXmlFile);
            try
            {
                ServerInfosXmlSerializer.ToXmlFile(update, filepath);
            }
            catch (Exception e)
            {
                AddinViewController.ShowErrorDialog("Can't save servers in:" + filepath, e.ToString());
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
                AddinViewController.ShowErrorDialog("Can't load servers in " + filepath, e.ToString());
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
        public Excel.Range topLeft = null;
        public int maxRowsToLoadIntoExcel = -1;
    }

    class ExcelWorkspaceController : Singleton<ExcelWorkspaceController>
    {
        public static int listObjMagicKey = 0;

        private static string GenListObjName(Worksheet sheet,string basicName)
        {
            string name = basicName;

            if (sheet == null)
                return name;

            while (sheet.Controls.Contains(name))
                name = basicName + listObjMagicKey++;
            
            return name;
        }

        public static void DeleteListObjectInRange(Worksheet sheet, Excel.Range r)
        {
            foreach(Excel.ListObject obj in sheet.ListObjects)
            {
                int objTopLeftRow = obj.Range.Row;
                int objTopLeftCol = obj.Range.Column;
           
                int objRightButtonRow = objTopLeftRow + obj.Range.Rows.Count - 1;
                int objRightButtonCol = objTopLeftCol + obj.Range.Columns.Count - 1;

                int rTopLeftRow = r.Row;
                int rTopLeftCol = r.Column;

                int rRightButtonRow = rTopLeftRow + r.Rows.Count - 1;
                int rRightButtonCol = rTopLeftCol + r.Columns.Count - 1;

                if (objRightButtonCol < rTopLeftCol)
                    continue;
                if (objRightButtonRow < rTopLeftRow)
                    continue;
                if (objTopLeftCol > rRightButtonCol)
                    continue;
                if (objTopLeftRow > rRightButtonRow)
                    continue;

               obj.Delete();
            }
        } 
      
        public static void ExportDDBTableToWorksheet(DataTable tb, IList<DATA_TYPE> columnsSrcType,ImportOpt opt)
        {
            List<string> format = new List<string>();
            Debug.Assert(columnsSrcType.Count == tb.Columns.Count);

            foreach (var dt in columnsSrcType)
                format.Add(DDBExcelNumericFormater.GetFormat(dt));

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

        public static void ExportDataTableToWorksheet(DataTable tb, ImportOpt opt, List<string> formats)
        {
            if (tb == null || opt == null)
                return;

            Excel.Range topLeft = opt.topLeft ?? Globals.ThisAddIn.Application.ActiveCell;
            Excel.Worksheet nativeActiveSheet = Globals.ThisAddIn.Application.ActiveSheet as Excel.Worksheet;
            Worksheet sheet = Globals.Factory.GetVstoObject(nativeActiveSheet);
           
            Excel.Range range = topLeft.Resize[tb.Rows.Count + 1, tb.Columns.Count];
            if (opt.overwrite)
                DeleteListObjectInRange(sheet, range);
            if (opt.maxRowsToLoadIntoExcel >= 0)
                RemoveLastNRowsInDataTable(tb, tb.Rows.Count - opt.maxRowsToLoadIntoExcel);

            ListObject list = sheet.Controls.AddListObject(range, GenListObjName(sheet, "DDBTable"));
            list.AutoSetDataBoundColumnHeaders = true;
            list.SetDataBinding(tb);
            list.Disconnect();

            if (formats != null)
                ApplyRangeFormat(list.ListColumns, formats);
        }

        private static int RemoveLastNRowsInDataTable(DataTable tb,int nRows)
        {
            if (nRows <= 0)
                return 0;

            int lastRowIndex = tb.Rows.Count - 1;
            int n = Math.Min(nRows, tb.Rows.Count);

            for (int i = 0; i < n; i++)
                tb.Rows.RemoveAt(lastRowIndex--);

            return n;
        }

        private static void ApplyRangeFormat(Excel.ListColumns cols,List<string> formats)
        {
            int i = 0;
            foreach (Excel.ListColumn col in cols)
            {
                if (i >= formats.Count)
                    return;
                col.Range.NumberFormat = formats[i++];
            }
        }

    }
}
