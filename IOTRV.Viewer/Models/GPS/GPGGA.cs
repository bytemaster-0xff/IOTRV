using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IOTRV.Viewer.Models.GPS
{
    public class GPGGA : INEMAMessage
    {

        //$GPGGA,174826.00,2804.57003,N,08242.58567,W,2,11,0.95,21.5,M,-28.0,M,,0000*54

        public DateTime DateStamp { get; set; }

        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int NumberSatellites { get; set; }
        public double HDOP { get; set; }
        public double Altitude { get; set; }
        
        public void Parse(String message)
        {
            var parts = message.Split(',','*');

            var timeStamp = message[1];
            var hh = parts[1].Substring(0, 2);
            var mm = parts[1].Substring(2, 2);
            var ss = parts[1].Substring(4, 2);
            var ms = parts[1].Substring(7, 2);

            var latDeg = parts[2].Substring(0,2);
            var latMin = parts[2].Substring(2);

            Latitude = Convert.ToDouble(latDeg) + (Convert.ToDouble(latMin) / Convert.ToDouble(60));
            if (parts[3] == "S")
                Latitude *= -1.0;

            var lonDeg = parts[4].Substring(0, 3);
            var lonMin = parts[4].Substring(3);

            Longitude = Convert.ToDouble(lonDeg) + (Convert.ToDouble(lonMin) / Convert.ToDouble(60));
            if (parts[5] == "W")
                Longitude *= -1.0;

            HDOP = Convert.ToDouble(parts[8]);
            Altitude = Convert.ToDouble(parts[9]);
            NumberSatellites = Convert.ToInt32(parts[7]);
        }
    }
}
