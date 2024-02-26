using Microsoft.Extensions.Options;
using Photino.NET.Extensions;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Photino.NET.Server;

public partial class PhotinoDevelopmentServer(IHostApplicationLifetime lifetime, IWebHostEnvironment environment, IOptions<PhotinoDevelopmentServerOptions> options)
{
    private readonly PhotinoDevelopmentServerOptions options = options.Value;

    public Uri Url { get; set; }

    public Task StartAsync() => Task.Factory.StartNew(StartProcess, TaskCreationOptions.LongRunning);

    public bool WaitForStartup() => SpinWait.SpinUntil(() => Url is not null, options.WaitUntilReadyTimeout);

#if NET7_0_OR_GREATER

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

    private static string GetExecutablePath()
    {
        if (OperatingSystem.IsWindows()) return "powershell.exe";
        else if (OperatingSystem.IsLinux()) return "/bin/bash";
        else if (OperatingSystem.IsMacOS()) return "/bin/sh";
        else throw new NotSupportedException();
    }

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
            process.WaitForExit();
        });
    }
}