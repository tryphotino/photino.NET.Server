using Photino.NET.Extensions;
using System.Text;

namespace Photino.NET.Server.Sample;

internal class Program
{
    const string WINDOW_TITLE = "Photino.Vite Demo Server App";

    [STAThread]
    static void Main(string[] args)
    {
        var builder = PhotinoApplicationBuilder.CreatePhotinoBuilder(args, out var baseUrl);

        var (app, window) = builder.BuildApplication();

        window
            .Center()
            .SetTitle(WINDOW_TITLE)
            .SetUseOsDefaultSize(false)
            .SetSize(2000, 1500)
            // .SetIconFile("Resources/photino-logo.ico")
            .RegisterCustomSchemeHandler("app", (object sender, string scheme, string url, out string contentType) =>
            {
                contentType = "text/javascript";
                return new MemoryStream(Encoding.UTF8.GetBytes(@"
                        (() =>{
                            window.setTimeout(() => {
                                alert(`🎉 Dynamically inserted JavaScript.`);
                            }, 1000);
                        })();
                    "));
            })
            // Most event handlers can be registered after the
            // PhotinoWindow was instantiated by calling a registration 
            // method like the following RegisterWebMessageReceivedHandler.
            // This could be added in the PhotinoWindowOptions if preferred.
            .RegisterWebMessageReceivedHandler((object? sender, string message) =>
            {
                if (sender is not PhotinoWindow window) return;

                // The message argument is coming in from sendMessage.
                // "window.external.sendMessage(message: string)"
                string response = $"Received message: \"{message}\"";

                // Send a message back the to JavaScript event handler.
                // "window.external.receiveMessage(callback: Function)"
                window.SendWebMessage(response);
            });

        window.Load(app, baseUrl);
    }
}
