using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Threading.Tasks;
using Excel = Microsoft.Office.Interop.Excel;
using System.Data;
using dolphindb.data;

namespace DolphinDBForExcel.WPFControls
{
    public partial class DDBScriptEditor : UserControl
    {
        public delegate Task<string> ExecScriptAsync(string script, Config cfg);

        private string GenExportOutputLog(int tbTotalRows,Config cfg)
        {
            int importedRow = cfg.autoLimitMaxRowsToImport ? Math.Min(tbTotalRows, cfg.maxRowsToImportInto) : tbTotalRows;
            if (importedRow == tbTotalRows)
                return string.Format("{0:N0} records have been imported!", importedRow);
            else
                return string.Format("{0:N0}/{1:N0} of records have been imported! " +
                    "To change the number of rows being imported, please go to settings.",
                    importedRow, tbTotalRows);
        }

        private ImportOpt GenImportOptFromConfig(Config cfg, Excel.Range topLeft)
        {
            return new ImportOpt
            {
                overwrite = cfg.overwrite,
                maxRowsToLoadIntoExcel = cfg.autoLimitMaxRowsToImport ? cfg.maxRowsToImportInto : -1,
                topLeft = topLeft
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
                    throw new ApplicationException("The script doesn't return any data.\n" +
                        "If the script assigns data to some variables, " +
                        "please select the variable at left panel to export data.");

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

        private async Task<string> RunScriptAndExportAsync(string script,Config cfg)
        {
            TableResult result = await RunScriptAndFetchResultAsDataTableAsync(conn, script);
            AppendRowLabelIfMatrix(result);
            return ExportTableAndGenOutputLog(result.table, result.columnSrcType, cfg, null);
        }

        private Excel.Range ShowSelectRangeInputBox()
        {
            return Globals.ThisAddIn.Application.
               InputBox(Prompt: "Select Range", Type: 8) as Excel.Range;
        }

        private async Task<string> RunScriptAndExportToAsync(string script, Config cfg)
        {
            Excel.Range topLeft = ShowSelectRangeInputBox();
            TableResult result = await RunScriptAndFetchResultAsDataTableAsync(conn, script);
            AppendRowLabelIfMatrix(result);
            return ExportTableAndGenOutputLog(result.table, result.columnSrcType, cfg, topLeft);
        }

        private async Task<string> RunScriptAsync(string script, Config cfg)
        {
            try
            {
                IEntity e = await Task.Factory.StartNew(() =>
                {
                    return AddinBackend.RunScript(conn,script);
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

        private void RenameDataTableColumnBasedValueName(TableResult result,string valueName)
        {
            DataTable dt = result.table;
            switch(result.srcForm)
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
                        if (result.matrix_ColumnLabels == null )
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

        private async Task<string> ObjItemExportAsync(string script, Config cfg)
        {
            DbObjectInfo info = CheckObjectSelectedItemVariable();

            TableResult result = await RunScriptAndFetchResultAsDataTableAsync(conn, info.name);
            RenameDataTableColumnBasedValueName(result, info.name);

            return ExportTableAndGenOutputLog(result.table, result.columnSrcType, cfg, null);
        }

        private async Task<string> ObjItemExportToAsync(string script, Config cfg)
        { 
            DbObjectInfo info = CheckObjectSelectedItemVariable();

            Excel.Range topLeft = ShowSelectRangeInputBox();
            TableResult result = await RunScriptAndFetchResultAsDataTableAsync(conn, info.name);
            RenameDataTableColumnBasedValueName(result, info.name);

            return ExportTableAndGenOutputLog(result.table, result.columnSrcType, cfg, topLeft);
        }
    }
}
