using dolphindb.data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using Excel = Microsoft.Office.Interop.Excel;

namespace DolphinDBForExcel
{
    [ComVisible(true)]
    public interface IAddinFunction
    {
        void CallDBMethod(string method, params object[] objs);
    }


    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.None)]
    public class AddinFunction : IAddinFunction
    {
        public void CallDBMethod(string method, params object[] objs)
        {

            //try
            //{
            //    ImportOpt opt = new ImportOpt();
            //    opt.overwrite = true;
            //    ExcelWorkspaceController.ImportTableFromCmdToWorksheet(method, opt);
            //}
            //catch (Exception e)
            //{
            //    AddinViewController.ShowErrorDialog(e);
            //}
            // object[] obj = objs.First() as object[];

            //DateTime.TryParse()
            // MessageBox.Show(DateTime.FromOADate((Double)obj[0]).ToLongDateString());
            //string istr = "";
            //if (objs.Length != 0)
            //{
            //    foreach (var v in (object[])objs.First())
            //        istr += (v.GetType().ToString() + " ");
            //}
            //MessageBox.Show(istr);
        }
    }
}
