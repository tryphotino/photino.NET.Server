using Photino.NET.Server;

namespace Photino.NET.Extensions;

public static class PhotinoWindowExtensions
{
    public static void Load(this PhotinoWindow window, WebApplication app, string baseUrl)
    {
        if (app.Environment.IsDevelopment())
        {
            var devserver = app.Services.GetRequiredService<PhotinoDevelopmentServer>();
            devserver.StartAsync();

            if (!devserver.WaitForStartup())
                throw new InvalidOperationException("Can't start development server");

            window.Load(devserver.Url);
        }
        else
        {
            window.Load($"{baseUrl}/index.html");
        }

        window.WaitForClose();
    }
}
