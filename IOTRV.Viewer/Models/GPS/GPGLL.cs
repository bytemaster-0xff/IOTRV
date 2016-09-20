using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IOTRV.Viewer.Models.GPS
{
    /* $GPGLL,2804.56795,N,08242.58974,W,195148.00,A,D*73 
     * http://aprs.gids.nl/nmea/#gll
     */

    public class GPGLL : INEMAMessage
    {
        public double Latitude {get; set;}
        public double Longitude { get; set; }
        public DateTime TimeStamp { get; set; }

        public void Parse(String message)
        {
            var parts = message.Split(',', '*');

            var latDeg = parts[1].Substring(0, 2);
            var latMin = parts[1].Substring(2);

            Latitude = Convert.ToDouble(latDeg) + (Convert.ToDouble(latMin) / Convert.ToDouble(60));
            if (parts[2] == "S")
                Latitude *= -1.0;

            var lonDeg = parts[3].Substring(0, 3);
            var lonMin = parts[3].Substring(3);

            Longitude = Convert.ToDouble(lonDeg) + (Convert.ToDouble(lonMin) / Convert.ToDouble(60));
            if (parts[4] == "W")
                Longitude *= -1.0;

            /*
            var timeStamp = message[1];
            var hh = parts[1].Substring(0, 2);
            var mm = parts[1].Substring(2, 2);
            var ss = parts[1].Substring(4, 2);
            var ms = parts[1].Substring(7, 2);
            */
        }
    }
}
