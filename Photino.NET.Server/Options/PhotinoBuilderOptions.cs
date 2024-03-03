namespace Photino.NET.Options;

/// <summary>
/// The PhotinoBuilderOptions class represents the configuration options for the Photino web application builder.
/// </summary>
public sealed class PhotinoBuilderOptions
{
    /// <summary>
    /// Gets or sets the command-line arguments for the Photino builder.
    /// </summary>
    public string[] Args { get; set; }

    /// <summary>
    /// Gets or sets the starting port for the web application.
    /// </summary>
    public int StartingPort { get; set; }

    /// <summary>
    /// Gets or sets the range of ports available for the web application.
    /// </summary>
    public int PortRange { get; set; }

    /// <summary>
    /// Gets or sets the path to the web root directory.
    /// </summary>
    public string WebRootPath { get; set; }

    /// <summary>
    /// Gets the default Photino builder options.
    /// </summary>
    public static PhotinoBuilderOptions Default => new()
    {
        StartingPort = 8000,
        PortRange = 100,
        WebRootPath = "wwwroot"
    };

    /// <summary>
    /// Implicitly converts PhotinoBuilderOptions to WebApplicationOptions for compatibility.
    /// </summary>
    /// <param name="options">The Photino builder options to convert.</param>
    /// <returns>The equivalent WebApplicationOptions.</returns>
    public static implicit operator WebApplicationOptions(PhotinoBuilderOptions options) => new()
    {
        Args = options.Args,
        WebRootPath = options.WebRootPath,
    };
}
