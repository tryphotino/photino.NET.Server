namespace Photino.NET.Options;

public sealed class PhotinoBuilderOptions
{
    public string[] Args { get; set; }
    public int StartingPort { get; set; }
    public int PortRange { get; set; }
    public string WebRootPath { get; set; }

    public static PhotinoBuilderOptions Default => new()
    {
        StartingPort = 8000,
        PortRange = 100,
        WebRootPath = "wwwroot"
    };

    public static implicit operator WebApplicationOptions(PhotinoBuilderOptions options) => new()
    {
        Args = options.Args,
        WebRootPath = options.WebRootPath,
    };
}
