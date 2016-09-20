using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IOTRV.Viewer.Models.GPS
{
    public class GPGSV : INEMAMessage
    {

        const int START_INDEX = 5;
        public int TotalSatellitesInView { get; set; }
        public int MessageCount { get; set; }
        public int MessageIndex { get; set; }
        public List<SV> SatellitesInView { get; private set; }

        public void Parse(String message)
        {
            SatellitesInView = new List<SV>();

            var parts = message.Split(',','*');
            TotalSatellitesInView = Convert.ToInt32(parts[3]);
            MessageCount = Convert.ToInt32(parts[1]);
            MessageIndex = Convert.ToInt32(parts[2]);

            var idx = 0;

            var satIndex = idx + (MessageIndex - 1) * 4;
            
            while (satIndex < TotalSatellitesInView && idx < 4)
            {
                try
                {
                    var start = (idx * 4) + 4;

                    SatellitesInView.Add(new SV()
                    {
                        SatelliteNumber = Convert.ToInt32(parts[start + 0]),
                        Elevation = String.IsNullOrEmpty(parts[start + 1]) ? (int?)null : Convert.ToInt32(parts[start + 1]),
                        Azimuth = String.IsNullOrEmpty(parts[start + 2]) ? (int?)null : Convert.ToInt32(parts[start + 2]),
                        SNR = String.IsNullOrEmpty(parts[start + 3]) ? (int?)null : Convert.ToInt32(parts[start + 3])
                    });
                    satIndex++;
                    idx++;
                }
                catch(Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    Debug.WriteLine(ex.StackTrace);
                    Debug.WriteLine(message);
                    Debugger.Break();
                }
            }
        }
    }
}
