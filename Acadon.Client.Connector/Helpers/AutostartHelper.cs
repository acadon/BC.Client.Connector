using Microsoft.Win32;
using System.Windows.Forms;

namespace Acadon.Client.Connector.Helpers
{
    public class AutostartHelper
    {
        public static bool ToggleStartup(string AppName)
        {
            string runKey = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run";

            var startupKey = Registry.CurrentUser.OpenSubKey(runKey);

            if (startupKey.GetValue(AppName) == null)
            {
                startupKey.Close();
                startupKey = Registry.CurrentUser.OpenSubKey(runKey, true);
                startupKey.SetValue(AppName, Application.ExecutablePath.ToString());
                startupKey.Close();
                return true;
            }
            else
            {
                startupKey = Registry.CurrentUser.OpenSubKey(runKey, true);
                startupKey.DeleteValue(AppName, false);
                startupKey.Close();
                return false;
            }
        }

        public static bool AutoStartEnabled(string AppName)
        {
            string runKey = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run";
            var startupKey = Registry.CurrentUser.OpenSubKey(runKey);
            var result = (startupKey.GetValue(AppName) != null);
            startupKey.Close();
            return result;
        }
    }
}
