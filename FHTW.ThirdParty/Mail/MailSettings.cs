namespace FHTW.ThirdParty.Mail;

public class MailSettings
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; }
    public string Account { get; set; } = Environment.GetEnvironmentVariable("MAILSETTINGS_ACCOUNT") ?? string.Empty;
    //public string Password { get; set; }
    public string AppPassword { get; set; } = Environment.GetEnvironmentVariable("MAILLSETTINGS_PASSWORD") ?? string.Empty;
}