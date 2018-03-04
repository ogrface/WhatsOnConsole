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

        void Main(string[] args)
        {
            Run();
        }

        void Run()
        {
            EnvironmentSettings settings = EnvironmentSettings.RetrieveSettings(new AppSettingsReader());
            var client = new RestClient(tvGuideProvidersUrl);
            var request = new RestRequest(Method.GET);
            var response = client.Execute(request);
            
            if (response.ResponseStatus == ResponseStatus.Completed)
            {
                IEnumerable<TVGuideProviderData.RootObject> providers = 
                    (IEnumerable<TVGuideProviderData.RootObject>)JsonConvert.DeserializeObject(response.Content, typeof(List<TVGuideProviderData.RootObject>));

                TVGuideProviderData.RootObject provider = 
                    (from p in providers
                    where p.Type == settings.ProviderType
                    && p.Name.Contains(settings.ProviderName)
                    select p).First();

                IEnumerable<TVGuideProviderData.Device> providerDevices = provider.Devices;
                int providerDevice = 
                    (from d in providerDevices
                    where d.DeviceName.Contains(settings.DeviceName)
                    select d).First().DeviceFlag;

                string providerUrlPart = provider.Id.ToString() + "." + providerDevice.ToString();

                string startUrlPart = Utilities.GetCurrentTimeInMilliseconds();
                startUrlPart = startUrlPart.Substring(0, startUrlPart.Length - 3);
                client = new RestClient(tvGuideSchedulesUrl + "/" + providerUrlPart + "/start/" + startUrlPart + "/duration/240");
                response = client.Execute(new RestRequest(Method.GET));

                if (response.ResponseStatus == ResponseStatus.Completed)
                {
                    IEnumerable<TVGuideListingsData.RootObject> listings = 
                        (IEnumerable<TVGuideListingsData.RootObject>)JsonConvert.DeserializeObject(response.Content, typeof(IEnumerable<TVGuideListingsData.RootObject>));

                    tv tv = MapTVGuideDataToXMLtv(settings.Channels, listings);

                    StreamWriter writer = new StreamWriter("C:\\temp\\xmltv.xml");
                    XmlSerializer serializer = new XmlSerializer(typeof(tv));
                    serializer.Serialize(writer, tv);

                }
                Console.ReadLine();
            }
        }

        private static tv MapTVGuideDataToXMLtv(IEnumerable<string> scannedChannelList, IEnumerable<TVGuideListingsData.RootObject> listings)
        {
            tv tv = new tv()
            {
                generatorinfoname = "WhatsOnConsole",
                generatorinfourl = "https://github.com/ogrface/WhatsOnConsole"
            };

            tv.channel = new tvChannel[listings.Count()];
            int i = 0;

            foreach (var listing in listings)
            {
                if (scannedChannelList.Contains(listing.Channel.Number))
                {
                    var tvChannel = new tvChannel()
                    {
                        id = listing.Channel.Name,
                        displayname = new tvChannelDisplayname()
                        {
                            lang = "en",
                            Value = listing.Channel.FullName
                        },
                        url = "http://www.tvguide.com.FargoND-OTA"
                    };

                    tv.channel[i] = tvChannel;

                    i++;
                }
            }

            foreach (var listing in listings)
            {
                if (scannedChannelList.Contains(listing.Channel.Number))
                {
                    int j = 0;
                    tv.programme = new tvProgramme[listing.ProgramSchedules.Count];

                    foreach (var schedule in listing.ProgramSchedules)
                    {
                        var tvProgramme = new tvProgramme()
                        {
                            start = schedule.StartTime.ToString(),
                            stop = schedule.EndTime.ToString(),
                            channel = listing.Channel.Name,
                        };

                        var tvProgrammeTitle = new tvProgrammeTitle()
                        {
                            lang = "en",
                            Value = schedule.Title
                        };

                        var tvProgrammeSubTitle = new tvProgrammeSubtitle()
                        {
                            lang = "en",
                            Value = schedule.EpisodeTitle
                        };

                        var tvProgrammeDesc = new tvProgrammeDesc()
                        {
                            lang = "en",
                            Value = schedule.CopyText
                        };

                        var episodeNum = new tvProgrammeEpisodenum()
                        {
                            system = "xmltv_ns",
                            Value = $"{schedule.TVObject?.SeasonNumber}.{schedule.TVObject?.EpisodeNumber}."
                        };

                        var itemList = new List<object>();
                        itemList.Add(tvProgrammeTitle);
                        itemList.Add(tvProgrammeSubTitle);
                        itemList.Add(tvProgrammeDesc);
                        tvProgramme.Items = itemList.ToArray();

                        tv.programme[j] = tvProgramme;

                        j++;
                    }
                }
            }
            return tv;
        }
    }
}
