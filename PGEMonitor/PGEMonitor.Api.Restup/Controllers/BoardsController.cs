using Autofac;
using PGEMonitor.Core;
using PGEMonitor.Gateway;
using Restup.Webserver.Attributes;
using Restup.Webserver.Models.Contracts;
using Restup.Webserver.Models.Schemas;
using System;

namespace PGEMonitor.Api.Restup.Controllers
{
    [RestController(InstanceCreationType.Singleton)]
    public class BoardsController
    {
        [UriFormat("/boards")]
        public IGetResponse GetAll()
        {
            using (ILifetimeScope scope = DI.Container.BeginLifetimeScope())
            {
                IGatewayManager gateway = scope.Resolve<IGatewayManager>();

                return new GetResponse(GetResponse.ResponseStatus.OK, gateway.GetBoards());
            }
        }

        [UriFormat("/boards/set/{id}")]
        public IGetResponse Set(int id)
        {
            using (ILifetimeScope scope = DI.Container.BeginLifetimeScope())
            {
                IGatewayManager gateway = scope.Resolve<IGatewayManager>();

                if (gateway.TrySelectBoard(id))
                {
                    return new GetResponse(GetResponse.ResponseStatus.OK, $"Monitoring successfully changed to Board ID {id}.");
                }
                else
                {
                    return new GetResponse(GetResponse.ResponseStatus.OK, $"Board ID {id} was not detected.");
                }
            }
        }
    }
}
