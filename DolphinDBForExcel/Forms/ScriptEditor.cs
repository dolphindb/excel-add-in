using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using dolphindb;

namespace DolphinDBForExcel.Forms
{
    public partial class ScriptEditor : Form
    {
        private static string lastScriptText = "";
        private static string lastResult = "";

        public ScriptEditor()
        {
            InitializeComponent();
            Shown += InitConnection;
        }

        private void InitConnection(object sender, EventArgs e)
        {
            DBConnection conn = AddinViewController.Instance.ShowLoginDialog();
            if (conn == null)
            {
                Close();
                return;
            }
            wpfScriptEditor1.InitConnection(conn);
            wpfScriptEditor1.ScriptText = lastScriptText;
            wpfScriptEditor1.ResultText = lastResult;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            lastScriptText = wpfScriptEditor1.ScriptText;
            lastResult = wpfScriptEditor1.ResultText;
            base.OnClosing(e);
        }
    }
}
