namespace Photino.NET;

/// <summary>
/// The PhotinoApplication record represents a Photino web application along with its associated window.
/// </summary>
public record PhotinoApplication(WebApplication App, PhotinoWindow Window);