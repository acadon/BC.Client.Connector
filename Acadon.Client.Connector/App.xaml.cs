using Acadon.Client.Connector.Helpers;
using System;
using System.Windows;

namespace Acadon.Client.Connector
{
    /// <summary>
    /// Interaktionslogik für "App.xaml"
    /// </summary>
    public partial class App : Application
    {

        protected override void OnStartup(StartupEventArgs e)
        {
            if (SingleInstanceHelper.IsFirstInsance())
                base.OnStartup(e);
            else
            {
                MessageBox.Show(Acadon.Client.Connector.Properties.Resources.OnlyOncePerSession, Consts.AppName);
                Environment.Exit(0);
            }
        }
    }
}
