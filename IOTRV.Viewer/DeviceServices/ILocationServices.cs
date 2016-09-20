using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.Foundation;

namespace IOTRV.Viewer.DeviceServices
{
    public interface ILocationServices
    {
        void Init();

        event TypedEventHandler<ILocationServices, PositionChangedEventArgs> PositionChanged;
        event TypedEventHandler<ILocationServices, StatusChangedEventArgs> StatusChanged;
        IAsyncOperation<Geoposition> GetGeopositionAsync();
        IAsyncOperation<Geoposition> GetGeopositionAsync(TimeSpan maximumAge, TimeSpan timeout);
    }
}
