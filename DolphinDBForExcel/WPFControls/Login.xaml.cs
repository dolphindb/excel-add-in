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

namespace DolphinDBForExcel.WPFControls
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : UserControl
    {
        public class ServerItemText : INotifyPropertyChanged
        {

            public string Text { get; set; }

            public event PropertyChangedEventHandler PropertyChanged;

            private void OnPropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        ServerItemText serverItemTxt;

        private ObservableCollection<ServerInfo> servers = new ObservableCollection<ServerInfo>();

        public delegate void InputFinish(DBConnection conn);

        public InputFinish InputFinishHandler;

        public delegate void InputCancle();

        public InputCancle InputCancleHandler;

        public Login()
        {
            InitializeComponent();
            serverItemTxt = new ServerItemText();
            serverItemTxt.Text = "127.0.0.1:8848";
            ServerComboBox.DataContext = serverItemTxt;
            ServerComboBox.ItemsSource = servers;

            Loaded += Login_Loaded;
        }

        private void Login_Loaded(object sender, RoutedEventArgs e)
        {
            servers.Clear();

            IList<ServerInfo> sinfos = ConnectionController.Instance.LoadServerInfos();
            foreach (var s in sinfos)
                servers.Add(s);

            if (servers.Count > 0)
                ServerComboBox.SelectedIndex = 0;
        }

        public void AddServerItemToFirstAndSelected(ServerInfo sinfo)
        {
            ServerInfo si = servers.FirstOrDefault(s => s.Equals(sinfo));
            if (si != null)
                servers.Remove(si);

            servers.Insert(0, sinfo);
            ServerComboBox.SelectedIndex = 0;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            InputCancleHandler?.Invoke();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ServerInfo sinfo = ServerInfo.FromString(serverItemTxt.Text);
                AddServerItemToFirstAndSelected(sinfo);
                DBConnection conn = new DBConnection();

                string username = UsernameInputBox.Text;
                string password = PasswordInputBox.Password;
                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                    ConnectionController.Instance.ResetConnection(conn, sinfo);
                else
                    ConnectionController.Instance.ResetConnection(conn, sinfo, username, password);

                ConnectionController.Instance.SaveServerInfos(servers.ToList());

                InputFinishHandler(conn);
            }
            catch(Exception ex)
            {
                AddinViewController.ShowErrorDialog(ex);
            }
        }

        private void ServerDeleteButton_Click(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            servers.Remove(b.Tag as ServerInfo);
        }
    }
}
