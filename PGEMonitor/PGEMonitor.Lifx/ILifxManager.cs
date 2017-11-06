using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI;

namespace PGEMonitor.Lifx
{
    public interface ILifxManager : IDisposable
    {
        Task<LifxDevice> GetSelectedDeviceAsync();
        Task<bool> SelectDeviceAsync(string label);
        Task InitAsync(Dictionary<string, string> parameters);
        Task SetSelectedDeviceRgbColorAsync(Color color);
        Task SetDeviceRgbColorAsync(string label, Color color);
        Task<IEnumerable<LifxDevice>> GetAllDevicesAsync(bool refresh);
        Task<LifxDevice> GetDeviceByLabelAsync(string label);
    }
}
