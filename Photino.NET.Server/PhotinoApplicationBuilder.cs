using Photino.NET.Extensions;
using Photino.NET.Options;
using Photino.NET.Server;
using System.Net;
using System.Net.NetworkInformation;

namespace Photino.NET;

/// <summary>
/// The PhotinoServer class enables users to host their web projects in
/// a static, local file server to prevent CORS and other issues.
/// </summary>
public static class PhotinoApplicationBuilder
{
    public static WebApplicationBuilder CreatePhotinoBuilder(string[] args, out string baseUrl)
    {
        var options = PhotinoBuilderOptions.Default;
        options.Args = args;

        return CreatePhotinoBuilder(options, out baseUrl);
    }

    public static WebApplicationBuilder CreatePhotinoBuilder(PhotinoBuilderOptions options, out string baseUrl)
    {
        var builder = WebApplication.CreateBuilder(options);

        builder.Services.AddSingleton<PhotinoDevelopmentServer>();
        builder.Services.Configure<PhotinoDevelopmentServerOptions>(options =>
        {
            options.IndexFile = "index.html";
            options.StartCommand = "npm run dev";
            options.UserInterfacePath = "Interface";
            options.WaitUntilReadyTimeout = TimeSpan.FromSeconds(10);
        });

        // TODO: Even in the published version the application is a bit slow to start. due the server starting,
        //       see if there is a better way, maybe with the embedded provider

        //if (!builder.Environment.IsDevelopment())
        //{
        //    var manifestEmbeddedFileProvider = new ManifestEmbeddedFileProvider(
        //        Assembly.GetEntryAssembly(), $"Resources/{options.WebRootPath}");

        //    var physicalFileProvider = builder.Environment.WebRootFileProvider;

        //    CompositeFileProvider compositeWebProvider = new(manifestEmbeddedFileProvider, physicalFileProvider);

        //    builder.Environment.WebRootFileProvider = compositeWebProvider;
        //}

        var port = GetAvailablePort(options.StartingPort, options.PortRange);
        if (port == 0)
            throw new SystemException($"Couldn't find open port within range {port} - {options.PortRange}.");

        baseUrl = $"http://localhost:{port}";

        builder.WebHost.UseUrls(baseUrl);

        return builder;
    }

    public static int GetAvailablePort(int startingPort, int portRange)
    {
        IPEndPoint[] endPoints;
        List<int> portArray = [];

        IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();

        //getting active connections
        TcpConnectionInformation[] connections = properties.GetActiveTcpConnections();
        portArray.AddRange(from n in connections
                           where n.LocalEndPoint.Port >= startingPort
                           select n.LocalEndPoint.Port);

        //getting active tcp listners - WCF service listening in tcp
        endPoints = properties.GetActiveTcpListeners();
        portArray.AddRange(from n in endPoints
                           where n.Port >= startingPort
                           select n.Port);

        //getting active udp listeners
        endPoints = properties.GetActiveUdpListeners();
        portArray.AddRange(from n in endPoints
                           where n.Port >= startingPort
                           select n.Port);

        portArray.Sort();

        int range = startingPort + portRange;
        for (int i = startingPort; i < range; i++)
            if (!portArray.Contains(i))
                return i;

        return 0;
    }
}
