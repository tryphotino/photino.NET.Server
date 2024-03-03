using Photino.NET.Server;

namespace Photino.NET.Extensions;

/// <summary>
/// The PhotinoWindowExtensions class provides extension methods for interacting with a PhotinoWindow.
/// </summary>
public static class PhotinoWindowExtensions
{
    /// <summary>
    /// Loads the content into the PhotinoWindow based on the provided web application and base URL.
    /// </summary>
    /// <param name="window">The PhotinoWindow instance.</param>
    /// <param name="app">The web application instance.</param>
    /// <param name="baseUrl">The base URL of the web application.</param>
    public static void Load(this PhotinoWindow window, WebApplication app, string baseUrl)
    {
        // If the environment is in development mode
        if (app.Environment.IsDevelopment())
        {
            var devserver = app.Services.GetRequiredService<PhotinoDevelopmentServer>();

            // Start the development server asynchronously
            devserver.StartAsync();

            // Wait until the development server is ready
            if (!devserver.WaitForStartup())
                throw new InvalidOperationException("Can't start development server");

            // Enable DevTools and load the development server URL
            window.SetDevToolsEnabled(true);
            window.Load(devserver.Url);
        }
        else
        {
            // Disable DevTools and load the production URL
            window.SetDevToolsEnabled(false);
            window.Load($"{baseUrl}/index.html");
        }

        // Wait for the window to close
        window.WaitForClose();
    }
}

