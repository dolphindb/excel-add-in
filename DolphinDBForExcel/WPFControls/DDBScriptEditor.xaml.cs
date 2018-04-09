using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Xml;
using System.Xml.Serialization;
using System.Windows.Shapes;
using dolphindb;

namespace DolphinDBForExcel.WPFControls
{
    /// <summary>
    /// Interaction logic for ScriptEditor.xaml
    /// </summary>
    public partial class DDBScriptEditor : UserControl
    {
        private Config DefaultCfg { get; set; }

        public string ScriptText
        {
            get
            {
                return new TextRange(EditBox.Document.ContentStart, EditBox.Document.ContentEnd).Text;
            }
            set
            {
                new TextRange(EditBox.Document.ContentStart, EditBox.Document.ContentEnd).Text = value;
            }
        }

        public string ResultText
        {
            get
            {
                return ScriptResultTextBox.Text;
            }
            set
            {
                ScriptResultTextBox.Text = value;
            }
        }

        ObjectViewTreeHelper objectViewTreeHelper = new ObjectViewTreeHelper();

        ObservableCollection<ObjectViewItem> objectViewItemSource = new ObservableCollection<ObjectViewItem>();

        ObservableCollection<ServerInfo> serverItems = new ObservableCollection<ServerInfo>();

        private DBConnection conn;

        public Config EnviromentCfg { get; private set; }

        private bool IsRuningCmd { get; set; }

        public DDBScriptEditor()
        {
            InitializeComponent();
            
            ObjectView.ItemsSource = objectViewItemSource;
            serversList.ItemsSource = serverItems;

            Loaded += (s, e) => InitEditerConfigValue();

            DefaultCfg = new Config
            {
                fontSource = EditBox.FontFamily.Source,
                fontSize = EditBox.FontSize,
                lineHeight = EditBox.Document.LineHeight,
                overwrite = false,
                maxRowsToImportInto = 65536,
                autoLimitMaxRowsToImport = true
            };

            IsRuningCmd = false;
        }

        public void InitConnection(DBConnection connection)
        {
            conn = connection;
            objectViewTreeHelper.UpdateObjectViewItem(conn, objectViewItemSource);
            ResetServers();
        }

        private void ResetServers()
        {
            serverItems.Clear();
            IList<ServerInfo> sinfos = ConnectionController.Instance.LoadServerInfos();
            foreach (var s in sinfos)
                serverItems.Add(s);

            ServerInfo sinfoNow = new ServerInfo { Host = conn.HostName, Port = conn.Port };
            ServerInfo sinfo = serverItems.FirstOrDefault(s => s.Equals(sinfoNow));
            if (sinfo == null)
                serverItems.Add(sinfoNow);

            serversList.SelectedItem = sinfoNow;
        }

        private void InitEditerConfigValue()
        {
            try
            {
                EnviromentCfg = Config.ReadConfigFromDefaultFile();
            }
            catch (Exception e)
            {
                EnviromentCfg = null;
            }

            if (EnviromentCfg != null)
                ApplyCfg();
            else
                EnviromentCfg = DefaultCfg.Clone() as Config;
        }

        private void ApplyCfg()
        {
            EditBox.FontFamily = Fonts.SystemFontFamilies.First(p => p.Source.Equals(EnviromentCfg.fontSource));
            EditBox.FontSize = EnviromentCfg.fontSize;
            EditBox.Document.LineHeight = EnviromentCfg.lineHeight;
        }

        private void ApplyAndSaveCfg()
        {
            ApplyCfg();
            try
            {
                EnviromentCfg.SaveConfigToDefaultFile();
            }
            catch (Exception e)
            {
                AddinViewController.ShowErrorDialog(e);
            }

        }

        private void CfgButton_Click_ShowCfgWindow(object sender, RoutedEventArgs e)
        {
            using (var f = new Forms.ScriptEditorConfiguration())
            {
                f.SetDefaultConfig(DefaultCfg);
                f.SetCfgValue(EnviromentCfg);
                if (f.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                    return;
                f.UpdateCfgValue(EnviromentCfg);
                ApplyAndSaveCfg();
            }
        }

        private void RefreshObjectViewButton_Click(object sender, RoutedEventArgs e)
        {
            objectViewTreeHelper.UpdateObjectViewItem(conn,objectViewItemSource);
        }

        private void CollapseAllObjectViewItem(IList<ObjectViewItem> items)
        {
            foreach (var item in items)
            {
                item.IsExpanded = false;
                CollapseAllObjectViewItem(item.Children);
            }
        }

        private void ExpandAllObjectViewItem(IList<ObjectViewItem> items)
        {
            foreach (var item in items)
            {
                item.IsExpanded = true;
                ExpandAllObjectViewItem(item.Children);
            }
        }

        private void ObjectViewExpandOrCollapseButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in objectViewItemSource)
                if (item.IsExpanded)
                {
                    CollapseAllObjectViewItem(objectViewItemSource);
                    return;
                }
            ExpandAllObjectViewItem(objectViewItemSource);
        }

