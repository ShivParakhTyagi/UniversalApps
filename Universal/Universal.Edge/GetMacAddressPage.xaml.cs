using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using Windows.Networking.Connectivity;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Universal.Edge
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class GetMacAddressPage : Page
    {
        public GetMacAddressPage()
        {
            this.InitializeComponent();
        }

        private void TryMeButton_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                //var inf = AdaptersHelper.GetAdapters().Where(x => x.Type != "Ethernet");
                //NetworkInterfacesListView.ItemsSource = inf;

                resultTextBox.Text = DeviceHelper.TryGetDeviceMacAddress() ?? "";
            }
            catch (Exception ex)
            {

            }

            //resultTextBox.Text = GetMacAddress().ToString();
        }


        /// Gets the MAC address of the current PC.
        /// </summary>
        /// <returns></returns>
        public static PhysicalAddress GetMacAddress()
        {

            foreach (var nic in NetworkInformation.GetConnectionProfiles())
            {
                // Only consider Ethernet network interfaces
                //if (nic.NetworkInterfaceType == NetworkInterfaceType.Ethernet &&
                //    nic.OperationalStatus == OperationalStatus.Up)
                //{
                //    return nic.GetPhysicalAddress();
                //}
            }

            return null;
        }

        private void NetworkInterfacesListView_OnItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is NetworkInterface networkInterface)
            {
                var physicalAddress = networkInterface.GetPhysicalAddress();
                resultTextBox.Text = physicalAddress.ToString();
            }

        }

    }
/*

    public static class AdaptersHelper
    {
        const int MAX_ADAPTER_DESCRIPTION_LENGTH = 128;
        const int ERROR_BUFFER_OVERFLOW = 111;
        const int MAX_ADAPTER_NAME_LENGTH = 256;
        const int MAX_ADAPTER_ADDRESS_LENGTH = 8;
        const int MIB_IF_TYPE_OTHER = 71;
        const int MIB_IF_TYPE_ETHERNET = 6;
        const int MIB_IF_TYPE_TOKENRING = 9;
        const int MIB_IF_TYPE_FDDI = 15;
        const int MIB_IF_TYPE_PPP = 23;
        const int MIB_IF_TYPE_LOOPBACK = 24;
        const int MIB_IF_TYPE_SLIP = 28;

        [DllImport("iphlpapi.dll", CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern int GetAdaptersInfo(IntPtr pAdapterInfo, ref Int64 pBufOutLen);

        public static List<AdapterInfo> GetAdapters()
        {
            var adapters = new List<AdapterInfo>();

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

                do
                {
                    var adapter = new AdapterInfo();

                    // Retrieve the adapter info from the memory address
                    var entry = (IP_ADAPTER_INFO) Marshal.PtrToStructure(pEntry, typeof(IP_ADAPTER_INFO));

                    // Adapter Type
                    switch (entry.Type)
                    {
                        case MIB_IF_TYPE_ETHERNET:
                            adapter.Type = "Ethernet";
                            break;
                        case MIB_IF_TYPE_TOKENRING:
                            adapter.Type = "Token Ring";
                            break;
                        case MIB_IF_TYPE_FDDI:
                            adapter.Type = "FDDI";
                            break;
                        case MIB_IF_TYPE_PPP:
                            adapter.Type = "PPP";
                            break;
                        case MIB_IF_TYPE_LOOPBACK:
                            adapter.Type = "Loopback";
                            break;
                        case MIB_IF_TYPE_SLIP:
                            adapter.Type = "Slip";
                            break;
                        default:
                            adapter.Type = "Other/Unknown";
                            break;
                    } // switch

                    adapter.Name = entry.AdapterName;
                    adapter.Description = entry.AdapterDescription;
                    adapter.IP1 = entry.IpAddressList.IpAddress.Address.ToString();
                    adapter.IP2 = entry.DhcpServer.IpAddress.Address.ToString();
                    adapter.IP3 = entry.GatewayList.IpAddress.Address.ToString();
                    // MAC Address (data is in a byte[])
                    adapter.MAC = string.Join(":",
                        Enumerable.Range(0, (int) entry.AddressLength)
                            .Select(s => string.Format("{0:X2}", entry.Address[s])));

                    // Get next adapter (if any)

                    adapters.Add(adapter);

                    pEntry = entry.Next;
                } while (pEntry != IntPtr.Zero);

                Marshal.FreeHGlobal(pArray);
            }
            else
            {
                Marshal.FreeHGlobal(pArray);
                throw new InvalidOperationException("GetAdaptersInfo failed: " + ret);
            }

            return adapters;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct IP_ADAPTER_INFO
        {
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
            public IP_ADDR_STRING IpAddressList;
            public IP_ADDR_STRING GatewayList;
            public IP_ADDR_STRING DhcpServer;
            public bool HaveWins;
            public IP_ADDR_STRING PrimaryWinsServer;
            public IP_ADDR_STRING SecondaryWinsServer;
            public Int32 LeaseObtained;
            public Int32 LeaseExpires;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct IP_ADDR_STRING
        {
            public IntPtr Next;
            public IP_ADDRESS_STRING IpAddress;
            public IP_ADDRESS_STRING IpMask;
            public Int32 Context;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct IP_ADDRESS_STRING
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
            public string Address;
        }
    }

    public class AdapterInfo
    {
        public string Type { get; set; }

        public string Description { get; set; }

        public string Name { get; set; }

        public string MAC { get; set; }
        public List<string> IPs { get; set; }
        public string IP1 { get; set; }
        public string IP2 { get; set; }
        public string IP3 { get; set; }
    }*/
}
