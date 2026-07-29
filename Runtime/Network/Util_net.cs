using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public static partial class Util_net
{
    public static readonly IPAddress
        IP_3VE = IPAddress.Parse("141.94.223.114"),
        IP_GOOGLE = IPAddress.Parse("8.8.8.8");

    public const string
        DOMAIN_3VE = "www.shitstorm.ovh";

    public const ushort
        PORT_ARMA = 40000,
        PORT_PARAGON = 65000;

    public static readonly IPEndPoint
        end_ARMA = new(IP_3VE, PORT_ARMA),
        end_ARMA_loop = new(IPAddress.Loopback, PORT_ARMA),
        end_init = end_ARMA_loop;

    static IPAddress localIP;

    //----------------------------------------------------------------------------------------------------------

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void OnResetStatics()
    {
        localIP = null;
    }

    //----------------------------------------------------------------------------------------------------------

    public static IPAddress GetLocalIP()
    {
        if (localIP == null)
        {
            using Socket socket = new(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            socket.Connect(new IPEndPoint(IP_GOOGLE, PORT_ARMA));
            localIP = ((IPEndPoint)socket.LocalEndPoint).Address;
        }
        return localIP;
    }

    public static void LogIpConfig()
    {
        string hostname = Dns.GetHostName();
        StringBuilder log = new();

        log.AppendLine($"\"{hostname}\"");

        foreach (IPAddress ip in Dns.GetHostEntry(hostname).AddressList)
            if (ip.AddressFamily == AddressFamily.InterNetwork)
                log.AppendLine($"{ip} ({ip.AddressFamily})");

        Debug.Log(log.ToString()[..^1]);
    }
}