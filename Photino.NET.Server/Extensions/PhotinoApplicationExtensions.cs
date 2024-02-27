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

        var window = app.Services.GetRequiredService<PhotinoWindow>();

        window.WindowClosing += (s, e) => !app.StopAsync()
            .Wait(TimeSpan.FromSeconds(10));

        return new PhotinoApplication(app, window);
    }
}
