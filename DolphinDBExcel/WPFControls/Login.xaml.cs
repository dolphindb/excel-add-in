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
using System.Collections.ObjectModel;
using System.ComponentModel;
using dolphindb;
using DolphinDBForExcel;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ToolTip;

namespace DolphinDBForExcel.WPFControls
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : System.Windows.Controls.UserControl
    {
        public event Action<DialogResult> Close;
        public class ServerItemText : INotifyPropertyChanged
        {

            public string Text { get; set; }

            public event PropertyChangedEventHandler PropertyChanged;

            private void OnPropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }


        private ObservableCollection<ServerInfo> servers = new ObservableCollection<ServerInfo>();

        public delegate void InputFinish(DBConnection conn);

        public InputFinish InputFinishHandler;

        public delegate void InputCancle();

        public InputCancle InputCancleHandler;

        public Login(Action<DialogResult> Close)
        {
            InitializeComponent();
            string text = "127.0.0.1";
            HostInputBox.Text = text;
            PortInputBox.Text = "8848";

            Loaded += Login_Loaded;
            this.Close = Close;
        }

        private void Login_Loaded(object sender, RoutedEventArgs e)
        {
            servers.Clear();

            //IList<ServerInfo> sinfos = ConnectionController.Instance.LoadServerInfos();
            //foreach (var s in sinfos)
            //    servers.Add(s);

            //if (servers.Count > 0)
            //    ServerComboBox.SelectedIndex = 0;
        }

        private void checkInfoValid(ServerInfo sinfo)
        {
            if(sinfo.Name == null || sinfo.Name == "")
            {
                throw new Exception("Name cannot be empty.");
            }
            if (sinfo.Host == null || sinfo.Host == "")
            {
                throw new Exception("Host cannot be empty.");
            }
        }

        private void TestButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string username = UsernameInputBox.Text;
                string password = PasswordInputBox.Password;
                int port;
                if(!int.TryParse(PortInputBox.Text,out port))
                {
                    throw new Exception("Incorrect port.");
                }
                ServerInfo sinfo = new ServerInfo
                {
                    Host = HostInputBox.Text,
                    Name = NameInputBox.Text,
                    Username = UsernameInputBox.Text,
                    Password = PasswordInputBox.Password,
                    Port = port
                };
                checkInfoValid(sinfo);
                dolphindb.DBConnection conn = new DBConnection();
                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                {
                    if (!conn.connect(sinfo.Host, sinfo.Port))
                        throw new Exception("Failed to connect to " + sinfo.Host + ":" + sinfo.Port);
                    else
                    {
                        System.Windows.MessageBox.Show("Successfully connected to " + sinfo.Host + ":" + sinfo.Port, "success", MessageBoxButton.OK, MessageBoxImage.None);
                    }
                }
                else
                {
                    if (!conn.connect(sinfo.Host, sinfo.Port, username, password))
                        throw new Exception("Failed to connect to " + sinfo.Host + ":" + sinfo.Port);
                    else
                    {
                        System.Windows.MessageBox.Show("Successfully connected to " + sinfo.Host + ":" + sinfo.Port, "success", MessageBoxButton.OK, MessageBoxImage.None);
                    }
                }
            }catch(Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message, "error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public Window FindParentWindow(DependencyObject dependencyObject)
        {
            var parent = VisualTreeHelper.GetParent(dependencyObject) as DependencyObject;
            if (parent == null) return null;

            var parentWindow = Window.GetWindow(parent);
            if (parentWindow != null) return parentWindow;

            return FindParentWindow(parent);
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int port;
                if(!int.TryParse(PortInputBox.Text,out port))
                {
                    throw new Exception("Incorrect port.");
                }
                ServerInfo sinfo = new ServerInfo
                {
                    Host = HostInputBox.Text,
                    Name = NameInputBox.Text,
                    Username = UsernameInputBox.Text,
                    Password = PasswordInputBox.Password,
                    Port = port
                };
                checkInfoValid(sinfo);
                ConnectionController.Instance.ResetConnection(sinfo);
                List<ServerInfo> serverInfos = ConnectionController.Instance.LoadServerInfos();
                serverInfos.Add(sinfo);
                ConnectionController.Instance.SaveServerInfos(serverInfos);
                Close?.Invoke(DialogResult.OK);

            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message, "error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ServerDeleteButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.Button b = sender as System.Windows.Controls.Button;
            servers.Remove(b.Tag as ServerInfo);
        }
    }
}
