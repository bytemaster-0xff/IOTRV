using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IOTRV.Viewer.Models.GPS
{
    public interface INEMAMessage
    {
        void Parse(String message);
    }
}
