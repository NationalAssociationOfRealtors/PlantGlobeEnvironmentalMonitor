using LifxNet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace PGEMonitor.Lifx.Udp
{
    public class UdpLifxManager : ILifxManager
    {
        LifxClient _client = null;
        List<LightBulb> _bulbs = new List<LightBulb>();
        List<LifxDevice> _devices = new List<LifxDevice>();
        LifxDevice _selected;

        string _initLabel;

        void _client_DeviceLost(object sender, LifxClient.DeviceDiscoveryEventArgs e)
        {
            LightBulb bulb = e.Device as LightBulb;
            if (_bulbs.Contains(bulb))
            {
                _devices.RemoveAll(x => x.MacAddressName == bulb.MacAddressName);

                if (bulb.MacAddressName == _selected.MacAddressName)
                {
                    _selected = null;
                }

                _bulbs.Remove(bulb);
            }
        }

        async void _client_DeviceDiscovered(object sender, LifxClient.DeviceDiscoveryEventArgs e)
        {
            LightBulb bulb = e.Device as LightBulb;
            if (!_bulbs.Contains(bulb))
            {
                _bulbs.Add(bulb);

                LifxDevice device = new LifxDevice
                {
                    MacAddressName = bulb.MacAddressName
                };

                device.Label = await _client.GetDeviceLabelAsync(bulb);

                if (device.Label == _initLabel && _selected == null)
                {
                    _selected = device;
                }
                _devices.Add(device);
            }
        }
        public void Dispose()
        {
            if (_client != null)
            {
                _client.DeviceDiscovered -= _client_DeviceDiscovered;
                _client.DeviceLost -= _client_DeviceLost;
                _client.StopDeviceDiscovery();
                _client.Dispose();
            }
        }

        public async Task InitAsync(Dictionary<string, string> parameters)
        {
            _client = await LifxClient.CreateAsync();
            _client.DeviceDiscovered += _client_DeviceDiscovered;
            _client.DeviceLost += _client_DeviceLost;
            _client.StartDeviceDiscovery();

            if (!parameters.TryGetValue("SELECTED_DEVICE", out _initLabel))
            {
                throw new Exception("SELECTED_DEVICE parameter is not defined.");
            }
        }

        public async Task SetDeviceRgbColorAsync(string label, Windows.UI.Color color)
        {
            LifxDevice lifx = await GetDeviceByLabelAsync(label);

            if (lifx == null)
            {
                Debug.WriteLine($"Device with label {label} was not found.");
                return;
            }

            LightBulb bulb = GetBulb(lifx);

            LifxNet.Color lifxColor = new LifxNet.Color
            {
                R = color.R,
                G = color.G,
                B = color.B
            };

            // Allowed values for kelvin parameter are between 2500 and 9000.
            Task task = _client.SetColorAsync(bulb, lifxColor, 5000, TimeSpan.FromSeconds(1));

            await Task.WhenAny(task, Task.Delay(5000));
        }

        LightBulb GetBulb(LifxDevice device)
        {
            return _bulbs.Where(x => x.MacAddressName == device.MacAddressName).FirstOrDefault();
        }

        public async Task<IEnumerable<LifxDevice>> GetAllDevicesAsync(bool refresh = true)
        {
            return await Task.FromResult(_devices);
        }

        public async Task<LifxDevice> GetDeviceByLabelAsync(string label)
        {
            IEnumerable<LifxDevice> devices = await GetAllDevicesAsync();

            return devices.Where(x => x.Label == label).FirstOrDefault();
        }

        public async Task<LifxDevice> GetSelectedDeviceAsync()
        {
            return await Task.FromResult(_selected);
        }

        public async Task<bool> SelectDeviceAsync(string label)
        {
            LifxDevice device = await GetDeviceByLabelAsync(label);

            if (device == null)
            {
                Debug.WriteLine($"No device was found with label {label}.");
                return false;
            }

            _selected = device;
            return true;
        }

        public async Task SetSelectedDeviceRgbColorAsync(Windows.UI.Color color)
        {
            if (_selected == null)
            {
                Debug.WriteLine("No device is selected!");
                return;
            }
            await SetDeviceRgbColorAsync(_selected.Label, color);
        }
    }
}
