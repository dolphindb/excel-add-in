using DolphinDBForExcelWPFLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WindowsTest
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //var w = new ScriptEditorWindow();
            //w.OnUpdateObjectViewItem += W_OnUpdateObjectViewItem;
        }

        //private void W_OnUpdateObjectViewItem(IList<ScriptEditor.ObjectViewItem> items)
        //{
        //    var item = new ScriptEditor.ObjectViewItem();
        //    item.Header = "fun";
        //    item.IsExpanded = true;
        //    items.Add(item);

        //    item = new ScriptEditor.ObjectViewItem();
        //    item.Header = "fun2";
        //    item.IsExpanded = true;
        //    items[0].Children.Add(item);
        //}
    }
}
