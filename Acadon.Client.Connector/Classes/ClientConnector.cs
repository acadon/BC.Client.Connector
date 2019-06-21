using Acadon.Client.Connector.Helpers;
using Acadon.Client.Connector.Models;
using NLua;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows;

namespace Acadon.Client.Connector.Classes
{
    public class ClientConnector
    {
        WebServer webServer;

        public bool Initialize()
        {
            if (!MutexHelper.OpenOrCreateMutex())
            {
                if (!StartSystemService())
                    return false;
            }
            else
            {
                if (!StartService())
                    return false;

                MutexHelper.ExecuteOnMutexRelease(AddSystemService);
            }

            CleanupWorkspaceFolder();
            return true;
        }

        private bool StartSystemService()
        {
            try
            {
                var port = Consts.SessionPortRangeStart + Process.GetCurrentProcess().SessionId;
                webServer = new WebServer(SendResponse, string.Format(Consts.ServicePrefix, Consts.BrokerPort), string.Format(Consts.ServicePrefix, port));
                webServer.Run();
                LogHelper.WriteLog(string.Format(Properties.Resources.StartedSystemServer, Process.GetCurrentProcess().SessionId));
                LogHelper.WriteLog(string.Format(Properties.Resources.StartedServer, port));
                return true;
            }
            catch (Exception)
            {
            }

            return false;
        }

        private bool StartService()
        {
            try
            {
                var port = Consts.SessionPortRangeStart + Process.GetCurrentProcess().SessionId;
                webServer = new WebServer(SendResponse, string.Format(Consts.ServicePrefix, port));
                webServer.Run();
                LogHelper.WriteLog(string.Format(Properties.Resources.StartedServer, port));
                return true;
            }
            catch (HttpListenerException e)
            {
                if (e.ErrorCode == 183)
                {
                    MessageBox.Show(Properties.Resources.OnlyOncePerSession, Consts.AppName);
                    App.Current.Shutdown(0);
                }
            }

            return false;
        }

        private void AddSystemService()
        {
            webServer.AddPrefix(string.Format(Consts.ServicePrefix, Consts.BrokerPort));
            LogHelper.WriteLog(Properties.Resources.BrokerServiceAdded);
        }

        private void CleanupWorkspaceFolder()
        {
            try
            {
                var tempPath = Path.Combine(Path.GetTempPath(), "ACC");
                if (Directory.Exists(tempPath))
                    Directory.Delete(tempPath, true);
            }
            catch { }
        }

        public static string SendResponse(HttpListenerRequest request)
        {
            if (request.HttpMethod != "POST")
                return string.Format("<HTML><BODY>{0}<br>{1}</BODY></HTML>", Consts.AppName, DateTime.Now);

            var streamReader = new StreamReader(request.InputStream);
            var inputString = streamReader.ReadToEnd();

            if (inputString == "BROKER")
                return ProcessBroker(request);

            if (inputString == "PING")
                return ProcessPing(request);

            return ProcessOperation(inputString);
        }

        private static string ProcessBroker(HttpListenerRequest request)
        {
            return "{ \"ResponseType\": \"System\", \"Port\": " + SessionIdHelper.GetClientSessionPort(request.RemoteEndPoint) + " }";
        }

        private static string ProcessPing(HttpListenerRequest request)
        {
            return "{ \"ResponseType\": \"System\", \"Port\": " + request.LocalEndPoint.Port + " }";
        }

        private static string ProcessOperation(string input)
        {
            try
            {
                var operationRequest = OperationRequest.ParseFromString(input);
                if (string.IsNullOrEmpty(operationRequest.OperationName))
                    throw new ArgumentException("OperationName not set.");

                if (string.IsNullOrEmpty(operationRequest.CompanyName))
                    throw new ArgumentException("CompanyName not set.");

                if (string.IsNullOrEmpty(operationRequest.SerialNo))
                    throw new ArgumentException("SerialNo not set.");

                if (string.IsNullOrEmpty(operationRequest.Script))
                    throw new ArgumentException("Script not set.");

                LogHelper.WriteLog(string.Format(Properties.Resources.ExecutingOperation, operationRequest.OperationName));

                if (!PermissionHelper.CheckPermission(operationRequest))
                {
                    LogHelper.WriteLog(string.Format(Properties.Resources.ExecutionDenied, operationRequest.OperationName));
                    return (new ErrorResponse("Execution denied.")).ToString();
                }

                var tempPath = Path.Combine(Path.GetTempPath(), "ACC");
                if (!Directory.Exists(tempPath))
                    Directory.CreateDirectory(tempPath);

                tempPath = Path.Combine(tempPath, Guid.NewGuid().ToString());
                Directory.CreateDirectory(tempPath);
                operationRequest.SaveAllFiles(tempPath);

                var lua = new Lua();
                lua.State.Encoding = Encoding.UTF8;
                lua.LoadCLRPackage();

                foreach (var parameter in operationRequest.Parameters ?? Enumerable.Empty<Parameter>())
                    lua[parameter.Name] = parameter.Value;

                lua["WorkspaceFolder"] = tempPath;

                var operationResponse = new OperationResponse();
                lua.RegisterFunction("AddFile", operationResponse, operationResponse.GetType().GetMethod("AddFile"));
                lua.RegisterFunction("AddVariable", operationResponse, operationResponse.GetType().GetMethod("AddVariable"));

                lua.DoString(operationRequest.Script);

                LogHelper.WriteLog(string.Format(Properties.Resources.OperationExecuted, operationRequest.OperationName));
                return operationResponse.ToString();
            }
            catch (Exception e)
            {
                LogHelper.WriteLog(string.Format(Properties.Resources.ErrorOccured, e.Message));
                var errorResponse = new ErrorResponse(e.Message);
                return errorResponse.ToString();
            }
        }
    }
}
