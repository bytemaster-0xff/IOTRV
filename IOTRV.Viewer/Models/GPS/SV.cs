using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IOTRV.Viewer.Models.GPS
{
    public class SV
    {
        public int SatelliteNumber { get; set; }
        public int? Elevation { get; set; }
        public int? Azimuth { get; set; }
        public int? SNR { get; set; }
    }
}
