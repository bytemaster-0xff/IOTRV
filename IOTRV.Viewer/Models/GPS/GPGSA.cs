using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IOTRV.Viewer.Models.GPS
{
    public class GPGSA : INEMAMessage
    {
        public enum FixModes
        {
            Manual,
            Automatic,
        }

        public enum FixTypes
        {
            NotAvailable,
            TwoDimmensional,
            ThreeDimmensional,
        }

        public double PDOP { get; set; }
        public double VDOP { get; set; }
        public double HDOP { get; set; }

        public FixModes FixMode {get; set;}
        public FixTypes FixType { get; set; }

        public List<int> Satellites { get; set; }

        public void Parse(String message)
        {

        }

    }
}
