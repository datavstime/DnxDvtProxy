using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.Framework.Configuration;
using Microsoft.Framework.DependencyInjection;
using Newtonsoft.Json;

namespace DataVsTime
{
    public class Startup
    {
        IConfiguration Configuration;
        IAdapter Adapter;
        JsonSerializer Serializer;
        string PredefinedPages;

        public Startup(IHostingEnvironment env)
        {
            Configuration = new ConfigurationBuilder(".", new[] { new JsonConfigurationSource("config.json") }).Build();
            Adapter = AdapterFactory.GetAdapter(Configuration["Adapter:Type"]);
            Serializer = new JsonSerializer();

            var configText = File.ReadAllText("config.json");
            var o = JsonConvert.DeserializeObject<Dictionary<string, object>>(configText);
            if (o.ContainsKey("PredefinedPages"))
            {
                PredefinedPages = JsonConvert.SerializeObject(o["PredefinedPages"]);
            }
        }

        public void ConfigureServices(IServiceCollection services)
        {
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            var lockObj = new object();

            app.Use(async (context, next) =>
                {
                    // Note: kestrel was hanging sporadically, and I couldn't work out why, so I added this to prevent multiple requests
                    // executing at once in the hope that simplifying things would solve the problem. It seems it does.
                    // Note II: kestrel is actually not ready for use in production, so you should probably not use it.
                    lock (lockObj)
                    {
                        try
                        {
                            context.Response.Headers.Append("Content-Type", "application/json; charset=utf-8");
                            // TODO: this should be locked down:
                            context.Response.Headers.Append("Access-Control-Allow-Origin", "*");
                            // TODO: reconsider the need for no-caching:
                            context.Response.Headers.Append("Cache-Control", "no-cache, no-store, must-revalidate");
                            context.Response.Headers.Append("Pragma", "no-cache");
                            context.Response.Headers.Append("Expires", "0");
                            var t = RequestHandler.Handle(context, Adapter, Serializer, PredefinedPages);
                            t.Wait();
                        }
                        catch (Exception e)
                        {
                            context.Response.StatusCode = 500;
                            // TODO: better logging.
                            Console.WriteLine("Request for url: " + context.Request.Path + " resulted in exception: " + e.Message);
                            using (var sw = new StreamWriter(context.Response.Body))
                            {
                                var t = sw.WriteLineAsync("internal server error");
                                t.Wait();
                            }
                        }
                    }
                    await next();
                });
        }
    }
}
