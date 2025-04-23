using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DolphinDBForExcel.Forms
{
    public partial class ScriptEditorConfiguration : Form
    {
        public ScriptEditorConfiguration()
        {
            InitializeComponent();

            scriptEditorConfiguration1.InputFinishHandler += () => { DialogResult = DialogResult.OK; Close(); };
            scriptEditorConfiguration1.InputCancelHandler += () => { Close(); };
        }

        public void SetCfgValue(WPFControls.DDBScriptEditorConfig.Config cfg)
        {
            scriptEditorConfiguration1.SetCfgValue(cfg);
        }

        public void UpdateCfgValue(WPFControls.DDBScriptEditorConfig.Config cfg)
        {
            scriptEditorConfiguration1.UpdateCfgValue(cfg);
        }

        private void elementHost1_ChildChanged(object sender, System.Windows.Forms.Integration.ChildChangedEventArgs e)
        {
        }
    }
}
