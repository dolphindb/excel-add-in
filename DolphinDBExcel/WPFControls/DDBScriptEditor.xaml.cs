using dolphindb.streaming;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using static DolphinDBForExcel.WPFControls.DDBScriptEditorConfig;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;
using dolphindb;
using ExcelDna.Integration;
using DolphinDBForExcel.Ribbon;

namespace DolphinDBForExcel.WPFControls
{
    /// <summary>
    /// DDBScriptEditor.xaml 的交互逻辑
    /// </summary>
    public partial class DDBScriptEditor : UserControl
    {
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

            private bool IsRuningCmd { get; set; }

            public DDBScriptEditor()
            {
                InitializeComponent();

                ObjectView.ItemsSource = objectViewItemSource;

                Loaded += (s, e) => ApplyCfg();


                IsRuningCmd = false;
            }


            private void ResetServers()
            {
                serverItems.Clear();
                IList<ServerInfo> sinfos = ConnectionController.Instance.LoadServerInfos();
                foreach (var s in sinfos)
                    serverItems.Add(s);

                ServerInfo sinfoNow = ConnectionController.Instance.getCurrentServerInfo();
                ServerInfo sinfo = serverItems.FirstOrDefault(s => s.Equals(sinfoNow));
                if (sinfo == null)
                    serverItems.Add(sinfoNow);

            }

            private void ApplyCfg()
            {
                EditBox.FontFamily = Fonts.SystemFontFamilies.First(p => p.Source.Equals(AddinRibbon.RibbonController.CONFIG.fontSource));
                EditBox.FontSize = AddinRibbon.RibbonController.CONFIG.fontSize;
            }

            private void ApplyAndSaveCfg()
            {
                ApplyCfg();
                try
                {
                AddinRibbon.RibbonController.CONFIG.SaveConfigToDefaultFile();
                }
                catch (Exception e)
                {
                    AddinViewController.ShowErrorDialog(e);
                }

            }

        private void RefreshObjectViewButton_Click(object sender, RoutedEventArgs e)
            {
                objectViewTreeHelper.UpdateObjectViewItem(ConnectionController.Instance.getConnection(), objectViewItemSource);
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
                RefreshButton.IsEnabled = enable;
            }

            public void AppendLineToScriptResultBox(string s)
            {
                ScriptResultTextBox.AppendText(s);
                ScriptResultTextBox.AppendText(Environment.NewLine);
            }


            private async Task ExecuteScriptNotInBackground(bool start)
            {
                string dateFormat = "yyyy-MM-dd HH:mm:ss.fff";
                try
                {
                    if (start)
                    {
                        DateTime before = DateTime.Now;
                        AppendLineToScriptResultBox(before.ToString(dateFormat) + ": executing code...");
                        SetEnabledRunScriptAbality(false);
                    }
                    else
                    {
                        DateTime after = DateTime.Now;
                        string finishMsg = string.Format("{0}: execution was completed", after.ToString(dateFormat));
                        AppendLineToScriptResultBox(finishMsg);
                    }
                }
                catch (Exception ex)
                {
                    AppendLineToScriptResultBox(DateTime.Now.ToString(dateFormat) + ": execution was completed with exception");
                    AppendLineToScriptResultBox(ex.GetBaseException().Message);
                }
            }

            private async Task ExecuteScriptInBackground(ExecScriptAsync e, bool needScript = true)
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

                    string output = await e(script, AddinRibbon.RibbonController.CONFIG);

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
                objectViewTreeHelper.UpdateObjectViewItem(ConnectionController.Instance.getConnection(), objectViewItemSource);
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
                //int itemIdx = RunAndLoadComboBox.SelectedIndex;
                //if (itemIdx < 0)
                //    return;
                //RunAndLoadComboBox.SelectedIndex = -1;

