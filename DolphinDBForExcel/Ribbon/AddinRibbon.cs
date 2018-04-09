using DolphinDBForExcel.Forms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Office = Microsoft.Office.Core;

namespace DolphinDBForExcel.Ribbon
{
    [ComVisible(true)]
    public class AddinRibbon : Office.IRibbonExtensibility
    {
        private Office.IRibbonUI ribbon;

        public AddinRibbon()
        {
        }

        #region IRibbonExtensibility 成员

        public string GetCustomUI(string ribbonID)
        {
            return GetResourceText("DolphinDBForExcel.Ribbon.AddinRibbon.xml");
        }

        #endregion

        #region 功能区回调
        //在此处创建回叫方法。有关添加回叫方法的详细信息，请访问 https://go.microsoft.com/fwlink/?LinkID=271226

        public void Ribbon_Load(Office.IRibbonUI ribbonUI)
        {
            this.ribbon = ribbonUI;
        }

        public void ConfigConnection(Office.IRibbonControl control)
        {
            //AddinViewController.Instance.ShowLoginDialog();
        }

        //public void InputAndExecuteDBCmd(Office.IRibbonControl control)
        //{
        // //  AddinViewController.Instance.InputAndExecuteDBCmd();
        //    using (var f = new LoginForm())
        //    {
        //        f.ShowDialog();
        //    }
        //}

        public void EnterScriptEditor(Office.IRibbonControl control)
        {
            AddinViewController.Instance.ShowScriptEditorWindow();
        }
        public object GetScriptEditorButtonImage(Office.IRibbonControl control)
        {
            return AddinResource.ddb;
        }

        public object GetLoginButtonImage(Office.IRibbonControl control)
        {
            return AddinResource.login_user;
        }
        #endregion

        #region 帮助器

        private static string GetResourceText(string resourceName)
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            string[] resourceNames = asm.GetManifestResourceNames();
            for (int i = 0; i < resourceNames.Length; ++i)
            {
                if (string.Compare(resourceName, resourceNames[i], StringComparison.OrdinalIgnoreCase) == 0)
                {
                    using (StreamReader resourceReader = new StreamReader(asm.GetManifestResourceStream(resourceNames[i])))
                    {
                        if (resourceReader != null)
                        {
                            return resourceReader.ReadToEnd();
                        }
                    }
                }
            }
            return null;
        }

        #endregion
    }
}
