using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WhatsOn.DataModel
{
    public class TVGuideProviderData
    {
        public TimeOffset timeOffset { get; private set; }
        public Device device { get; private set; }
        public RootObject rootObject { get; private set; }

        public TVGuideProviderData()
        {
            timeOffset = new TimeOffset();
            device = new Device();
            rootObject = new RootObject();
        }

        public class TimeOffset
        {
            public int End { get; set; }
            public int Offset { get; set; }
            public int Start { get; set; }
            public int PrimeTime { get; set; }
        }

        public class Device
        {
            public string DeviceName { get; set; }
            public int DeviceFlag { get; set; }
        }

        public class RootObject
        {
            public string City { get; set; }
            public int Id { get; set; }
            public string Name { get; set; }
            public string State { get; set; }
            public List<TimeOffset> TimeOffsets { get; set; }
            public string Type { get; set; }
            public List<Device> Devices { get; set; }
        }
    }
}