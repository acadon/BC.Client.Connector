using Acadon.Client.Connector.Classes;
using Acadon.Client.Connector.Helpers;
using System;
using System.Windows;
using System.Windows.Input;

namespace Acadon.Client.Connector
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ClientConnector clientConnector;

        public MainWindow()
        {
            InitializeComponent();
            LogHelper.OnNewEntryAdded += LogHelper_OnNewEntryAdded;

            clientConnector = new ClientConnector();
            if (!clientConnector.Initialize())
                return;

            if (!Properties.Settings.Default.AutostartChanged && !AutostartHelper.AutoStartEnabled(Consts.AppName))
                AutostartHelper.ToggleStartup(Consts.AppName);
        }

        private void LogHelper_OnNewEntryAdded(object sender, string e)
        {
            WriteLog(e);
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            Hide();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            MutexHelper.Dispose();
        }

        private void OpenLog_Click(object sender, RoutedEventArgs e)
        {
            if (this.Visibility == Visibility.Hidden)
                Show();
            else
                Hide();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Autostart_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(AutostartHelper.ToggleStartup(Consts.AppName) ? Properties.Resources.AutostartEnabled : Properties.Resources.AutostartDisabled, "acadon_client.connector");
            Properties.Settings.Default.AutostartChanged = true;
            Properties.Settings.Default.Save();
        }

        private void NotifyIcon_MouseClick(object sender, MouseButtonEventArgs e)
        {
            if (this.Visibility == Visibility.Hidden)
                Show();
            else
                Hide();
        }

        private void WriteLog(string Text)
        {
            Application.Current.Dispatcher.InvokeAsync(() =>
            {
                Log.Text += string.Format("{0}: {1}\n", DateTime.Now, Text);
            });
        }
    }
}
