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

        public void SetDefaultConfig(WPFControls.DDBScriptEditor.Config cfg)
        {
            scriptEditorConfiguration1.SetDefaultCfg(cfg);
        }

        public void SetCfgValue(WPFControls.DDBScriptEditor.Config cfg)
        {
            scriptEditorConfiguration1.SetCfgValue(cfg);
        }

        public void UpdateCfgValue(WPFControls.DDBScriptEditor.Config cfg)
        {
            scriptEditorConfiguration1.UpdateCfgValue(cfg);
        }

    }
}
