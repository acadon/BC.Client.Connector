using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Net;
using System.Diagnostics;


namespace Acadon.Client.Connector.Helpers
{
    class TcpConnectionHelper
    {
        // The version of IP used by the TCP/UDP endpoint. AF_INET is used for IPv4. 
        private const int AF_INET = 2;
        private const int AF_INET6 = 23;

        // The GetExtendedTcpTable function retrieves a table that contains a list of 
        // TCP endpoints available to the application. Decorating the function with 
        // DllImport attribute indicates that the attributed method is exposed by an 
        // unmanaged dynamic-link library 'iphlpapi.dll' as a static entry point. 
        [DllImport("iphlpapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern uint GetExtendedTcpTable(IntPtr pTcpTable, ref int pdwSize,
            bool bOrder, int ulAf, TcpTableClass tableClass, uint reserved = 0);

        // The GetExtendedUdpTable function retrieves a table that contains a list of 
        // UDP endpoints available to the application. Decorating the function with 
        // DllImport attribute indicates that the attributed method is exposed by an 
        // unmanaged dynamic-link library 'iphlpapi.dll' as a static entry point. 
        [DllImport("iphlpapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern uint GetExtendedUdpTable(IntPtr pUdpTable, ref int pdwSize,
            bool bOrder, int ulAf, UdpTableClass tableClass, uint reserved = 0);

        /// <summary> 
        /// This function reads and parses the active TCP socket connections available 
        /// and stores them in a list. 
        /// </summary> 
        /// <returns> 
        /// It returns the current set of TCP socket connections which are active. 
        /// </returns> 
        /// <exception cref="OutOfMemoryException"> 
        /// This exception may be thrown by the function Marshal.AllocHGlobal when there 
        /// is insufficient memory to satisfy the request. 
        /// </exception> 
        public static List<TcpProcessRecord> GetAllTcpConnections()
        {
            int bufferSize = 0;
            List<TcpProcessRecord> tcpTableRecords = new List<TcpProcessRecord>();

            // Getting the size of TCP table, that is returned in 'bufferSize' variable. 
            uint result = GetExtendedTcpTable(IntPtr.Zero, ref bufferSize, true, AF_INET,
                TcpTableClass.TCP_TABLE_OWNER_PID_ALL);

            // Allocating memory from the unmanaged memory of the process by using the 
            // specified number of bytes in 'bufferSize' variable. 
            IntPtr tcpTableRecordsPtr = Marshal.AllocHGlobal(bufferSize);

            try
            {
                // The size of the table returned in 'bufferSize' variable in previous 
                // call must be used in this subsequent call to 'GetExtendedTcpTable' 
                // function in order to successfully retrieve the table. 
                result = GetExtendedTcpTable(tcpTableRecordsPtr, ref bufferSize, true,
                    AF_INET, TcpTableClass.TCP_TABLE_OWNER_PID_ALL);

                // Non-zero value represent the function 'GetExtendedTcpTable' failed, 
                // hence empty list is returned to the caller function. 
                if (result != 0)
                    return new List<TcpProcessRecord>();

                // Marshals data from an unmanaged block of memory to a newly allocated 
                // managed object 'tcpRecordsTable' of type 'MIB_TCPTABLE_OWNER_PID' 
                // to get number of entries of the specified TCP table structure. 
                MIB_TCPTABLE_OWNER_PID tcpRecordsTable = (MIB_TCPTABLE_OWNER_PID)
                                        Marshal.PtrToStructure(tcpTableRecordsPtr,
                                        typeof(MIB_TCPTABLE_OWNER_PID));
                IntPtr tableRowPtr = (IntPtr)((long)tcpTableRecordsPtr +
                                        Marshal.SizeOf(tcpRecordsTable.dwNumEntries));

                // Reading and parsing the TCP records one by one from the table and 
                // storing them in a list of 'TcpProcessRecord' structure type objects. 
                for (int row = 0; row < tcpRecordsTable.dwNumEntries; row++)
                {
                    MIB_TCPROW_OWNER_PID tcpRow = (MIB_TCPROW_OWNER_PID)Marshal.
                        PtrToStructure(tableRowPtr, typeof(MIB_TCPROW_OWNER_PID));
                    tcpTableRecords.Add(new TcpProcessRecord(
                                            new IPAddress(tcpRow.localAddr),
                                            new IPAddress(tcpRow.remoteAddr),
                                            BitConverter.ToUInt16(new byte[2] {
                                            tcpRow.localPort[1],
                                            tcpRow.localPort[0] }, 0),
                                            BitConverter.ToUInt16(new byte[2] {
                                            tcpRow.remotePort[1],
                                            tcpRow.remotePort[0] }, 0),
                                            tcpRow.owningPid, tcpRow.state));
                    tableRowPtr = (IntPtr)((long)tableRowPtr + Marshal.SizeOf(tcpRow));
                }
            }
            catch (OutOfMemoryException outOfMemoryException)
            {
                MessageBox.Show(outOfMemoryException.Message, "Out Of Memory",
                    MessageBoxButtons.OK, MessageBoxIcon.Stop);
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "Exception",
                    MessageBoxButtons.OK, MessageBoxIcon.Stop);
            }
            finally
            {
                Marshal.FreeHGlobal(tcpTableRecordsPtr);
            }
            return tcpTableRecords != null ? tcpTableRecords.Distinct()
                .ToList<TcpProcessRecord>() : new List<TcpProcessRecord>();
        }

        public static List<TcpProcessRecord> GetAllTcpConnections6()
        {
            int bufferSize = 0;
            List<TcpProcessRecord> tcpTableRecords = new List<TcpProcessRecord>();

            // Getting the size of TCP table, that is returned in 'bufferSize' variable. 
            uint result = GetExtendedTcpTable(IntPtr.Zero, ref bufferSize, true, AF_INET6,
                TcpTableClass.TCP_TABLE_OWNER_PID_ALL);

            // Allocating memory from the unmanaged memory of the process by using the 
            // specified number of bytes in 'bufferSize' variable. 
            IntPtr tcpTableRecordsPtr = Marshal.AllocHGlobal(bufferSize);

            try
            {
                // The size of the table returned in 'bufferSize' variable in previous 
                // call must be used in this subsequent call to 'GetExtendedTcpTable' 
                // function in order to successfully retrieve the table. 
                result = GetExtendedTcpTable(tcpTableRecordsPtr, ref bufferSize, true,
                    AF_INET6, TcpTableClass.TCP_TABLE_OWNER_PID_ALL);

                // Non-zero value represent the function 'GetExtendedTcpTable' failed, 
                // hence empty list is returned to the caller function. 
                if (result != 0)
                    return new List<TcpProcessRecord>();

                // Marshals data from an unmanaged block of memory to a newly allocated 
                // managed object 'tcpRecordsTable' of type 'MIB_TCPTABLE_OWNER_PID' 
                // to get number of entries of the specified TCP table structure. 
                MIB_TCP6TABLE_OWNER_PID tcpRecordsTable = (MIB_TCP6TABLE_OWNER_PID)
                                        Marshal.PtrToStructure(tcpTableRecordsPtr,
                                        typeof(MIB_TCP6TABLE_OWNER_PID));
                IntPtr tableRowPtr = (IntPtr)((long)tcpTableRecordsPtr +
                                        Marshal.SizeOf(tcpRecordsTable.dwNumEntries));

                // Reading and parsing the TCP records one by one from the table and 
                // storing them in a list of 'TcpProcessRecord' structure type objects. 
                for (int row = 0; row < tcpRecordsTable.dwNumEntries; row++)
                {
                    MIB_TCP6ROW_OWNER_PID tcpRow = (MIB_TCP6ROW_OWNER_PID)Marshal.
                        PtrToStructure(tableRowPtr, typeof(MIB_TCP6ROW_OWNER_PID));
                    tcpTableRecords.Add(new TcpProcessRecord(
                                            new IPAddress(tcpRow.localAddr),
                                            new IPAddress(tcpRow.remoteAddr),
                                            BitConverter.ToUInt16(new byte[2] {
                                            tcpRow.localPort[1],
                                            tcpRow.localPort[0] }, 0),
                                            BitConverter.ToUInt16(new byte[2] {
                                            tcpRow.remotePort[1],
                                            tcpRow.remotePort[0] }, 0),
                                            (int)tcpRow.ProcessId, 0));
                    tableRowPtr = (IntPtr)((long)tableRowPtr + Marshal.SizeOf(tcpRow));
                }
            }
            catch (OutOfMemoryException outOfMemoryException)
            {
                MessageBox.Show(outOfMemoryException.Message, "Out Of Memory",
                    MessageBoxButtons.OK, MessageBoxIcon.Stop);
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "Exception",
                    MessageBoxButtons.OK, MessageBoxIcon.Stop);
            }
            finally
            {
                Marshal.FreeHGlobal(tcpTableRecordsPtr);
            }
            return tcpTableRecords != null ? tcpTableRecords.Distinct()
                .ToList<TcpProcessRecord>() : new List<TcpProcessRecord>();
        }
    }

