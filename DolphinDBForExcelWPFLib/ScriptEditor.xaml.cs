using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
    /// ScriptEditWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ScriptEditor : UserControl
    {
        [Serializable]
        public class Config :ICloneable
        {
            public string fontSource;
            public double fontSize;
            public double lineHeight;
            public bool overwrite;
            public int maxRowsToImportInto;
            public bool autoLimitMaxRowsToImport;
            public object Clone()
            {
                return new Config
                {
                    fontSource = fontSource,
                    fontSize = fontSize,
                    lineHeight = lineHeight,
                    overwrite = overwrite,
                    maxRowsToImportInto = maxRowsToImportInto,
                    autoLimitMaxRowsToImport = autoLimitMaxRowsToImport
                };
            }
        }

        private Config DefaultCfg { get; set; }

        public class ObjectViewItem : INotifyPropertyChanged
        {
            bool _IsExpanded = false;

            public bool IsExpanded
            {
                get { return _IsExpanded; }
                set
                {
                    if (_IsExpanded == value)
                        return;
                    _IsExpanded = value;
                    OnPropertyChanged("IsExpanded");
                    IconImage = _IsExpanded ? ExpandImage : UnExpandImage;
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;

            ObservableCollection<ObjectViewItem> _Children = new ObservableCollection<ObjectViewItem>();

            public ObservableCollection<ObjectViewItem> Children
            {
                get { return _Children; }
                set
                {
                    if (_Children == value)
                        return;
                    _Children = value;
                    OnPropertyChanged("Children");
                }
            }

            string _Header = "";

            public string Header
            {
                get { return _Header; }
                set
                {
                    if (_Header.Equals(value))
                        return;
                    _Header = value;
                    OnPropertyChanged("Header");
                }
            }

            public Object Tag { get; set; }

            public BitmapSource ExpandImage;

            public BitmapSource UnExpandImage;

            BitmapSource _IconImage;

            public BitmapSource IconImage
            {
                get { return _IconImage; }
                set
                {
                    if (_IconImage == value)
                        return;
                    _IconImage = value;
                    OnPropertyChanged("IconImage");
                }
            }

            private void OnPropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        ObservableCollection<ObjectViewItem> objectViewItemSource;

        public class ServerItems : INotifyPropertyChanged
        {
            string _selectItem;

            public string SelectedItem
            {
                get { return _selectItem; }
                set
                {
                    if (_selectItem.Equals(value))
                        return;
                    _selectItem = value;
                    OnPropertyChanged("SelectedItem");
                }
            }

            public ObservableCollection<string> items = new ObservableCollection<string>();

            public event PropertyChangedEventHandler PropertyChanged;

            private void OnPropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        ServerItems serverItems;

        public Config EnviromentCfg{ get; private set; }

        private bool IsRuningCmd { get; set; }

        public ScriptEditor()
        {
            InitializeComponent();
            objectViewItemSource = new ObservableCollection<ObjectViewItem>();
            ObjectView.ItemsSource = objectViewItemSource;

            serverItems = new ServerItems();
            serversList.DataContext = serverItems;

            Loaded += (s, e) => InitEditerConfigValue();
            //Loaded += (s, e) => InitServersHandler?.Invoke(serverItems);
            Loaded += (s, e) => UpdateObjectViewItemHandler?.Invoke(objectViewItemSource);


            DefaultCfg = new Config
            {
                fontSource = EditBox.FontFamily.Source,
                fontSize = EditBox.FontSize,
                lineHeight = EditBox.Document.LineHeight,
                overwrite = false,
                maxRowsToImportInto = 65535,
                autoLimitMaxRowsToImport = true
            };

            IsRuningCmd = false;
        }

        public void SetServers(IList<string> servers)
        {
            //serversItemSource.Clear();
            //foreach (var s in servers)
            //    serversItemSource.Add(s);
        }

        private void InitEditerConfigValue()
        {
            try
            {
                if (ReadConfigHandler != null)
                    EnviromentCfg = ReadConfigHandler();
            }
            catch(Exception e)
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

        private void ReadAndApplyCfg()
        {
            Config newCfg = null;
            try
            {
                newCfg = ReadConfigHandler?.Invoke();
            }
            catch (FileNotFoundException)
            {
            }
            catch (DirectoryNotFoundException)
            {
            }
            catch (Exception e)
            {
                UnExpectExceptionHandler?.Invoke(e);
                return;
            }

            if (newCfg == null)
                return;

            ApplyCfg();
        }

        private void ApplyAndSaveCfg()
        {
            ApplyCfg();
            try
            {
                SaveConfigHandler?.Invoke(EnviromentCfg);
            }
            catch (Exception e)
            {
                UnExpectExceptionHandler?.Invoke(e);
            }
           
        }

        public ObjectViewItem SelectedObjItem()
        {
            return ObjectView.SelectedItem as ObjectViewItem;
        }

        private void CfgButton_Click_ShowCfgWindow(object sender, RoutedEventArgs e)
        {
            var w = new ScriptEditorConfigWindow(DefaultCfg);
            w.SetCfgValue(EnviromentCfg);
            if (w.ShowDialog() == true)
            {
                w.UpdateCfgValue(EnviromentCfg);
                ApplyAndSaveCfg();
            }
            w.Close();
        }

        private void RefreshObjectViewButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateObjectViewItemHandler?.Invoke(objectViewItemSource);
        }

        private void CollapseAllObjectViewItem(IList<ObjectViewItem> items)
        {
           foreach(var item in items)
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

        private async Task ExecuteScriptInBackground(ExecScriptAsync e)
        {
            if (IsRuningCmd)
                return;

            string dateFormat = "yyyy-MM-dd HH:mm:ss.fff";

            try
            {
                string script = GetScriptString();
                if (string.IsNullOrWhiteSpace(script))
                    return;

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
                AppendLineToScriptResultBox(DateTime.Now.ToString(dateFormat) + " execution was completed with exception");
                AppendLineToScriptResultBox(ex.GetBaseException().Message);
            }
            finally
            {
                IsRuningCmd = false;
                SetEnabledRunScriptAbality(true);
                ScriptResultTextBox.ScrollToEnd();
            }

            AppendLineToScriptResultBox("");
            UpdateObjectViewItemHandler?.Invoke(objectViewItemSource);
        }

        private async void RunAndLoadButton_Click(object sender, RoutedEventArgs e)
        {
            await ExecuteScriptInBackground(RunScriptAndLoadAsyncHandler);
        }

        private async void RunScriptButton_Click(object sender, RoutedEventArgs e)
        {
            await ExecuteScriptInBackground(RunScriptAsyncHandler);
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
                await ExecuteScriptInBackground(RunScriptAndLoadAsyncHandler);
            else if (itemTag.Equals(runAndLoadToItemTag))
                await ExecuteScriptInBackground(RunScriptAndLoadToAsyncHandler);
        }

        private void ResultBoxClean_Click(object sender, RoutedEventArgs e)
        {
            ScriptResultTextBox.Clear();
        }

        private async void ObjectViewLoadItem_Click(object sender, RoutedEventArgs e)
        {
            await ExecuteScriptInBackground(ObjItemLoadAsyncHandler);
        }

        private async void ObjectViewLoadToItem_Click(object sender, RoutedEventArgs e)
        {
            await ExecuteScriptInBackground(ObjItemLoadToAsyncHandler);
        }

        private void TreeViewItem_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            (sender as TreeViewItem).IsSelected = true;
            e.Handled = true;
        }

        private void NewServerButton_Click(object sender, RoutedEventArgs e)
        {
            //try
            //{
            //    EditServerHandler?.Invoke(serversItemSource);
            //}
            //catch (Exception ex)
            //{
            //    UnExpectExceptionHandler?.Invoke(ex);
            //}
        }
    }
}
