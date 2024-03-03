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
}