                //DependencyObject d = RunAndLoadComboBox.ItemContainerGenerator.ContainerFromIndex(itemIdx);
                //string itemTag = (d as ComboBoxItem).Tag as string;

                //string runAndLoadItemTag = FindResource("RunAndLoadItemTag") as string;
                //string runAndLoadToItemTag = FindResource("RunAndLoadToItemTag") as string;

                //if (itemTag.Equals(runAndLoadItemTag))
                //    await ExecuteScriptInBackground(RunScriptAndExportAsync);
                //else if (itemTag.Equals(runAndLoadToItemTag))
                //    await ExecuteScriptInBackground(RunScriptAndExportToAsync);
            }

            private void ResultBoxClean_Click(object sender, RoutedEventArgs e)
            {
                ScriptResultTextBox.Clear();
            }

            private async void ObjectViewLoadItem_Click(object sender, RoutedEventArgs e)
            {
                await ExecuteScriptInBackground(ObjItemExportAsync, false);
            }

            private async void ObjectViewLoadToItem_Click(object sender, RoutedEventArgs e)
            {
                await ExecuteScriptInBackground(ObjItemExportToAsync, false);
            }


            private bool isPoll_ = false;
            private object lockObj_ = new object();

            public bool isPoll
            {
                get
                {
                    lock (lockObj_)
                    {
                        return isPoll_;
                    }
                }
                set
                {
                    lock (lockObj_)
                    {
                        isPoll_ = value;
                    }
                }
            }


            private bool isSubscribe_ = false;
            private object lockObj2_ = new object();

            public bool isSubscribe
            {
                get
                {
                    lock (lockObj2_)
                    {
                        return isSubscribe_;
                    }
                }
                set
                {
                    lock (lockObj2_)
                    {
                        isSubscribe_ = value;
                    }
                }
            }

            // stop polling
            private async void StopItemPolling_Click(object sender, RoutedEventArgs e)
            {
                if (isPoll == false)
                {
                    await ExecuteScriptInBackground(ObjectNotFind, false);
                    return;
                }
                isPoll = false;
                setMenuItemEnable("StopItemTextPolling", false);


                await ExecuteScriptNotInBackground(false);
                SetEnabledRunScriptAbality(true);
            }

            // stop subscribing
            private async void StopItemSubscribing_Click(object sender, RoutedEventArgs e)
            {
                if (isSubscribe == false)
                {
                    await ExecuteScriptInBackground(ObjectNotFind, false);
                    return;
                }
                isSubscribe = false;
                client_.close();
                setMenuItemEnable("StopItemTextSubscribing", false);
                setMenuItemEnable("RunAndLoadItemTextRestartSubscribe", true);
                await ExecuteScriptNotInBackground(false);
                SetEnabledRunScriptAbality(true);
            }


            Microsoft.Office.Interop.Excel.Range GetExcelRange()
        {
            Microsoft.Office.Interop.Excel.Application app = ExcelDnaUtil.Application as Microsoft.Office.Interop.Excel.Application;

            Microsoft.Office.Interop.Excel.Range topLeft =  app.ActiveCell;
            return topLeft;
        }

            // polling
            private async void ObjectViewLoadItemPolling_Click(object sender, RoutedEventArgs e)
        {
            await ExecuteScriptNotInBackground(true);
            try
            {
                Microsoft.Office.Interop.Excel.Range topLeft = GetExcelRange();

                DbObjectInfo info = CheckObjectSelectedItemVariable();
                string frequencyStr = GetFrequency();
                int frequency = int.Parse(frequencyStr);
                isPoll = true;

                setMenuItemEnable("StopItemTextPolling", true);

                Thread thread = new Thread(() =>
                {
                    while (isPoll)
                    {
                        try
                        {
                            Dispatcher.Invoke((Action)(async () =>
                            {
                                await ObjItemExportPollScribeAsync(null, topLeft, info, AddinRibbon.RibbonController.CONFIG);

                            }));
                            Thread.Sleep(frequency);
                            ;
                        }
                        catch (Exception ex)
                        {
                            string dateFormat = "yyyy-MM-dd HH:mm:ss.fff";
                            Dispatcher.Invoke((Action)(() =>
                            {
                                AppendLineToScriptResultBox(DateTime.Now.ToString(dateFormat) + ": execution was completed with exception");
                                AppendLineToScriptResultBox(ex.GetBaseException().Message);
                            }));
                        }

                    }
                });
                thread.Start();
            }
            catch (Exception ex)
            {
                string dateFormat = "yyyy-MM-dd HH:mm:ss.fff";
                AppendLineToScriptResultBox(DateTime.Now.ToString(dateFormat) + ": execution was completed with exception");
                AppendLineToScriptResultBox(ex.GetBaseException().Message);
            }
        }

