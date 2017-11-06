using Autofac;
using PGEMonitor.Core;
using PGEMonitor.Lifx;
using Restup.Webserver.Attributes;
using Restup.Webserver.Models.Contracts;
using Restup.Webserver.Models.Schemas;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PGEMonitor.Api.Restup.Controllers
{
    [RestController(InstanceCreationType.Singleton)]
    public class BulbsController
    {
        [UriFormat("/bulbs")]
        public async Task<IGetResponse> GetAll()
        {
            using (ILifetimeScope scope = DI.Container.BeginLifetimeScope())
            {
                ILifxManager lifxMan = scope.Resolve<ILifxManager>();

                IEnumerable<LifxDevice> devices = await lifxMan.GetAllDevicesAsync(false);

                return new GetResponse(GetResponse.ResponseStatus.OK, devices);
            }
        }

        [UriFormat("/bulbs/selected")]
        public async Task<IGetResponse> GetSelected()
        {
            using (ILifetimeScope scope = DI.Container.BeginLifetimeScope())
            {
                ILifxManager lifxMan = scope.Resolve<ILifxManager>();

                LifxDevice device = await lifxMan.GetSelectedDeviceAsync();

                if (device == null)
                {
                    return new GetResponse(GetResponse.ResponseStatus.OK, "No Lifx device has been selected.");
                }
                else
                {
                    return new GetResponse(GetResponse.ResponseStatus.OK, device);
                }
            }
        }

        [UriFormat("/bulbs/set/{label}")]
        public async Task<IGetResponse> Set(string label)
        {
            using (ILifetimeScope scope = DI.Container.BeginLifetimeScope())
            {
                ILifxManager lifxMan = scope.Resolve<ILifxManager>();

                bool selected = await lifxMan.SelectDeviceAsync(label);

                if (selected)
                {
                    return new GetResponse(GetResponse.ResponseStatus.OK, $"Device {label} was selected.");
                }
                else
                {
                    return new GetResponse(GetResponse.ResponseStatus.OK, $"Device {label} was not found.");
                }
            }
        }
    }
}
