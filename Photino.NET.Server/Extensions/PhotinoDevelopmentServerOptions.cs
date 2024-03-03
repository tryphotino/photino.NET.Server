namespace Photino.NET.Extensions;

/// <summary>
/// The PhotinoDevelopmentServerOptions class represents configuration options for the Photino development server.
/// </summary>
public class PhotinoDevelopmentServerOptions
{
    /// <summary>
    /// Gets or sets the name of the index file used by the development server.
    /// </summary>
    public string IndexFile { get; set; }

    /// <summary>
    /// Gets or sets the command to start the development server.
    /// </summary>
    public string StartCommand { get; set; }

    /// <summary>
    /// Gets or sets the path to the executable for the terminal used to start the server.
    /// </summary>
    public string TerminalExecutablePath { get; set; }

    /// <summary>
    /// Gets or sets the path to the user interface files used by the development server.
    /// </summary>
    public string UserInterfacePath { get; set; }

    /// <summary>
    /// Gets or sets the maximum time to wait for the server to become ready.
    /// </summary>
    public TimeSpan WaitUntilReadyTimeout { get; set; }
}
