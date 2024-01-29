using System.Net;
using System.Net.Sockets;

namespace Ethereal.FAF.UI.Client.Infrastructure.Helper
{
    internal static class NetworkHelper
    {
        /// <summary>
        /// Retrieve free port
        /// </summary>
        /// <returns>Port number</returns>
        public static int GetAvailablePort()
        {
            using var udp = new UdpClient(0, AddressFamily.InterNetwork);
            int port = ((IPEndPoint)udp.Client.LocalEndPoint).Port;
            return port;
        }
    }
}
