using Microsoft.Extensions.Options;
using Photino.NET.Extensions;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Photino.NET.Server;

/// <summary>
/// Represents the Photino Development Server for serving static files during development.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="PhotinoDevelopmentServer"/> class.
/// </remarks>
/// <param name="lifetime">Application lifetime interface.</param>
/// <param name="environment">WebHost environment.</param>
/// <param name="options">Options for configuring the Photino Development Server.</param>
public partial class PhotinoDevelopmentServer(IHostApplicationLifetime lifetime, IWebHostEnvironment environment, IOptions<PhotinoDevelopmentServerOptions> options)
{
    private readonly PhotinoDevelopmentServerOptions options = options.Value;
    private readonly IHostApplicationLifetime lifetime = lifetime;
    private readonly IWebHostEnvironment environment = environment;

    /// <summary>
    /// Gets the URL of the Photino Development Server.
    /// </summary>
    public Uri Url { get; protected set; }

    /// <summary>
    /// Starts the Photino Development Server asynchronously.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task StartAsync() => Task.Factory.StartNew(StartProcess, TaskCreationOptions.LongRunning);

    /// <summary>
    /// Waits for the server to start up, with a timeout.
    /// </summary>
    /// <returns>True if the server started successfully within the timeout; otherwise, false.</returns>
    public bool WaitForStartup() => SpinWait.SpinUntil(() => Url is not null, options.WaitUntilReadyTimeout);

#if NET7_0_OR_GREATER

    /// <summary>
    /// Determines the URL of the Photino Development Server from the output data.
    /// </summary>
    /// <param name="data">Output data from the server process.</param>
    /// <returns>The URL of the server, or null if not found.</returns>
    private static Uri DetermineDevServerUrl(string data)
    {
        var withoutUnicodes = GetUnicodeCharacterRegex().Replace(data, string.Empty); ;
        var withoutColorCodes = GetUnicodeDigitsRegex().Replace(withoutUnicodes, string.Empty);
        withoutColorCodes = withoutColorCodes.Trim();

        var httpIndex = withoutColorCodes.IndexOf("http://");

        if (httpIndex == -1) return null;

        var host = withoutColorCodes[httpIndex..];

        return new Uri(host);
    }

    [GeneratedRegex(@"[^\x20-\xaf]+")]
    private static partial Regex GetUnicodeCharacterRegex();

    [GeneratedRegex(@"\[[0-9]{1,2}m")]
    private static partial Regex GetUnicodeDigitsRegex();

#else

    /// <summary>
    /// Determines the URL of the Photino Development Server from the output data.
    /// </summary>
    /// <param name="data">Output data from the server process.</param>
    /// <returns>The URL of the server, or null if not found.</returns>
    private static Uri DetermineDevServerUrl(string data)
    {
        var withoutUnicodes = Regex.Replace(@"[^\x20-\xaf]+", data, string.Empty); ;
        var withoutColorCodes = Regex.Replace(@"\[[0-9]{1,2}m", withoutUnicodes, string.Empty);
        withoutColorCodes = withoutColorCodes.Trim();

        var httpIndex = withoutColorCodes.IndexOf("http://");

        if (httpIndex == -1) return null;

        var host = withoutColorCodes[httpIndex..];

        return new Uri(host);
    }

#endif

    /// <summary>
    /// Gets the executable path based on the operating system.
    /// </summary>
    /// <returns>The executable path.</returns>
    private static string GetExecutablePath()
    {
        if (OperatingSystem.IsWindows()) return "powershell.exe";
        else if (OperatingSystem.IsLinux()) return "/bin/bash";
        else if (OperatingSystem.IsMacOS()) return "/bin/sh";
        else throw new NotSupportedException();
    }

    /// <summary>
    /// Event handler for receiving output data from the server process.
    /// </summary>
    /// <param name="sender">The sender of the event.</param>
    /// <param name="e">Data received event arguments.</param>
    private void OnOutputDataReceived(object sender, DataReceivedEventArgs e)
    {
        Url ??= DetermineDevServerUrl(e.Data);

        if (Url is not null && sender is Process process)
        {
            process.CancelOutputRead();
        }
    }

    private void StartProcess()
    {
        var projectRootPath = Path.GetFullPath(Path.Combine(environment.ContentRootPath, "..\\..\\..\\"));

        var startInfo = new ProcessStartInfo
        {
            WorkingDirectory = Path.Combine(projectRootPath, options.UserInterfacePath),
            FileName = options.TerminalExecutablePath ?? GetExecutablePath(),
            Arguments = options.StartCommand,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
        };

        var process = Process.Start(startInfo);
        process.BeginOutputReadLine();

        process.OutputDataReceived += OnOutputDataReceived;
        lifetime.ApplicationStopping.Register(() =>
        {
            process.Kill(true);
            process.WaitForExit(5000);
        });
    }
}