using SuperSimpleTcp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace Shelf_Register
{
    public static class Network
    {
        public static string GetLocalIPAddress(NetworkInterfaceType _type)
        {
            string output = "";  // default output
            foreach (NetworkInterface item in NetworkInterface.GetAllNetworkInterfaces()) // Iterate over each network interface
            {  // Find the network interface which has been provided in the arguments, break the loop if found
                if (item.NetworkInterfaceType == _type && item.OperationalStatus == OperationalStatus.Up)
                {   // Fetch the properties of this adapter
                    IPInterfaceProperties adapterProperties = item.GetIPProperties();
                    // Check if the gateway adress exist, if not its most likley a virtual network or smth
                    if (adapterProperties.GatewayAddresses.FirstOrDefault() != null)
                    {   // Iterate over each available unicast adresses
                        foreach (UnicastIPAddressInformation ip in adapterProperties.UnicastAddresses)
                        {   // If the IP is a local IPv4 adress
                            if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                            {   // we got a match!
                                output = ip.Address.ToString();
                                break;  // break the loop!!
                            }
                        }
                    }
                }
                // Check if we got a result if so break this method
                if (output != "") { break; }
            }
            // Return results
            return output;
        }


        public static List<SimpleTcpClient> connectedTcpHosts = new List<SimpleTcpClient>();
        public static bool CreateConnect()
        {
            bool result = false;
            try
            {
                // instantiate
                // Stop scan another 

                int temp = Config.TcpHost.Count();
                for (int i = 0; i < temp; i++)
                {
                    Global.multiConnections = new SimpleTcpClient(Config.TcpHost[i]);
                    Global.multiConnections.Events.Connected += Connected;
                    //multiConnections.Events.Disconnected += Disconnected;
                    Global.multiConnections.Events.DataReceived += DataReceived;
                    //multiConnections.Events.DataSent += Events_DataSent;
                    Global.multiConnections.Connect();
                    Global.multiConnections.Send("ACTION_REGISTER_SHELF_END");
                    //Thread.Sleep(50);
                    //Global.multiConnections.Send("ACTION_REGISTER_SHELF_START");
                    connectedTcpHosts.Add(Global.multiConnections);

                }
                result = true;
            }
            catch (Exception)
            {
                //log error
            }

            return result;
        }

        public static bool CloseConnect()
        {
            bool result = false;
            try
            {
                // instantiate

                int temp = Config.TcpHost.Count();
                for (int i = 0; i < temp; i++)
                {
                    Global.multiConnections = new SimpleTcpClient(Config.TcpHost[i]);
                    Global.multiConnections.Events.Connected += Connected;
                    Global.multiConnections.Events.DataReceived += DataReceived;
                    Global.multiConnections.Connect();
                    Global.multiConnections.Send("ACTION_REGISTER_SHELF_END");
                    Global.multiConnections.Disconnect();

                }
                result = true;
            }
            catch (Exception)
            {
                //log error
            }

            return result;
        }

        private static void Connected(object sender, ConnectionEventArgs e)
        {
            Console.WriteLine($"*** Server {e.IpPort} connected");
        }

        private static void DataReceived(object sender, SuperSimpleTcp.DataReceivedEventArgs e)
        {
            Console.WriteLine($"[{e.IpPort}] {Encoding.UTF8.GetString(e.Data)}");
        }
    }
}
