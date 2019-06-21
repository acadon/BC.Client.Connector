using System;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;

namespace Acadon.Client.Connector.Helpers
{
    public class MutexHelper
    {
        static Mutex mutex;
        static ManualResetEvent resetEvent = new ManualResetEvent(false);
        static Thread thread;

        public static bool OpenOrCreateMutex()
        {
            var mutexName = @"Global\acadon_client.connector";
            try
            {
                mutex = Mutex.OpenExisting(mutexName);
                GC.KeepAlive(mutex);
                return true;
            }
            catch (Exception ex)
            {
                var allowEveryoneRule =
                    new MutexAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null),
                        MutexRights.FullControl, AccessControlType.Allow);
                var securitySettings = new MutexSecurity();
                securitySettings.AddAccessRule(allowEveryoneRule);
                mutex = new Mutex(true, mutexName, out bool createdNew, securitySettings);
                GC.KeepAlive(mutex);
                return false;
            }
        }

        public static void ExecuteOnMutexRelease(Action executeOnRelease)
        {
            thread = new Thread(unused =>
            {
                try
                {
                    mutex.WaitOne();
                }
                catch (AbandonedMutexException)
                {
                    Console.WriteLine("AbandonedMutexException...");
                }

                Thread.Sleep(1000);
                executeOnRelease();
                (resetEvent as ManualResetEvent).WaitOne();
            });
            thread.Start();
        }

        public static void Dispose()
        {
            resetEvent.Set();
            thread?.Abort();
        }
    }
}
