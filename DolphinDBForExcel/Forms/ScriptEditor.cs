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
        public static DBConnection CurrentConnection { get; set; }

        public ScriptEditor()
        {
            InitializeComponent();
            Shown += InitConnection;
        }

        private void InitConnection(object sender, EventArgs e)
        {
            if (CurrentConnection != null)
            {
                wpfScriptEditor1.InitConnection(CurrentConnection);
                return;
            }
            DBConnection conn = AddinViewController.Instance.ShowLoginDialog();
            if (conn == null)
            {
                Close();
                return;
            }
            CurrentConnection = conn;
            wpfScriptEditor1.InitConnection(conn);
        }
    }
}
