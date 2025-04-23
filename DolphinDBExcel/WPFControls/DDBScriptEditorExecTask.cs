using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Threading.Tasks;
using Excel = Microsoft.Office.Interop.Excel;
using System.Data;
using dolphindb.data;
using DolphinDBForExcel;
using static DolphinDBForExcel.WPFControls.DDBScriptEditorConfig;
using ExcelDna.Integration;
using System.Net.Sockets;
using System.Net;

namespace DolphinDBForExcel.WPFControls
{
    public partial class DDBScriptEditor : UserControl
    {
        public class SubScribePortAndKey
        {
            public SubScribePortAndKey(string subscribePort, string keyName)
            {
                subscribePort_ = subscribePort;
                keyName_ = keyName;
            }
            public string subscribePort_ { get; set; }
            public string keyName_ { get; set; }
        }

        public delegate Task<string> ExecScriptAsync(string script, Config cfg);

        private string GenExportOutputLog(int tbTotalRows, Config cfg)
        {
            int importedRow = cfg.autoLimitMaxRowsToImport ? Math.Min(tbTotalRows, cfg.maxRowsToImportInto) : tbTotalRows;
            if (importedRow == tbTotalRows)
                return string.Format("{0:N0} records have been imported!", importedRow);
            else
                return string.Format("{0:N0}/{1:N0} of records have been imported! " +
                    "To modify the maximum number of rows to be imported, please go to Settings.",
                    importedRow, tbTotalRows);
        }


        private ImportOpt GenImportOptFromConfig(Config cfg, Excel.Range topLeft)
        {
            return new ImportOpt
            {
                overwrite = cfg.overwrite,
                maxRowsToLoadIntoExcel = cfg.autoLimitMaxRowsToImport ? cfg.maxRowsToImportInto : -1,
            };
        }

        private void AppendRowLabelIfMatrix(TableResult result)
        {
            if (result.srcForm != DATA_FORM.DF_MATRIX)
                return;
            if (result.matrix_RowLabels == null)
                return;
            DataTable dt = result.table;

            DataColumn rowLabel = dt.Columns.Add(" ", typeof(string));
            rowLabel.SetOrdinal(0);

            for (int i = 0; i != result.matrix_RowLabels.Count; i++)
            {
                dt.Rows[i][rowLabel] = result.matrix_RowLabels[i];
            }
            result.columnSrcType.Insert(0, DATA_TYPE.DT_STRING);

        }

        private async Task<TableResult>
            RunScriptAndFetchResultAsDataTableAsync(dolphindb.DBConnection conn, string script)
        {
            try
            {
                TableResult tr = await Task.Factory.StartNew(() =>
                {
                    return AddinBackend.RunScriptAndFetchResultAsDataTable(conn, script);
                });

                if (tr == null)
                    throw new ApplicationException("The script must not return NULL.");

                return tr;
            }
            catch (Exception e)
            {
                throw new AggregateException(e);
            }
        }


        private string ExportTableAndGenOutputLog(DataTable tb, IList<DATA_TYPE> columnSrcType,
            Config cfg, Excel.Range topLeft)
        {
            string outputLog = GenExportOutputLog(tb.Rows.Count, cfg);

            ExcelWorkspaceController.ExportDDBTableToWorksheet(
                tb, columnSrcType, GenImportOptFromConfig(cfg, topLeft));

            return outputLog;
        }


        private string ExportTableAndGenOutputLogOverridable(DataTable tb, IList<DATA_TYPE> columnSrcType,
            Config cfg, Excel.Range topLeft)
        {
            string outputLog = GenExportOutputLog(tb.Rows.Count, cfg);

            ExcelWorkspaceController.ExportDDBTableToWorksheetOverridable(
               tb, columnSrcType, GenImportOptFromConfig(cfg, topLeft));

            return outputLog;
        }

