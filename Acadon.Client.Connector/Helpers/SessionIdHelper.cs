using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;

namespace Acadon.Client.Connector.Helpers
{
    public class SessionIdHelper
    {
        public static int GetClientSessionPort(IPEndPoint endPoint)
        {
            var sessionId = GetClientSessionId(endPoint);
            return Consts.SessionPortRangeStart + sessionId;
        }

        public static int GetClientSessionId(IPEndPoint endPoint)
        {
            List<TcpProcessRecord> connections = null;
            if (endPoint.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
                connections = TcpConnectionHelper.GetAllTcpConnections6();

            if (endPoint.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                connections = TcpConnectionHelper.GetAllTcpConnections();

            var clientConnection = connections.FirstOrDefault(c => c.LocalPort == endPoint.Port);

            if (clientConnection == null)
                return -1;

            var clientProcess = Process.GetProcessById(clientConnection.ProcessId);
            return clientProcess.SessionId;
        }

        public static int CurrentSessionId(IPEndPoint endPoint)
        {
            return Process.GetCurrentProcess().SessionId;
        }
    }
}
