using Photino.NET.Server;

namespace Photino.NET.Extensions;

public static class PhotinoApplicationExtensions
{
    public static PhotinoApplication BuildApplication(this WebApplicationBuilder builder)
    {
        var app = builder.Build();

        app.UseStaticFiles(new StaticFileOptions
        {
            DefaultContentType = "text/plain"
        });

        app.RunAsync();

        var window = new PhotinoWindow();

        window.WindowClosing += (s, e) => !app.StopAsync()
            .Wait(TimeSpan.FromSeconds(10));

        return new PhotinoApplication { App = app, MainWindow = window };
    }

    public static void Load(this PhotinoApplication photino, string baseUrl)
    {
        if (photino.App.Environment.IsDevelopment())
        {
            var devserver = photino.App.Services.GetRequiredService<PhotinoDevelopmentServer>();
            devserver.StartAsync();

            if (!devserver.WaitForStartup())
                throw new InvalidOperationException("Can't start development server");

            photino.MainWindow.Load(devserver.Url);
        }
        else
        {
            photino.MainWindow.Load($"{baseUrl}/index.html");
        }

        photino.MainWindow.WaitForClose();
    }
}
