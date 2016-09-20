using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.Geolocation;
using Windows.Devices.SerialCommunication;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Services.Maps;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace IOTRV.Viewer
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        bool _connected;
        bool _running;
        SerialDevice _serialDevice;
        DataWriter _dataWriter;
        DataReader _dataReader;


        public MainPage()
        {
            this.InitializeComponent();
            TheMap.Style = MapStyle.AerialWithRoads;
            this.Loaded += MainPage_Loaded;
        }

        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            
            TheMap.TileSources.Add(new MapTileSource(new HttpMapTileDataSource("http://seawolf.blob.core.windows.net/tiles/{quadkey}.png")));

            var aqs = SerialDevice.GetDeviceSelector();
            var dis = await DeviceInformation.FindAllAsync(aqs);
            if (dis.Any())
            {
                await OpenDevice(dis.First());
            }
        }

        private async void DownloadMaps_Click(object sender, RoutedEventArgs e)
        {
            var geoLocator = new Geolocator();
            geoLocator.DesiredAccuracy = PositionAccuracy.High;
            var pos = await geoLocator.GetGeopositionAsync();
            string latitude = "Latitude: " + pos.Coordinate.Point.Position.Latitude.ToString();
            string longitude = "Longitude: " + pos.Coordinate.Point.Position.Longitude.ToString();             

            TheMap.Center = new Windows.Devices.Geolocation.Geopoint(new Windows.Devices.Geolocation.BasicGeoposition() { Latitude = pos.Coordinate.Point.Position.Latitude, Longitude = pos.Coordinate.Point.Position.Longitude });
           
        }

        public void Log(string str, params object[] args)
        {
            Debug.WriteLine(String.Format(str, args));
        }

        private void ProcessMessage(String message)
        {
            var parts = message.Split(',');
            switch(parts[0])
            {
                case "$GPGGA":
                    {
                        var gpgga = new Models.GPS.GPGGA();
                        gpgga.Parse(message);
                        TheMap.Center = new Windows.Devices.Geolocation.Geopoint(new Windows.Devices.Geolocation.BasicGeoposition() { Latitude = gpgga.Latitude, Longitude = gpgga.Longitude });
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

        StringBuilder msgBuffer = new StringBuilder();

        public void HandleRXByte(byte ch)
        {
            if(ch == '\n' || ch == '\r')
            {
                if(msgBuffer.Length > 0)
                {
                    ProcessMessage(msgBuffer.ToString());
                    Debug.WriteLine(msgBuffer.ToString());
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
                    Log("Starting to listen.");
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
                Debug.WriteLine(ex);
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



        private async Task<bool> OpenDevice(DeviceInformation dis)
        {
            Log("Starting opening port {0} {1} {2}", dis.Id, dis.Name, DateTime.Now);
            _connected = false;
            try
            {
                Log("Opening port => " + dis.Name);
                _serialDevice = await SerialDevice.FromIdAsync(dis.Id);
                if (_serialDevice != null)
                {
                    Log("Found Device => " + dis.Name);
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
    }
}
