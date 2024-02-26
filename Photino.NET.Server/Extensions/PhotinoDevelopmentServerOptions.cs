namespace Photino.NET.Extensions;

public class PhotinoDevelopmentServerOptions
{
    public string IndexFile { get; set; }
    public string StartCommand { get; set; }
    public string TerminalExecutablePath { get; set; }
    public string UserInterfacePath { get; set; }
    public TimeSpan WaitUntilReadyTimeout { get; set; }
}