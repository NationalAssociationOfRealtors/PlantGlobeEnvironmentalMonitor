using PGEMonitor.Api.Restup.Controllers;
using Restup.Webserver.Http;
using Restup.Webserver.Rest;
using System.Threading.Tasks;

namespace PGEMonitor.Api.Restup
{
    public class RestupHttpServer : IHttpServer
    {
        public async Task ListenAsync(int port, string routePrefix)
        {
            RestRouteHandler restRouteHandler = new RestRouteHandler();

            restRouteHandler.RegisterController<BoardsController>();
            restRouteHandler.RegisterController<BulbsController>();

            HttpServerConfiguration configuration = new HttpServerConfiguration()
              .ListenOnPort(port)
              .RegisterRoute(routePrefix, restRouteHandler)
              .EnableCors();

            HttpServer httpServer = new HttpServer(configuration);
            await httpServer.StartServerAsync();
        }
    }
}
