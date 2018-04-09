using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net;
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

namespace DolphinDBForExcelWPFLib
{
    /// <summary>
    /// Login.xaml 的交互逻辑
    /// </summary>
    public partial class Login : UserControl
    {

        public class ServerItemText : INotifyPropertyChanged
        {
            
            public string Text {get;set;}

            public event PropertyChangedEventHandler PropertyChanged;

            private void OnPropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        ServerItemText serverItemTxt;

        private ObservableCollection<string> servers = new ObservableCollection<string>();

        public delegate void InputFinish(IList<string> servers,string host,int port, string username, string password);

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
        }

        public void SetServerItem(IList<string> addresses)
        {
            servers.Clear();
            foreach (var s in addresses)
                servers.Add(s);
        }

        public void AddServerItem(string address)
        {
            if (!servers.Contains(address))
                servers.Add(address);
        }

        private void ServerDeleteButton_Click(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            servers.Remove(b.Tag as string);
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            string s = serverItemTxt.Text;
            if (!Util.ParseServerStr(s,out string host,out int port))
            {
                Util.ShowErrorMessageBox("Invalid server");
                return;
            }

            string username = UsernameInputBox.Text;
            string password = PasswordInputBox.Password;

            AddServerItem(s);

            InputFinishHandler?.Invoke(servers, host, port, username, password);
            return;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            InputCancleHandler?.Invoke();
        }
    }
}
