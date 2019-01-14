using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Universal.Edge
{
    public class DeviceHelper
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct IP_ADAPTER_INFO
        {
            const int MAX_ADAPTER_ADDRESS_LENGTH = 8;
            const int MAX_ADAPTER_NAME_LENGTH = 256;
            const int MAX_ADAPTER_DESCRIPTION_LENGTH = 128;
            public IntPtr Next;
            public Int32 ComboIndex;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_ADAPTER_NAME_LENGTH + 4)]
            public string AdapterName;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_ADAPTER_DESCRIPTION_LENGTH + 4)]
            public string AdapterDescription;

            public UInt32 AddressLength;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_ADAPTER_ADDRESS_LENGTH)]
            public byte[] Address;

            public Int32 Index;
            public UInt32 Type;
            public UInt32 DhcpEnabled;
            public IntPtr CurrentIpAddress;
            //public IP_ADDR_STRING IpAddressList;
            //public IP_ADDR_STRING GatewayList;
            //public IP_ADDR_STRING DhcpServer;
            public bool HaveWins;
            //public IP_ADDR_STRING PrimaryWinsServer;
            //public IP_ADDR_STRING SecondaryWinsServer;
            public Int32 LeaseObtained;
            public Int32 LeaseExpires;
        }
        [DllImport("iphlpapi.dll", CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern int GetAdaptersInfo(IntPtr pAdapterInfo, ref Int64 pBufOutLen);
        const int MIB_IF_TYPE_OTHER = 71;
        const int ERROR_BUFFER_OVERFLOW = 111;

        public static string TryGetDeviceMacAddress()
        {
            try
            {
                return GetDeviceMacAddress();
            }
            catch (Exception ex)
            {

            }

            return null;
        }
        public static string GetDeviceMacAddress()
        {

            long structSize = Marshal.SizeOf(typeof(IP_ADAPTER_INFO));
            IntPtr pArray = Marshal.AllocHGlobal(new IntPtr(structSize));

            int ret = GetAdaptersInfo(pArray, ref structSize);

            if (ret == ERROR_BUFFER_OVERFLOW) // ERROR_BUFFER_OVERFLOW == 111
            {
                // Buffer was too small, reallocate the correct size for the buffer.
                pArray = Marshal.ReAllocHGlobal(pArray, new IntPtr(structSize));

                ret = GetAdaptersInfo(pArray, ref structSize);
            }

            if (ret == 0)
            {
                // Call Succeeded
                IntPtr pEntry = pArray;
                List<string> t = new List<string>();
                string macAddress = "";
                do
                {
                    // Retrieve the adapter info from the memory address
                    var entry = (IP_ADAPTER_INFO) Marshal.PtrToStructure(pEntry, typeof(IP_ADAPTER_INFO));
                    t.Add(string.Join(":",
                        Enumerable.Range(0, (int) entry.AddressLength)
                            .Select(s => string.Format("{0:X2}", entry.Address[s]))));
                    if (entry.Type == MIB_IF_TYPE_OTHER)
                    {
                        macAddress = string.Join(":",
                            Enumerable.Range(0, (int) entry.AddressLength)
                                .Select(s => string.Format("{0:x2}", entry.Address[s])));
                        break;
                    }
                    pEntry = entry.Next;
                } while (pEntry != IntPtr.Zero);

                Marshal.FreeHGlobal(pArray);
                return macAddress;
            }
            //else
            //{
            //    Marshal.FreeHGlobal(pArray);
            //    throw new InvalidOperationException("GetAdaptersInfo failed: " + ret);
            //}
            return null;
        }
    }
}