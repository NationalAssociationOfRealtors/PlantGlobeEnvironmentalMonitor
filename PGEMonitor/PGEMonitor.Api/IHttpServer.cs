using System.Threading.Tasks;

namespace PGEMonitor.Api
{
    public interface IHttpServer
    {
        Task ListenAsync(int port, string routePrefix);
    }
}
