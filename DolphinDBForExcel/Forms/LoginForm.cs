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
    public partial class LoginForm : Form
    {

        public DBConnection Connection { get; private set; }

        public LoginForm()
        {
            InitializeComponent();
            login1.InputFinishHandler += (conn) =>
            {
                Connection = conn;
                DialogResult = DialogResult.OK; Close();
            };
            login1.InputCancleHandler += () => { DialogResult = DialogResult.Cancel; Close(); };
        }
    }
}
