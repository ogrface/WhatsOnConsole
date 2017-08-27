using System;
using System.Configuration;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WhatsOn.DataModel;
using RestSharp;
using Newtonsoft.Json;
using System.Xml.Serialization;

namespace WhatsOnConsole
{
    class Program
    {
        private const string tvGuideBaseUrl = "http://mobilelistings.tvguide.com/Listingsweb/ws/rest";
        private const string tvGuideProvidersUrl = tvGuideBaseUrl + "/serviceproviders/zipcode/58103";
        private const string tvGuideSchedulesUrl = tvGuideBaseUrl + "/schedules";
        private const string tvGuideProgramBaseUrl = "http://mapi.tvguide.com/listings/details?program=";
        private static readonly DateTime unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        static void Main(string[] args)
        {
            Run();
        }

        static void Run()
        {
            string providerUrlPart;

            var client = new RestClient(tvGuideProvidersUrl);
            var request = new RestRequest(Method.GET);
            var response = client.Execute(request);
            AppSettingsReader settingsReader = new AppSettingsReader();

            string providerType = (string)settingsReader.GetValue("providerType", typeof(string));
            string providerName = (string)settingsReader.GetValue("providerName", typeof(string));
            string deviceName = (string)settingsReader.GetValue("deviceName", typeof(string));
            string scannedChannels = (string)settingsReader.GetValue("channels", typeof(string));

            string[] scannedChannelList = scannedChannels.Split(',');

            if (response.ResponseStatus == ResponseStatus.Completed)
            {
                IEnumerable<TVGuideProviderData.RootObject> providers = 
                    (IEnumerable<TVGuideProviderData.RootObject>)JsonConvert.DeserializeObject(response.Content, typeof(List<TVGuideProviderData.RootObject>));

                TVGuideProviderData.RootObject provider = 
                    (from p in providers
                    where p.Type == providerType
                    && p.Name.Contains(providerName)
                    select p).First();

                IEnumerable<TVGuideProviderData.Device> providerDevices = provider.Devices;
                int providerDevice = 
                    (from d in providerDevices
                    where d.DeviceName.Contains(deviceName)
                    select d).First().DeviceFlag;

                providerUrlPart = provider.Id.ToString() + "." + providerDevice.ToString();

                string startUrlPart = GetCurrentTimeInMilliseconds();
                startUrlPart = startUrlPart.Substring(0, startUrlPart.Length - 3);
                client = new RestClient(tvGuideSchedulesUrl + "/" + providerUrlPart + "/start/" + startUrlPart + "/duration/240");
                response = client.Execute(new RestRequest(Method.GET));

                if (response.ResponseStatus == ResponseStatus.Completed)
                {
                    IEnumerable<TVGuideListingsData.RootObject> listings = 
                        (IEnumerable<TVGuideListingsData.RootObject>)JsonConvert.DeserializeObject(response.Content, typeof(IEnumerable<TVGuideListingsData.RootObject>));

                    foreach (var l in listings)
                    {
                        if (scannedChannelList.Contains(l.Channel.Number))
                        {
                            Console.WriteLine(l.Channel.Number + ", " + l.Channel.FullName + ", " + l.Channel.Name);
                        }
                    }

                    tv tv = new tv();
                    tv.channel = new tvChannel[listings.Count()];
                    int i = 0;

                    foreach (var listing in listings)
                    {
                        if (scannedChannelList.Contains(listing.Channel.Number))
                        {
                            var tvChannelDisplayName = new tvChannelDisplayname()
                            {
                                lang = "en",
                                Value = listing.Channel.FullName
                            };

                            var tvChannel = new tvChannel()
                            {
                                id = listing.Channel.Name,
                                displayname = tvChannelDisplayName,
                                url = "http://www.tvguide.com.FargoND-OTA"
                            };

                            tv.channel[i] = tvChannel;
                            i++;
                        }
                    }

                    XmlSerializer serializer = new XmlSerializer(typeof(tv));
                    serializer.Serialize(Console.Out, tv);

                }
                Console.ReadLine();
            }
        }

        private static string GetCurrentTimeInMilliseconds()
        {
            DateTime posixTime = new DateTime(1970, 01, 01);
            DateTime currentTime = DateTime.Now;
            return ((currentTime - posixTime).Ticks / 10000).ToString();
        }
    }
}
