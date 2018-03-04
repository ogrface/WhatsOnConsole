using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhatsOnConsole
{
    public static class Utilities
    {
        public static string GetCurrentTimeInMilliseconds()
        {
            DateTime posixTime = new DateTime(1970, 01, 01);
            DateTime currentTime = DateTime.Now;
            return ((currentTime - posixTime).Ticks / 10000).ToString();
        }
    }
}
