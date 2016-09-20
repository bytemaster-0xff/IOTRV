using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.Geolocation;
using Windows.Devices.SerialCommunication;
using Windows.Foundation;
using Windows.Storage.Streams;

namespace IOTRV.Viewer.DeviceServices
{
    public class LocationServices : ILocationServices, IDisposable
    {
        Geolocator _geoLocator;

        SerialDevice _serialDevice;
        DataWriter _dataWriter;
        DataReader _dataReader;
        bool _running;
        bool _connected;

        private void Log(String str, params object[] args)
        {
            Debug.WriteLine(String.Format(str, args));
        }

        private async Task<bool> OpenDevice()
        {
            var aqs = SerialDevice.GetDeviceSelector();
            var serialPorts = await DeviceInformation.FindAllAsync(aqs);
            var firstSerialPort = serialPorts.FirstOrDefault();
            if(firstSerialPort == null)
            {
                return false;
            }

            Log("Starting opening port {0} {1} {2}", firstSerialPort.Id, firstSerialPort.Name, DateTime.Now);
            _connected = false;
            try
            {
                Log("Opening port => " + firstSerialPort.Name);
                _serialDevice = await SerialDevice.FromIdAsync(firstSerialPort.Id);
                if (_serialDevice != null)
                {
                    Log("Found Device => " + firstSerialPort.Name);
                    _serialDevice.BaudRate = 9600;
                    _serialDevice.DataBits = 8;
                    _serialDevice.Parity = SerialParity.None;
                    _serialDevice.StopBits = SerialStopBitCount.One;

                    _dataWriter = new DataWriter(_serialDevice.OutputStream);

                    Listen();

                    await Task.Delay(500);

                    _connected = true;

                    Log("SUCCESS! Could Not Open Port");
                }
                else
                {
                    Log("FAILED! Could Not Open Port");
                }
            }
            catch (Exception ex)
            {
                Log("OpenAsync {0}", ex);
            }

            Log("END " + DateTime.Now.ToString());

            return _connected;
        }

        public void Init()
        {
            _geoLocator = new Geolocator();
            _geoLocator.PositionChanged += (sndr, args) => PositionChanged(this, args);
            _geoLocator.StatusChanged += (sndr, args) => StatusChanged(this, args);
        }

        public event TypedEventHandler<ILocationServices, PositionChangedEventArgs> PositionChanged;
        public event TypedEventHandler<ILocationServices, StatusChangedEventArgs> StatusChanged;

        public IAsyncOperation<Geoposition> GetGeopositionAsync()
        {
            return _geoLocator.GetGeopositionAsync();
        }

        public IAsyncOperation<Geoposition> GetGeopositionAsync(TimeSpan maximumAge, TimeSpan timeout)
        {
            return _geoLocator.GetGeopositionAsync(maximumAge, timeout);
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        private PositionStatus _locationStatus;
        public PositionStatus LocationStatus
        {
            get { return _locationStatus; }
        }

        private double _movementThreshold;
        public System.Double MovementThreshold
        {
            get { return _movementThreshold; }
            set
            {
                _geoLocator.MovementThreshold = value;
                _movementThreshold = value;
            }
        }

        private UInt32 _reportInterval = 0;
        public System.UInt32 ReportInterval
        {
            get { return _reportInterval; }
            set
            {
                _geoLocator.ReportInterval = value;
                _reportInterval = value;
            }
        }

        StringBuilder msgBuffer = new StringBuilder();

        private void ProcessMessage(String message)
        {
            var parts = message.Split(',');
            switch (parts[0])
            {
                case "$GPGGA":
                    {
                        var gpgga = new Models.GPS.GPGGA();
                        gpgga.Parse(message);
                    }
                    break;
                case "$GPGSV":
                    {
                        var gpgsv = new Models.GPS.GPGSV();
                        gpgsv.Parse(message);
                    }
                    break;
            }
        }

        public void HandleRXByte(byte ch)
        {
            if (ch == '\n' || ch == '\r')
            {
                if (msgBuffer.Length > 0)
                {
                    ProcessMessage(msgBuffer.ToString());
                }

                msgBuffer.Clear();
            }
            else
            {
                msgBuffer.Append((char)ch);
            }
        }



        public async void Listen()
        {
            try
            {
                if (_serialDevice != null)
                {
                    _dataReader = new DataReader(_serialDevice.InputStream);
                    _dataReader.InputStreamOptions = InputStreamOptions.Partial;


                    uint ReadBufferLength = 1;
                    var buffer = new byte[ReadBufferLength];
                    _running = true;
                    // keep reading the serial input
                    while (_running)
                    {
                        var bytesRead = await _dataReader.LoadAsync(ReadBufferLength);
                        if (bytesRead > 0)
                        {
                            _dataReader.ReadBytes(buffer);

                            foreach (var ch in buffer)
                            {
                                HandleRXByte(ch);
                            }
                        }
                    }
                };
            }
            catch (Exception ex)
            {
                Close();
            }
        }

        public void Close()
        {
            if (_dataWriter != null)
            {
                _dataWriter.Dispose();
                _dataWriter = null;
            }

            if (_dataReader != null)
            {
                _running = false;
                _dataReader.Dispose();
                _dataReader = null;
            }

            if (_serialDevice != null)
            {
                _serialDevice.Dispose();
                _serialDevice = null;
            }
        }
    }
}
