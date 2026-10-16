namespace MetaCompta.Application.Options;

public class EmailOptions
{
    public const string SectionName = "Email";

    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 25;
    public bool EnableSsl { get; set; }
    public string FromAddress { get; set; } = "noreply@metacompta.ma";
    public string FromName { get; set; } = "MetaCompta";
    public string? UserName { get; set; }
    /// <summary>Alias for UserName (binds "Username" from config).</summary>
    public string? Username
    {
        get => UserName;
        set => UserName = value;
    }
    public string? Password { get; set; }
}
