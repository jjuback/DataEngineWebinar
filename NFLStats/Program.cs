using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using C1.DataEngine;
using NFLStats.Data;

namespace NFLStats
{
    public class Program
    {
        private static Workspace workspace;

        public static Workspace Workspace
        {
            get { return workspace; }
        }

        public static void Main(string[] args)
        {
            Initialize();
            CreateHostBuilder(args).Build().Run();
        }

        private static void Initialize()
        {
            workspace = new Workspace();
            workspace.KeepFiles = KeepFileType.Index;
            workspace.ReuseJoins = false;
            workspace.AutoSave = true;
            workspace.Init("workspace");
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