    // Enum to define the set of values used to indicate the type of table returned by  
    // calls made to the function 'GetExtendedTcpTable'. 
    public enum TcpTableClass
    {
        TCP_TABLE_BASIC_LISTENER,
        TCP_TABLE_BASIC_CONNECTIONS,
        TCP_TABLE_BASIC_ALL,
        TCP_TABLE_OWNER_PID_LISTENER,
        TCP_TABLE_OWNER_PID_CONNECTIONS,
        TCP_TABLE_OWNER_PID_ALL,
        TCP_TABLE_OWNER_MODULE_LISTENER,
        TCP_TABLE_OWNER_MODULE_CONNECTIONS,
        TCP_TABLE_OWNER_MODULE_ALL
    }

    // Enum to define the set of values used to indicate the type of table returned by calls 
    // made to the function GetExtendedUdpTable. 
    public enum UdpTableClass
    {
        UDP_TABLE_BASIC,
        UDP_TABLE_OWNER_PID,
        UDP_TABLE_OWNER_MODULE
    }

    // Enum for different possible states of TCP connection 
    public enum MibTcpState
    {
        CLOSED = 1,
        LISTENING = 2,
        SYN_SENT = 3,
        SYN_RCVD = 4,
        ESTABLISHED = 5,
        FIN_WAIT1 = 6,
        FIN_WAIT2 = 7,
        CLOSE_WAIT = 8,
        CLOSING = 9,
        LAST_ACK = 10,
        TIME_WAIT = 11,
        DELETE_TCB = 12,
        NONE = 0
    }

