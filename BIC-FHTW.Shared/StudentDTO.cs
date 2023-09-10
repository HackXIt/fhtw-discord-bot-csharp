using System.Net.Mail;

namespace BIC_FHTW.Shared;

public class StudentDTO
{
    public string UID => MailAddress.User;
    public MailAddress MailAddress { get; set; }
}