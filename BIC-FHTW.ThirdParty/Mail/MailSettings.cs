namespace BIC_FHTW.ThirdParty;

public class MailSettings
{
    public MailSettings(string host, int port, string account, string password)
    {
        Host = host;
        Port = port;
        Account = account;
        Password = password;
    }

    public string Host { get; set; }
    public int Port { get; set; }
    public string Account { get; set; }
    public string Password { get; set; }
}