        private async Task<string> RunScriptAndExportAsync(string script, Config cfg)
        {
            TableResult result = await RunScriptAndFetchResultAsDataTableAsync(ConnectionController.Instance.getConnection(), script);
            AppendRowLabelIfMatrix(result);
            return ExportTableAndGenOutputLog(result.table, result.columnSrcType, cfg, null);
        }

        //private Excel.Range ShowSelectRangeInputBox()
        //{
        //    return Globals.ThisAddIn.Application.
        //       InputBox(Prompt: "Select Range", Type: 8) as Excel.Range;
        //}

        private async Task<string> RunScriptAndExportToAsync(string script, Config cfg)
        {
            Excel.Range topLeft = GetExcelRange();
            TableResult result = await RunScriptAndFetchResultAsDataTableAsync(ConnectionController.Instance.getConnection(), script);
            AppendRowLabelIfMatrix(result);
            return ExportTableAndGenOutputLog(result.table, result.columnSrcType, cfg, topLeft);
            return "";
        }

        private async Task<string> RunScriptAsync(string script, Config cfg)
        {
            try
            {
                IEntity e = await Task.Factory.StartNew(() =>
                {
                    return AddinBackend.RunScript(ConnectionController.Instance.getConnection(), script);
                });

                return DDBString.GetValueAsStringIfScalarOrPair(e);
            }
            catch (Exception ex)
            {
                throw new AggregateException(ex);
            }
        }

        private DbObjectInfo CheckObjectSelectedItemVariable()
        {
            ObjectViewItem item = ObjectView.SelectedItem as ObjectViewItem;
            if (item == null)
                throw new ArgumentNullException("Invalid Variable");
            DbObjectInfo info = item.Tag as DbObjectInfo;
            if (info == null)
                throw new ArgumentException("Invalid Variable");
            return info;
        }

        private void RenameDataTableColumnBasedValueName(TableResult result, string valueName)
        {
            DataTable dt = result.table;
            switch (result.srcForm)
            {
                case DATA_FORM.DF_VECTOR:
                    dt.Columns[0].ColumnName = valueName;
                    break;
                case DATA_FORM.DF_DICTIONARY:
                    {
                        dt.Columns[0].ColumnName = valueName + "_key";
                        dt.Columns[1].ColumnName = valueName + "_value";
                    }
                    break;
                case DATA_FORM.DF_MATRIX:
                    {
                        if (result.matrix_ColumnLabels == null)
                        {
                            int colNum = dt.Columns.Count;
                            for (int i = 0; i != colNum; i++)
                                dt.Columns[i].ColumnName = valueName + "_col" + i;
                        }

                        AppendRowLabelIfMatrix(result);
                    }
                    break;
            }
        }

        // Deduplication
        private DataTable DistinctDataTable(TableResult result, string columnName)
        {
            DataTable table = result.table;
            DataTable distinctTable = table.Clone();
            Dictionary<string, DataRow> uniqueRows = new Dictionary<string, DataRow>();

            foreach (DataRow row in table.Rows)
            {
                string key = row[columnName].ToString();
                uniqueRows[key] = row;
            }

            foreach (var row in uniqueRows.Values)
            {
                distinctTable.Rows.Add(row.ItemArray);
            }

            // Default ascending sort
            DataView dataView = distinctTable.DefaultView;
            dataView.Sort = columnName + " ASC";
            distinctTable = dataView.ToTable();

            return distinctTable;
        }

        private async Task<string> ObjItemExportAsync(string script, Config cfg)
        {
            DbObjectInfo info = CheckObjectSelectedItemVariable();

            TableResult result = await RunScriptAndFetchResultAsDataTableAsync(ConnectionController.Instance.getConnection(), info.name);
            RenameDataTableColumnBasedValueName(result, info.name);

            return ExportTableAndGenOutputLog(result.table, result.columnSrcType, cfg, null);
        }

