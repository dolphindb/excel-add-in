using DolphinDBForExcel.Ribbon;
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
using static DolphinDBForExcel.WPFControls.DDBScriptEditorConfig;

namespace DolphinDBForExcel.WPFControls
{
    /// <summary>
    /// Interaction logic for ScriptEditorConfiguration.xaml
    /// </summary>
    public partial class ScriptEditorConfiguration : UserControl
    {
        private DDBScriptEditorConfig.Config DefaultCfg { get; set; }

        public delegate void InputFinish();

        public InputFinish InputFinishHandler;

        public delegate void InputCancle();

        public InputCancle InputCancelHandler;

        public ScriptEditorConfiguration()
        {
            InitializeComponent();
            Version.Text = "Version: " + AddinRibbon.RibbonController.VERSION;
            foreach (var f in Fonts.SystemFontFamilies)
                FontChoiceBox.Items.Add(f.Source);
            FontChoiceBox.SelectedIndex = 0;

            DefaultCfg = null;

            DefaultCfg = new Config
                    {
                        fontSource = "Microsoft YaHei UI",
                        fontSize = 14,
                        overwrite = false,
                        maxRowsToImportInto = 65536,
                        autoLimitMaxRowsToImport = true
                    };
            SetCfgValue(AddinRibbon.RibbonController.CONFIG);
        }

        public void SetCfgValue(DDBScriptEditorConfig.Config cfg)
        {
            if (cfg == null)
                return;

            if (FontChoiceBox.Items.Contains(cfg.fontSource))
                FontChoiceBox.SelectedItem = cfg.fontSource;

            FontSizeBox.Text = cfg.fontSize.ToString();

            maxRowsToLoadIntoExcelBox.Text = cfg.maxRowsToImportInto.ToString();
            AutolimitTableRowsCheckBox.IsChecked = cfg.autoLimitMaxRowsToImport;
        }

        public void UpdateCfgValue(DDBScriptEditorConfig.Config cfg)
        {
            cfg.fontSource = FontChoiceBox.SelectedItem as string;
            Double.TryParse(FontSizeBox.Text, out cfg.fontSize);

            cfg.overwrite = false;

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

            if (AutolimitTableRowsCheckBox.IsChecked == true)
            {
                if (!int.TryParse(maxRowsToLoadIntoExcelBox.Text, out int maxRowsToLoadInto) || maxRowsToLoadInto < 0)
                {
                    AddinViewController.ShowErrorDialog("Invalid rows", "Invalid rows");
                    return;
                }
            }
            UpdateCfgValue(AddinRibbon.RibbonController.CONFIG);
            AddinRibbon.RibbonController.CONFIG.SaveConfigToDefaultFile();
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