        private string GetScriptString()
        {
            StringBuilder builder = new StringBuilder();
            if (!string.IsNullOrEmpty(EditBox.Selection.Text))
                builder.Append(EditBox.Selection.Text);
            else
            {
                TextRange all = new TextRange(EditBox.Document.ContentStart, EditBox.Document.ContentEnd);
                builder.Append(all.Text);
            }

            builder.Replace("\r\n", " \n");
            return builder.ToString();
        }

        public void SetEnabledRunScriptAbality(bool enable)
        {
            RunButton.IsEnabled = enable;
            RunAndLoadButton.IsEnabled = enable;
            RunAndLoadComboBox.IsEnabled = enable;
            RefreshButton.IsEnabled = enable;
        }

        public void AppendLineToScriptResultBox(string s)
        {
            ScriptResultTextBox.AppendText(s);
            ScriptResultTextBox.AppendText(Environment.NewLine);
        }

        private async Task ExecuteScriptInBackground(ExecScriptAsync e,bool needScript=true)
        {
            if (IsRuningCmd)
                return;

            string dateFormat = "yyyy-MM-dd HH:mm:ss.fff";

            try
            {
                string script = null;
                if (needScript)
                {
                    script = GetScriptString();
                    if (string.IsNullOrWhiteSpace(script))
                        return;
                }

                IsRuningCmd = true;
                DateTime before = DateTime.Now;
                AppendLineToScriptResultBox(before.ToString(dateFormat) + ": executing code...");
                SetEnabledRunScriptAbality(false);

                string output = await e(script, EnviromentCfg);

                if (!string.IsNullOrEmpty(output))
                    AppendLineToScriptResultBox(output);

                DateTime after = DateTime.Now;
                TimeSpan span = after - before;
                string finishMsg = string.Format("{0}: execution was completed [{1}]", after.ToString(dateFormat), Util.ConvTimeSpanToString(span));
                AppendLineToScriptResultBox(finishMsg);
            }
            catch (Exception ex)
            {
                AppendLineToScriptResultBox(DateTime.Now.ToString(dateFormat) + ": execution was completed with exception");
                AppendLineToScriptResultBox(ex.GetBaseException().Message);
            }
            finally
            {
                IsRuningCmd = false;
                SetEnabledRunScriptAbality(true);
                ScriptResultTextBox.ScrollToEnd();
            }

            AppendLineToScriptResultBox("");
            objectViewTreeHelper.UpdateObjectViewItem(conn,objectViewItemSource);
        }

        private async void RunAndLoadButton_Click(object sender, RoutedEventArgs e)
        {
            await ExecuteScriptInBackground(RunScriptAndExportAsync);
        }

        private async void RunScriptButton_Click(object sender, RoutedEventArgs e)
        {
            await ExecuteScriptInBackground(RunScriptAsync);
        }

        private async void RunAndLoadComboBox_DropDownClosed(object sender, EventArgs e)
        {
            int itemIdx = RunAndLoadComboBox.SelectedIndex;
            if (itemIdx < 0)
                return;
            RunAndLoadComboBox.SelectedIndex = -1;

            DependencyObject d = RunAndLoadComboBox.ItemContainerGenerator.ContainerFromIndex(itemIdx);
            string itemTag = (d as ComboBoxItem).Tag as string;

            string runAndLoadItemTag = FindResource("RunAndLoadItemTag") as string;
            string runAndLoadToItemTag = FindResource("RunAndLoadToItemTag") as string;

            if (itemTag.Equals(runAndLoadItemTag))
                await ExecuteScriptInBackground(RunScriptAndExportAsync);
            else if (itemTag.Equals(runAndLoadToItemTag))
                await ExecuteScriptInBackground(RunScriptAndExportToAsync);
        }

        private void ResultBoxClean_Click(object sender, RoutedEventArgs e)
        {
            ScriptResultTextBox.Clear();
        }

        private async void ObjectViewLoadItem_Click(object sender, RoutedEventArgs e)
        {
            await ExecuteScriptInBackground(ObjItemExportAsync,false);
        }

        private async void ObjectViewLoadToItem_Click(object sender, RoutedEventArgs e)
        {
            await ExecuteScriptInBackground(ObjItemExportToAsync,false);
        }

        private void TreeViewItem_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            (sender as TreeViewItem).IsSelected = true;
            e.Handled = true;
        }

        private void NewServerButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DBConnection c = AddinViewController.Instance.ShowLoginDialog();
                if (c != null)
                {
                    conn = c;
                    ResetServers();
                }
            }
            catch (Exception ex)
            {
                AddinViewController.ShowErrorDialog(ex);
            }
        }

        private void ServersList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                ServerInfo sinfoSelected = serversList.SelectedItem as ServerInfo;
                if (sinfoSelected == null)
                    return;
                ServerInfo sinfoNow = new ServerInfo { Host = conn.HostName, Port = conn.Port };

                if (sinfoSelected.Equals(sinfoNow))
                    return;

                ConnectionController.Instance.ResetConnection(conn, sinfoSelected);
                objectViewTreeHelper.UpdateObjectViewItem(conn, objectViewItemSource);
            }
            catch (Exception ex)
            {
                AddinViewController.ShowErrorDialog(ex);
            }
        }
    }
}