        private async Task<string> ObjItemExportToAsync(string script, Config cfg)
        {
            //DbObjectInfo info = CheckObjectSelectedItemVariable();

            //Excel.Range topLeft = ShowSelectRangeInputBox();
            //TableResult result = await RunScriptAndFetchResultAsDataTableAsync(conn, info.name);
            //RenameDataTableColumnBasedValueName(result, info.name);

            //return ExportTableAndGenOutputLog(result.table, result.columnSrcType, cfg, topLeft);
            return "";
        }




        //private Excel.Range GetExcelRangeSelect()
        //{
        //    return ShowSelectRangeInputBox();
        //}

        //  Get the IP as well as the port
        private ServerInfo GetIPAndPort()
        {
            ServerInfo sinfoSelected = ConnectionController.Instance.getCurrentServerInfo();
            return sinfoSelected;
        }


        // Get the subscription port and primary key
        private SubScribePortAndKey GetSubscribePort()
        {

            string version = AddinBackend.RunScript(ConnectionController.Instance.getConnection(), "version()").getString();
            string[] parts = version.Split(' ')[0].Split('.');
            int v0 = int.Parse(parts[0]);
            int v1 = int.Parse(parts[1]);
            int v2 = int.Parse(parts[2]);
            string subscribePort;
            Microsoft.Office.Interop.Excel.Application app = ExcelDnaUtil.Application as Microsoft.Office.Interop.Excel.Application;
            if ((v0 == 2 && v1 == 0 && v2 >= 9) || v0 > 2)
            {
                // server only support reverse connection
                subscribePort = "0";
            }
            else
            {
                subscribePort =  app.InputBox(
                    Prompt: "Enter the subscription port", Type: 2) as string;
            }
            string keyName = app.InputBox(
                Prompt: "Enter the column name as the primary key", Type: 2) as string;
            if (subscribePort == null || subscribePort == "")
                throw new Exception("Subscribe port cannot be empty. ");
            if (keyName == null || keyName == "")
                throw new Exception("Primary key cannot be empty. ");
            return new SubScribePortAndKey(subscribePort, keyName);
        }


        // Sets whether the specified menu item can be clicked
        private void setMenuItemEnable(string resourceName, bool isEnable)
        {

            var loadMenu = ObjectView.Resources["LoadMenu"] as ContextMenu;
            if (loadMenu != null)
            {
                foreach (var item in loadMenu.Items)
                {
                    if (item is MenuItem menuItem && menuItem.Header.Equals(this.FindResource(resourceName)))
                    {
                        menuItem.IsEnabled = isEnable;
                        break;
                    }
                }
            }
        }



        private string GetFrequency()
        {

            Microsoft.Office.Interop.Excel.Application app = ExcelDnaUtil.Application as Microsoft.Office.Interop.Excel.Application;
            string frequency = app.InputBox(
                Prompt: "Enter the refresh time (in milliseconds): ", Type: 2) as string;
            return frequency;
        }

        // Poll
        private async Task<string> ObjItemExportPollScribeAsync(string script, Excel.Range topLeft, DbObjectInfo info, Config cfg)
        {

            TableResult result = await RunScriptAndFetchResultAsDataTableAsync(ConnectionController.Instance.getConnection(), info.name);
            RenameDataTableColumnBasedValueName(result, info.name);
            return ExportTableAndGenOutputLogOverridable(result.table, result.columnSrcType, cfg, topLeft);
        }

        // Subscribe
        private async Task<string> ObjItemExportScribeAsync(string script, Excel.Range topLeft, DbObjectInfo info, Config cfg, string keyName)
        {

            TableResult result = await RunScriptAndFetchResultAsDataTableAsync(ConnectionController.Instance.getConnection(), info.name);
            RenameDataTableColumnBasedValueName(result, info.name);
            DataTable dt = DistinctDataTable(result, keyName);
            return ExportTableAndGenOutputLogOverridable(dt, result.columnSrcType, cfg, topLeft);
        }


        // There is currently no subscription
        private async Task<string> ObjectNotFind(string script, Config cfg)
        {
            throw new ApplicationException("No active subscription found.");
        }

    }
}
