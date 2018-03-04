using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhatsOnConsole
{
    public class EnvironmentSettings
    {
        public string ProviderType { get; set; }

        public string ProviderName { get; set; }

        public string DeviceName { get; set; }

        public IEnumerable<string> Channels { get; set; }

        public static EnvironmentSettings RetrieveSettings(AppSettingsReader reader)
        {
            string scannedChannels = (string)reader.GetValue("channels", typeof(string));
        
            return new EnvironmentSettings()
            {
                ProviderType = (string)reader.GetValue("providerType", typeof(string)),
                ProviderName = (string)reader.GetValue("providerName", typeof(string)),
                DeviceName = (string)reader.GetValue("deviceName", typeof(string)),
                Channels = scannedChannels.Split(',').ToList()
            };
        
        }
    }
}