using Photino.NET.Server;

namespace Photino.NET.Extensions;

/// <summary>
/// The PhotinoApplicationExtensions class provides extension methods for building a Photino web application.
/// </summary>
public static class PhotinoApplicationExtensions
{
    /// <summary>
    /// Builds a Photino application from the provided web application builder.
    /// </summary>
    /// <param name="builder">The web application builder.</param>
    /// <returns>A PhotinoApplication containing the built web application and associated window.</returns>
    public static PhotinoApplication BuildApplication(this WebApplicationBuilder builder)
    {
        // Build the web application
        var app = builder.Build();

        // Configure static file handling with default content type
        app.UseStaticFiles(new StaticFileOptions
        {
            DefaultContentType = "text/plain"
        });

        // Run the web application asynchronously
        app.RunAsync();

        // Get the PhotinoWindow service from the built application
        var window = app.Services.GetRequiredService<PhotinoWindow>();

        // Handle window closing event and attempt to stop the application
        window.WindowClosing += (s, e) => !app.StopAsync()
            .Wait(TimeSpan.FromSeconds(10));

        // Return a PhotinoApplication containing the built web application and associated window
        return new PhotinoApplication(app, window);
    }

    /// <summary>
    /// Loads the content into the PhotinoWindow based on the provided web application and base URL.
    /// </summary>
    /// <param name="window">The PhotinoWindow instance.</param>
    /// <param name="app">The web application instance.</param>
    /// <param name="baseUrl">The base URL of the web application.</param>
    public static void Run(this PhotinoApplication photino, string baseUrl)
    {
        var (app, window) = photino;

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