    /// <summary> 
    /// The structure contains information that describes an IPv4 TCP connection with  
    /// IPv4 addresses, ports used by the TCP connection, and the specific process ID 
    /// (PID) associated with connection. 
    /// </summary> 
    [StructLayout(LayoutKind.Sequential)]
    public struct MIB_TCPROW_OWNER_PID
    {
        public MibTcpState state;
        public uint localAddr;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] localPort;
        public uint remoteAddr;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] remotePort;
        public int owningPid;
    }

    /// <summary> 
    /// The structure contains a table of process IDs (PIDs) and the IPv4 TCP links that  
    /// are context bound to these PIDs. 
    /// </summary> 
    [StructLayout(LayoutKind.Sequential)]
    public struct MIB_TCPTABLE_OWNER_PID
    {
        public uint dwNumEntries;
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.Struct,
            SizeConst = 1)]
        public MIB_TCPROW_OWNER_PID[] table;
    }

    /// <summary> 
    /// This class provides access an IPv4 TCP connection addresses and ports and its 
    /// associated Process IDs and names. 
    /// </summary> 
    [StructLayout(LayoutKind.Sequential)]
    public class TcpProcessRecord
    {
        [DisplayName("Local Address")]
        public IPAddress LocalAddress { get; set; }
        [DisplayName("Local Port")]
        public ushort LocalPort { get; set; }
        [DisplayName("Remote Address")]
        public IPAddress RemoteAddress { get; set; }
        [DisplayName("Remote Port")]
        public ushort RemotePort { get; set; }
        [DisplayName("State")]
        public MibTcpState State { get; set; }
        [DisplayName("Process ID")]
        public int ProcessId { get; set; }
        [DisplayName("Process Name")]
        public string ProcessName { get; set; }

        public TcpProcessRecord(IPAddress localIp, IPAddress remoteIp, ushort localPort,
            ushort remotePort, int pId, MibTcpState state)
        {
            LocalAddress = localIp;
            RemoteAddress = remoteIp;
            LocalPort = localPort;
            RemotePort = remotePort;
            State = state;
            ProcessId = pId;
            // Getting the process name associated with a process id. 
            if (Process.GetProcesses().Any(process => process.Id == pId))
            {
                ProcessName = Process.GetProcessById(ProcessId).ProcessName;
            }
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MIB_TCP6ROW_OWNER_PID
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public byte[] localAddr;
        public uint localScopeId;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] localPort;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public byte[] remoteAddr;
        public uint remoteScopeId;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] remotePort;
        public uint state;
        public uint owningPid;

        public uint ProcessId
        {
            get { return owningPid; }
        }

        public long LocalScopeId
        {
            get { return localScopeId; }
        }

        public IPAddress LocalAddress
        {
            get { return new IPAddress(localAddr, LocalScopeId); }
        }

        public ushort LocalPort
        {
            get { return BitConverter.ToUInt16(localPort.Take(2).Reverse().ToArray(), 0); }
        }

        public long RemoteScopeId
        {
            get { return remoteScopeId; }
        }

        public IPAddress RemoteAddress
        {
            get { return new IPAddress(remoteAddr, RemoteScopeId); }
        }

        public ushort RemotePort
        {
            get { return BitConverter.ToUInt16(remotePort.Take(2).Reverse().ToArray(), 0); }
        }

        public MIB_TCP_STATE State
        {
            get { return (MIB_TCP_STATE)state; }
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MIB_TCP6TABLE_OWNER_PID
    {
        public uint dwNumEntries;
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.Struct, SizeConst = 1)]
        public MIB_TCP6ROW_OWNER_PID[] table;
    }

    public enum MIB_TCP_STATE
    {
        MIB_TCP_STATE_CLOSED = 1,
        MIB_TCP_STATE_LISTEN = 2,
        MIB_TCP_STATE_SYN_SENT = 3,
        MIB_TCP_STATE_SYN_RCVD = 4,
        MIB_TCP_STATE_ESTAB = 5,
        MIB_TCP_STATE_FIN_WAIT1 = 6,
        MIB_TCP_STATE_FIN_WAIT2 = 7,
        MIB_TCP_STATE_CLOSE_WAIT = 8,
        MIB_TCP_STATE_CLOSING = 9,
        MIB_TCP_STATE_LAST_ACK = 10,
        MIB_TCP_STATE_TIME_WAIT = 11,
        MIB_TCP_STATE_DELETE_TCB = 12
    }
}