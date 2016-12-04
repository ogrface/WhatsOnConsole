using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using WhatsOn.DataModel;
using RestSharp;
using Newtonsoft.Json;
using System.Xml;

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

            if (response.ResponseStatus == ResponseStatus.Completed)
            {
                IEnumerable<TVGuideProviderData.RootObject> providers = (IEnumerable<TVGuideProviderData.RootObject>)JsonConvert.DeserializeObject(response.Content, typeof(List<TVGuideProviderData.RootObject>));

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
                    IEnumerable<TVGuideListingsData.RootObject> listings = (IEnumerable<TVGuideListingsData.RootObject>)JsonConvert.DeserializeObject(response.Content, typeof(IEnumerable<TVGuideListingsData.RootObject>));
                    foreach (var l in listings)
                    {
                        Console.WriteLine(l.Channel.Number + ", " + l.Channel.FullName + ", " + l.Channel.Name);
                    }

                    //IEnumerable<TVGuideListingsData.ProgramSchedule> schedules = 
                    //    (from l in listings
                    //    where l.Channel.Name.Contains("KVLY")
                    //    select l).First().ProgramSchedules;

                    //foreach (var s in schedules)
                    //{
                    //    string startDateTime = unixEpoch.AddSeconds(s.StartTime).ToString();
                    //    string endDateTime = unixEpoch.AddSeconds(s.EndTime).ToString();
                    //    Console.WriteLine(s.Title + ", " + s.Rating + ", " + s.EpisodeTitle + ", " + startDateTime + ", " + endDateTime);
                    //}

                    tv lineupType = new xmltvLineupType();
                    lineupType.type = lineupTypeEnum.Analog;
                    

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
