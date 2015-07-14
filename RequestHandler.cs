using Microsoft.AspNet.Http;
using System.IO;
using Newtonsoft.Json;
using System.Threading.Tasks;


namespace DataVsTime
{
    public class RequestHandler
    {
        public static int Version = 1;

        public async static Task Handle(HttpContext ctx, IAdapter adapter, JsonSerializer serializer, string predefinedPages)
        {
            ctx.Response.StatusCode = 200;

            // Response is cached in a MemoryStream first rather than writing directly to ctx.Response.Body
            // so that a 501 can be sent in the event of an exception.
            using (var ms = new MemoryStream())
            {
                using (var sw = new StreamWriter(ms))
                using (var tw = new JsonTextWriter(sw))
                {
                    if (ctx.Request.Path.Value.StartsWith("/api/v1/values"))
                    {
                        var series = ctx.Request.Query["series"];
                        var start = long.Parse(ctx.Request.Query["start"]);
                        var stop = long.Parse(ctx.Request.Query["stop"]);
                        var step = int.Parse(ctx.Request.Query["step"]);
                        serializer.Serialize(tw, await adapter.GetValuesAsync(series, start, stop, step));
                    }
                    else if (ctx.Request.Path.Value == "/api/v1/series")
                    {
                        serializer.Serialize(tw, await adapter.GetSeriesNamesAsync());
                    }
                    else if (ctx.Request.Path.Value == "/api/v1/predefined-pages")
                    {
                        await sw.WriteAsync(predefinedPages);
                    }
                    else if (ctx.Request.Path.Value == "/api/version")
                    {
                        serializer.Serialize(tw, Version);
                    }
                    else
                    {
                        ctx.Response.StatusCode = 501;
                        await sw.WriteLineAsync("not implemented");
                    }
                }

                var bs = ms.ToArray();
                ctx.Response.Body.Write(bs, 0, bs.Length);
            }
        }
    }
}
