using Phrenapates.Controllers.Api.ProtocolHandlers;
using Serilog;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text.Json;

namespace Phrenapates.Utils
{
    public class Config : Singleton<Config>
    {
        public static string ConfigPath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config.json");
        
        public string IRCAddress { get; set; } = "127.0.0.1";
        public int IRCPort { get; set; } = 6667;

        public string VersionId { get; set; } = "r72_vofckiwbubkg62digzhl";

        public static void Load()
        {
            if (!File.Exists(ConfigPath))
            {
                Instance.IRCAddress = GetLocalIPv4(NetworkInterfaceType.Wireless80211) == string.Empty ? GetLocalIPv4(NetworkInterfaceType.Ethernet) : GetLocalIPv4(NetworkInterfaceType.Wireless80211);
                Save();
            }

            string json = File.ReadAllText(ConfigPath);
            Instance = JsonSerializer.Deserialize<Config>(json);
            
            Log.Debug($"Config loaded");
        }

        public static void Save()
        {
            File.WriteAllText(ConfigPath, JsonSerializer.Serialize(Instance));

            Log.Debug($"Config saved");
        }

        public static string GetLocalIPv4(NetworkInterfaceType _type)
        {
            string output = "";
            foreach (NetworkInterface item in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (item.NetworkInterfaceType == _type && item.OperationalStatus == OperationalStatus.Up)
                {
                    foreach (UnicastIPAddressInformation ip in item.GetIPProperties().UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            output = ip.Address.ToString();
                        }
                    }
                }
            }
            return output;
        }
    }
}
