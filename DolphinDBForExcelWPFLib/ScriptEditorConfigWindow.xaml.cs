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
using System.Windows.Shapes;

namespace DolphinDBForExcelWPFLib
{
    /// <summary>
    /// ScriptEditConfigWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ScriptEditorConfigWindow : Window
    {

        private ScriptEditor.Config DefaultCfg { get; set; }

        private GridLength[] configItemGridRowLength;

        public ScriptEditorConfigWindow(ScriptEditor.Config defaultCfg)
        {
            InitializeComponent();

            foreach (var f in Fonts.SystemFontFamilies)
                FontChoiceBox.Items.Add(f.Source);
            FontChoiceBox.SelectedIndex = 0;

            DefaultCfg = defaultCfg;

            configItemGridRowLength = new GridLength[ConfigItemGrid.RowDefinitions.Count];
            for(int i=0;i!=  ConfigItemGrid.RowDefinitions.Count;i++)
                configItemGridRowLength[i] = ConfigItemGrid.RowDefinitions[i].Height;
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            Util.RemoveWindowIcon(this);
        }

        public void SetCfgValue(ScriptEditor.Config cfg)
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

        public void UpdateCfgValue(ScriptEditor.Config cfg)
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
                MessageBox.Show("invalid font size");
                return;
            }
            if (!Double.TryParse(LineHeightBox.Text, out double lineHeight) || lineHeight <= 0)
            {
                MessageBox.Show("invalid line height");
                return;
            }

            if(AutolimitTableRowsCheckBox.IsChecked==true)
            {
                if (!int.TryParse(maxRowsToLoadIntoExcelBox.Text, out int maxRowsToLoadInto) || maxRowsToLoadInto < 0)
                {
                    MessageBox.Show("Invalid rows");
                    return;
                }
            }

            
           
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void ResetEditorButton_Click(object sender, RoutedEventArgs e)
        {
            SetCfgValue(DefaultCfg);
        }

    }
}
