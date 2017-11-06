using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace PGEMonitor.Lifx.Http
{
    public class LifxHttpManager : ILifxManager
    {
        string _token;

        HttpClient _httpClient;

        const string ENDPOINT = "https://api.lifx.com/v1/lights";

        IEnumerable<LifxDevice> _devices;

        LifxDevice _selected;

        public void Dispose()
        {
            if (_httpClient != null)
            {
                _httpClient.Dispose();
            }
        }

        public async Task<IEnumerable<LifxDevice>> GetAllDevicesAsync(bool refresh = false)
        {
            if (refresh == false)
            {
                return _devices;
            }

            HttpResponseMessage response = await _httpClient.GetAsync(ENDPOINT + "/all");

            string content = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<IEnumerable<LifxDevice>>(content);
        }

        public async Task SetPowerAsync(string deviceId, bool state)
        {
            JObject jsonReq = new JObject();
            jsonReq.Add("power", state ? "on" : "off");
            jsonReq.Add("infrared", "1.0");

            string jsonData = jsonReq.ToString();

            HttpContent content = new StringContent(jsonReq.ToString(), Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _httpClient.PutAsync(ENDPOINT + $"/id:{deviceId}/state", content);
        }

        public async Task InitAsync(Dictionary<string, string> parameters)
        {
            if (!parameters.TryGetValue("OAUTH2_TOKEN", out _token))
            {
                throw new Exception("OAUTH2_TOKEN parameter is not defined.");
            }
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"Bearer {_token}");

            _devices = await GetAllDevicesAsync(true);

            string label;
            if (!parameters.TryGetValue("SELECTED_DEVICE", out label))
            {
                throw new Exception("SELECTED_DEVICE parameter is not defined.");
            }

            await SelectDeviceAsync(label);
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

        public async Task SetDeviceRgbColorAsync(string label, Windows.UI.Color color)
        {
            LifxDevice device = await GetDeviceByLabelAsync(label);

            if (device == null)
            {
                Debug.WriteLine($"Device with label {label} was not found.");
            }

            JObject jsonReq = new JObject();
            jsonReq.Add("power", "on");
            jsonReq.Add("color", $"rgb:{color.R},{color.G},{color.B}");
            jsonReq.Add("brightness", 1);
            jsonReq.Add("duration", 1);

            string jsonData = jsonReq.ToString();

            HttpContent content = new StringContent(jsonReq.ToString(), Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _httpClient.PutAsync(ENDPOINT + $"/id:{device.Id}/state", content);
        }

        public async Task<LifxDevice> GetDeviceByLabelAsync(string label)
        {
            IEnumerable<LifxDevice> devices = await GetAllDevicesAsync();

            return _devices.Where(x => x.Label == label).FirstOrDefault();
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
    }
}