            // Listen and specify column buttons
            private async void ObjectViewLoadToItemPolling_Click(object sender, RoutedEventArgs e)
            {
            await ExecuteScriptNotInBackground(true);
            try
            {
                Microsoft.Office.Interop.Excel.Range topLeft = GetExcelRange();

                DbObjectInfo info = CheckObjectSelectedItemVariable();
                string frequencyStr = GetFrequency();
                int frequency = int.Parse(frequencyStr);
                isPoll = true;
                setMenuItemEnable("StopItemTextPolling", true);
                Thread thread = new Thread(() =>
                {
                    while (isPoll)
                    {
                        try
                        {
                            Dispatcher.Invoke((Action)(async () =>
                            {
                                await ObjItemExportPollScribeAsync(null, topLeft, info, AddinRibbon.RibbonController.CONFIG);

                            }));
                            Thread.Sleep(frequency);
                        }
                        catch (Exception ex)
                        {
                            string dateFormat = "yyyy-MM-dd HH:mm:ss.fff";
                            Dispatcher.Invoke((Action)(() =>
                            {
                                AppendLineToScriptResultBox(DateTime.Now.ToString(dateFormat) + ": execution was completed with exception");
                                AppendLineToScriptResultBox(ex.GetBaseException().Message);
                            }));
                        }
                    }
                });
                thread.Start();
            }
            catch (Exception ex)
            {
                string dateFormat = "yyyy-MM-dd HH:mm:ss.fff";
                AppendLineToScriptResultBox(DateTime.Now.ToString(dateFormat) + ": execution was completed with exception");
                AppendLineToScriptResultBox(ex.GetBaseException().Message);
            }
        }
            SubScribePortAndKey result_;
            Microsoft.Office.Interop.Excel.Range topLeft_ = null;
            PollingClient client_ = null;
            // Restart Subscribe
            private async void ObjectViewLoadItemRestartSubscribe_Click(object sender, RoutedEventArgs e)
            {

                await ExecuteScriptNotInBackground(true);
                try
                {
                    if (topLeft_ == null || result_.keyName_ == null || result_.subscribePort_ == null)
                    {
                        throw new Exception("No active subscription found.");
                    }
                    string subscribePortStr = result_.subscribePort_;
                    string keyName = result_.keyName_;
                    DbObjectInfo info = CheckObjectSelectedItemVariable();
                    await ObjItemExportScribeAsync(null, topLeft_, info, AddinRibbon.RibbonController.CONFIG, keyName);

                    int subscribePort = int.Parse(subscribePortStr);
                    string tableName = info.name;
                    ServerInfo sinfo = GetIPAndPort();
                    string UUID = System.Guid.NewGuid().ToString("N");
                    client_ = new PollingClient(subscribePort);
                    TopicPoller poller = client_.subscribe(sinfo.Host, sinfo.Port, tableName, tableName + UUID, -1, true);
                    isSubscribe = true;

                    setMenuItemEnable("StopItemTextSubscribing", true);
                    Thread thread = new Thread(() =>
                    {
                        while (isSubscribe)
                        {
                            List<IMessage> msgs = poller.poll(1000);
                            if (msgs.Count > 0)
                            {
                                try
                                {
                                    Dispatcher.Invoke((Action)(async () =>
                                    {
                                        await ObjItemExportScribeAsync(null, topLeft_, info, AddinRibbon.RibbonController.CONFIG, keyName);
                                    }));
                                }
                                catch (Exception ex)
                                {
                                    string dateFormat = "yyyy-MM-dd HH:mm:ss.fff";
                                    Dispatcher.Invoke((Action)(() =>
                                    {
                                        AppendLineToScriptResultBox(DateTime.Now.ToString(dateFormat) + ": execution was completed with exception");
                                        AppendLineToScriptResultBox(ex.GetBaseException().Message);
                                    }));
                                }
                            }
                        }
                        client_.unsubscribe(sinfo.Host, sinfo.Port, tableName, tableName + UUID);
                    });
                    thread.Start();
                }
                catch (Exception ex)
                {
                    string dateFormat = "yyyy-MM-dd HH:mm:ss.fff";
                    AppendLineToScriptResultBox(DateTime.Now.ToString(dateFormat) + ": execution was completed with exception");
                    AppendLineToScriptResultBox(ex.GetBaseException().Message);
                }
            }


