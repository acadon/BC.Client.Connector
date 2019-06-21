using System;

namespace Acadon.Client.Connector.Helpers
{
    public static class LogHelper
    {
        public static event EventHandler<string> OnNewEntryAdded;
        public static void WriteLog(string message)
        {
            OnNewEntryAdded?.Invoke(null, message);
        }
    }
}
