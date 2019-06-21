using Acadon.Client.Connector.Models;
using Acadon.Client.Connector.Views;
using System.Windows;

namespace Acadon.Client.Connector.Helpers
{
    public static class PermissionHelper
    {
        public static bool CheckPermission(OperationRequest request)
        {
            var checksum = request.GetCheckSum();
            if (Properties.Settings.Default.AllowedScripts?.Contains(checksum) ?? false)
                return true;

            return Application.Current.Dispatcher.Invoke(() => {
                var window = new ConfirmWindow();
                window.OperationName = request.OperationName;
                window.CompanyName = request.CompanyName;
                var dialogResult = window.ShowDialog();
                if (dialogResult.HasValue && !dialogResult.Value)
                    return false;

                if (window.AlwaysYes)
                {
                    if (Properties.Settings.Default.AllowedScripts == null)
                        Properties.Settings.Default.AllowedScripts = new System.Collections.Specialized.StringCollection();
                    Properties.Settings.Default.AllowedScripts.Add(checksum);
                    Properties.Settings.Default.Save();
                }

                return true;
            });
        }
    }
}
