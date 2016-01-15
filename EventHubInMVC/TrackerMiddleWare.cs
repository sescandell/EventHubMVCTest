using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Http;
using System.Threading.Tasks;

namespace EventHubInMVC
{
    internal sealed class TrackerMiddleWare
    {
        private readonly RequestDelegate _next;

        public TrackerMiddleWare(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, IMessenger messenger)
        {
            await _next.Invoke(context);

            await messenger.SendAsync(context.Request.Path.ToString());
        }
    }
}