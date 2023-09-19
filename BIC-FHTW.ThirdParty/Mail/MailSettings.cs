namespace BIC_FHTW.ThirdParty;

public class MailSettings
{
    public MailSettings(string host, int port, string account, string appPassword)
    {
        Host = host;
        Port = port;
        Account = account;
        AppPassword = appPassword;
    }

    public string Host { get; set; }
    public int Port { get; set; }
    public string Account { get; set; }
    //public string Password { get; set; }
    public string AppPassword { get; set; }
}