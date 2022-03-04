namespace Pioneer.Server.Configuration;

internal class Config
{
    public bool IsDebug { get; set; } = false;
    
    public bool IsLocal { get; set; } = true;

    public int Port { get; set; } = 2077;

    public string Password { get; set; } = "Change Me!";
}