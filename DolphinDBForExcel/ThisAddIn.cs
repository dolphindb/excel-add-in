using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Excel = Microsoft.Office.Interop.Excel;
using Office = Microsoft.Office.Core;
using Microsoft.Office.Tools.Excel;
using System.Threading;
using System.Diagnostics;
using System.Windows.Forms;
using OfficeTools = Microsoft.Office.Tools;
using DolphinDBForExcel.Forms;
using System.Collections;

namespace DolphinDBForExcel
{
    public partial class ThisAddIn
    { 
        //private AddinFunction function;

        static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            AddinViewController.ShowErrorDialog(e.Exception);
        }

        private void ThisAddIn_Startup(object sender, System.EventArgs e)
        {
            System.Windows.Forms.Application.ThreadException += Application_ThreadException;
            //ActivateTaskPaneSpecifiedByWindows(Application.ActiveWindow);
            //    Application.WindowActivate += (book, win) => RemoveInvalidTaskPand();
            //    Application.WindowActivate += (book, win) => ActivateTaskPaneSpecifiedByWindows(win);
            //}
        }

        //private void ActivateTaskPaneSpecifiedByWindows(Excel.Window win)
        //{
        //    //AddinViewController.Instance.SetCurrentTaskPane(GetOrCreatTaskPane(win));
        //}

        //private void RemoveInvalidTaskPand()
        //{
        //    var invalidPanes = new List<OfficeTools.CustomTaskPane>();

        //    foreach (var item in CustomTaskPanes)
        //    {
        //        bool valid = true;
        //        Excel.Window w = null;

        //        try
        //        {
        //            w = item.Window as Excel.Window;
        //        }
        //        catch
        //        {
        //            valid = false;
        //        }

        //        if (!valid)
        //            invalidPanes.Add(item);
        //    }

        //    invalidPanes.ForEach(p => { CustomTaskPanes.Remove(p); p.Dispose(); });
        //}

        //private OfficeTools.CustomTaskPane GetTaskPaneOfTheWindow(Excel.Window win)
        //{
        //    return CustomTaskPanes.FirstOrDefault(p =>
        //    {
        //        if (p.Control.IsDisposed || p.Control.Disposing)
        //            return false;

        //        Excel.Window w = null;

        //        try
        //        {
        //            w = p.Window as Excel.Window;
        //        }
        //        catch
        //        {
        //            w = null;
        //        }

        //        return w != null && w.Hwnd.Equals(win.Hwnd);
        //    });
        //}

        //private OfficeTools.CustomTaskPane GetOrCreatTaskPane(Excel.Window win)
        //{
        //    OfficeTools.CustomTaskPane pane;

        //    if ((pane = GetTaskPaneOfTheWindow(win)) != null)
        //        return pane;

        //    UserControl entranceCtrl;

        //    entranceCtrl = AddinViewController.Instance.CreateAddinEntranceTaskPaneControl();
        //    pane = CustomTaskPanes.Add(entranceCtrl, AddinViewController.AddinTitle,win);
        //    return pane;
        //}

        protected override Office.IRibbonExtensibility CreateRibbonExtensibilityObject()
        {
            //return AddinViewController.Instance.CreateAddinRibbon();
            return new Ribbon.AddinRibbon();
        }

        //protected override object RequestComAddInAutomationService()
        //{
        //    if (function == null)
        //        function = new AddinFunction();
        //    return function;
        //}

        private void ThisAddIn_Shutdown(object sender, System.EventArgs e)
        {
        }

        #region VSTO 生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InternalStartup()
        {
            this.Startup += new System.EventHandler(ThisAddIn_Startup);
            this.Shutdown += new System.EventHandler(ThisAddIn_Shutdown);
        }
        
        #endregion
    }
}
