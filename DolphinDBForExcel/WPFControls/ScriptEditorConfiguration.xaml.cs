using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DolphinDBForExcel.WPFControls
{
    /// <summary>
    /// Interaction logic for ScriptEditorConfiguration.xaml
    /// </summary>
    public partial class ScriptEditorConfiguration : UserControl
    {
        private DDBScriptEditor.Config DefaultCfg { get; set; }

        public delegate void InputFinish();

        public InputFinish InputFinishHandler;

        public delegate void InputCancle();

        public InputCancle InputCancelHandler;

        public ScriptEditorConfiguration()
        {
            InitializeComponent();
            
            foreach (var f in Fonts.SystemFontFamilies)
                FontChoiceBox.Items.Add(f.Source);
            FontChoiceBox.SelectedIndex = 0;

            DefaultCfg = null;
        }

        public void SetDefaultCfg(DDBScriptEditor.Config cfg)
        {
            DefaultCfg = cfg;
        }

        public void SetCfgValue(DDBScriptEditor.Config cfg)
        {
            if (cfg == null)
                return;

            if (FontChoiceBox.Items.Contains(cfg.fontSource))
                FontChoiceBox.SelectedItem = cfg.fontSource;

            FontSizeBox.Text = cfg.fontSize.ToString();
            LineHeightBox.Text = cfg.lineHeight.ToString();

            OverwriteCheckBox.IsChecked = cfg.overwrite;
            maxRowsToLoadIntoExcelBox.Text = cfg.maxRowsToImportInto.ToString();
            AutolimitTableRowsCheckBox.IsChecked = cfg.autoLimitMaxRowsToImport;
        }

        public void UpdateCfgValue(DDBScriptEditor.Config cfg)
        {
            cfg.fontSource = FontChoiceBox.SelectedItem as string;
            Double.TryParse(FontSizeBox.Text, out cfg.fontSize);
            Double.TryParse(LineHeightBox.Text, out cfg.lineHeight);

            cfg.overwrite = OverwriteCheckBox.IsChecked == true;

            cfg.autoLimitMaxRowsToImport = AutolimitTableRowsCheckBox.IsChecked == true;
            if (cfg.autoLimitMaxRowsToImport)
                int.TryParse(maxRowsToLoadIntoExcelBox.Text, out cfg.maxRowsToImportInto);
        }
    
        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (!Double.TryParse(FontSizeBox.Text, out double fontSize) || fontSize <= 0)
            {
                AddinViewController.ShowErrorDialog("Invalid font size", "Invalid font size");
                return;
            }
            if (!Double.TryParse(LineHeightBox.Text, out double lineHeight) || lineHeight <= 0)
            {
                AddinViewController.ShowErrorDialog("Invalid line height", "Invalid line height");
                return;
            }

            if (AutolimitTableRowsCheckBox.IsChecked == true)
            {
                if (!int.TryParse(maxRowsToLoadIntoExcelBox.Text, out int maxRowsToLoadInto) || maxRowsToLoadInto < 0)
                {
                    AddinViewController.ShowErrorDialog("Invalid rows", "Invalid rows");
                    return;
                }
            }

            InputFinishHandler?.Invoke();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            InputCancelHandler?.Invoke();
        }

        private void ResetEditorButton_Click(object sender, RoutedEventArgs e)
        {
            SetCfgValue(DefaultCfg);
        }

    }
}
