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
        static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            using (var f = ErrorDialog.CreateFrom(e.Exception.Message, e.Exception.ToString()))
            {
                f.ShowDialog();
            }
        }

        private void ThisAddIn_Startup(object sender, System.EventArgs e)
        {
            System.Windows.Forms.Application.ThreadException += Application_ThreadException;
        }
 
        protected override Office.IRibbonExtensibility CreateRibbonExtensibilityObject()
        {
            return new Ribbon.AddinRibbon();
        }

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