            // Subscribe
            private async void ObjectViewLoadItemSubscribe_Click(object sender, RoutedEventArgs e)
            {
            await ExecuteScriptNotInBackground(true);
            try
            {
                result_ = GetSubscribePort();
                string subscribePortStr = result_.subscribePort_;
                string keyName = result_.keyName_;
                topLeft_ = GetExcelRange();
                DbObjectInfo info = CheckObjectSelectedItemVariable();
                await ObjItemExportScribeAsync(null, topLeft_, info, AddinRibbon.RibbonController.CONFIG, keyName);

                int subscribePort = int.Parse(subscribePortStr);
                string tableName = info.name;
                ServerInfo sinfo = GetIPAndPort();
                string UUID = System.Guid.NewGuid().ToString("N");
                client_ = new PollingClient(subscribePort);
                TopicPoller poller = client_.subscribe(sinfo.Host, sinfo.Port, tableName, tableName + UUID, -1, true);
                isSubscribe = true;

                setMenuItemEnable("StopItemTextSubscribing", true);
                Thread thread = new Thread(() =>
                {
                    while (isSubscribe)
                    {
                        List<IMessage> msgs = poller.poll(1000);
                        if (msgs.Count > 0)
                        {
                            try
                            {
                                Dispatcher.Invoke((Action)(async () =>
                                {
                                    await ObjItemExportScribeAsync(null, topLeft_, info, AddinRibbon.RibbonController.CONFIG, keyName);
                                }));
                            }
                            catch (Exception ex)
                            {
                                string dateFormat = "yyyy-MM-dd HH:mm:ss.fff";
                                Dispatcher.Invoke((Action)(() =>
                                {
                                    AppendLineToScriptResultBox(DateTime.Now.ToString(dateFormat) + ": execution was completed with exception");
                                    AppendLineToScriptResultBox(ex.GetBaseException().Message);
                                }));
                            }
                        }
                    }
                    ///client.unsubscribe(sinfo.Host, sinfo.Port, tableName, tableName + UUID);
                    client_.close();
                });
                thread.Start();
            }
            catch (Exception ex)
            {
                string dateFormat = "yyyy-MM-dd HH:mm:ss.fff";
                AppendLineToScriptResultBox(DateTime.Now.ToString(dateFormat) + ": execution was completed with exception");
                AppendLineToScriptResultBox(ex.GetBaseException().Message);
            }
        }

