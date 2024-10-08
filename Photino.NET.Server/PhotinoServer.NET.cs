using System;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;

namespace PhotinoNET.Server;

/// <summary>
/// The PhotinoServer class enables users to host their web projects in
/// a static, local file server to prevent CORS and other issues.
/// </summary>
public class PhotinoServer
{
    public static WebApplication CreateStaticFileServer(
        string[] args,
        out string baseUrl)
    {
        return CreateStaticFileServer(
            args,
            startPort: 8000,
            portRange: 100,
            webRootFolder: "wwwroot",
            out baseUrl);
    }

    public static WebApplication CreateStaticFileServer(
        string[] args,
        int startPort,
        int portRange,
        string webRootFolder,
        out string baseUrl)
    {
        var builder = WebApplication
            .CreateBuilder(new WebApplicationOptions()
            {
                Args = args,
                WebRootPath = webRootFolder
            });

        string embeddedResourcePath = $"Resources/{webRootFolder}";

        if (Directory.Exists(embeddedResourcePath))
        {
            var manifestEmbeddedFileProvider =
                new ManifestEmbeddedFileProvider(
                    System.Reflection.Assembly.GetEntryAssembly(),
                    embeddedResourcePath);

            var physicalFileProvider = builder.Environment.WebRootFileProvider;

            CompositeFileProvider compositeWebProvider
                = new(manifestEmbeddedFileProvider, physicalFileProvider);

            builder.Environment.WebRootFileProvider = compositeWebProvider;
        }
        else
        {
            Console.Error.WriteLine($"The folder {webRootFolder} already exists. Please remove it before running the server.");
        }

        int port = startPort;

        // Try ports until available port is found
        while (IPGlobalProperties
            .GetIPGlobalProperties()
            .GetActiveTcpListeners()
            .Any(x => x.Port == port))
        {
            if (port > port + portRange)
            {
                throw new SystemException($"Couldn't find open port within range {port - portRange} - {port}.");
            }

            port++;
        }

        baseUrl = $"http://localhost:{port}";

        builder.WebHost.UseUrls(baseUrl);

        WebApplication app = builder.Build();
        app.UseDefaultFiles();
        app.UseStaticFiles(new StaticFileOptions
        {
            DefaultContentType = "text/plain"
        });

        return app;
    }
}
