using System;
using System.Threading;

namespace Acadon.Client.Connector.Helpers
{
    public static class SingleInstanceHelper
    {
        private static Mutex mutex;
        public static bool IsFirstInsance()
        {
            var mutexName = @"Local\acadon_client.connector_SI";
            try
            {
                mutex = Mutex.OpenExisting(mutexName);
                GC.KeepAlive(mutex);
                return false;
            }
            catch (Exception ex)
            {
                mutex = new Mutex(true, mutexName);
                GC.KeepAlive(mutex);
                return true;
            }
        }
    }
}