            // Subscribe and specify column buttons
            private async void ObjectViewLoadToItemSubscribe_Click(object sender, RoutedEventArgs e)
            {


                //await executescriptnotinbackground(true);
                //try
                //{
                //    topleft_ = getexcelrangeselect();
                //    result_ = getsubscribeport();
                //    string subscribeportstr = result_.subscribeport_;
                //    string keyname = result_.keyname_;

                //    dbobjectinfo info = checkobjectselecteditemvariable();
                //    await objitemexportscribeasync(null, topleft_, info, enviromentcfg, keyname);
                //    int subscribeport = int.parse(subscribeportstr);
                //    string tablename = info.name;
                //    serverinfo sinfo = getipandport();
                //    // generate uuids to avoid duplication
                //    string uuid = system.guid.newguid().tostring("n");
                //    client_ = new pollingclient(subscribeport);
                //    topicpoller poller = client_.subscribe(sinfo.host, sinfo.port, tablename, tablename + uuid, -1, true);
                //    issubscribe = true;
                //    setmenuitemenable("stopitemtextsubscribing", true);
                //    thread thread = new thread(() =>
                //    {
                //        while (issubscribe)
                //        {
                //            list<imessage> msgs = poller.poll(1000);
                //            if (msgs.count > 0)
                //            {
                //                try
                //                {
                //                    // use dispatcher.invoke to switch the method back to the ui thread
                //                    dispatcher.invoke((action)(async () =>
                //                    {
                //                        await objitemexportscribeasync(null, topleft_, info, enviromentcfg, keyname);
                //                    }));
                //                }
                //                catch (exception ex)
                //                {
                //                    string dateformat = "yyyy-mm-dd hh:mm:ss.fff";
                //                    dispatcher.invoke((action)(() =>
                //                    {
                //                        appendlinetoscriptresultbox(datetime.now.tostring(dateformat) + ": execution was completed with exception");
                //                        appendlinetoscriptresultbox(ex.getbaseexception().message);
                //                    }));
                //                }
                //            }
                //        }
                //        //client.unsubscribe(sinfo.host, sinfo.port, tablename, tablename + uuid);
                //        client_.close();

                //    });
                //    thread.start();
                //}
                //catch (exception ex)
                //{
                //    string dateformat = "yyyy-mm-dd hh:mm:ss.fff";
                //    appendlinetoscriptresultbox(datetime.now.tostring(dateformat) + ": execution was completed with exception");
                //    appendlinetoscriptresultbox(ex.getbaseexception().message);
                //}
            }


            private void TreeViewItem_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
            {
                (sender as TreeViewItem).IsSelected = true;
                e.Handled = true;
            }

            // private void NewServerButton_Click(object sender, RoutedEventArgs e)
            // {
            //     try
            //     {
            //         DBConnection c = AddinViewController.Instance.ShowLoginDialog();
            //         if (c != null)
            //         {
            //             ResetServers();
            //         }
            //     }
            //     catch (Exception ex)
            //     {
            //         AddinViewController.ShowErrorDialog(ex);
            //     }
            // }

            private void ServersList_SelectionChanged(object sender, SelectionChangedEventArgs e)
            {
                //try
                //{
                //    ServerInfo sinfoSelected = serversList.SelectedItem as ServerInfo;
                //    if (sinfoSelected == null)
                //        return;
                //    ServerInfo sinfoNow = ConnectionController.Instance.getCurrentServerInfo();

                //    if (sinfoSelected.Equals(sinfoNow))
                //        return;

                //    ConnectionController.Instance.ResetConnection(sinfoNow);
                //    objectViewTreeHelper.UpdateObjectViewItem(ConnectionController.Instance.getConnection(), objectViewItemSource);
                //}
                //catch (Exception ex)
                //{
                //    AddinViewController.ShowErrorDialog(ex);
                //}
            }

            private void TreeViewItem_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
            {
                ObjectViewItem item = ObjectView.SelectedItem as ObjectViewItem;
                e.Handled = true;

                if (item != null && item.Tag != null)
                    e.Handled = false;
            }
        }
    }