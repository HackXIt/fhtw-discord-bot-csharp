using System.Net.Mail;
using BIC_FHTW.Scraper.Scrapers.Userprofile;
using BIC_FHTW.Shared;

namespace BIC_FHTW.Database.Models;

public static class EntityFactory
{
    public static DiscordUserDTO? ConvertFromDiscordUser(DiscordUser? user)
    {
        if (user == null)
            return null;
        var userInfo = new DiscordUserDTO()
        {
            IsAuthenticated = user.Status == DiscordUser.UserStatus.Active,
            UserId = user.DiscordUserId,
            MailAddress = new MailAddress(user.ActivationMail),
            Token = user.ActivationToken
        };
        return userInfo;
    }

    public static StudentDTO? ConvertFromStudent(Student? student)
    {
        if (student == null)
            return null;
        var studentDto = new StudentDTO()
        {
            UID = student.UID
        };
        return studentDto;
    }

    public static Student? ConvertFromUserprofileScrapeResult(UserprofileScrapeResult scrapeResult)
    {
        return scrapeResult is 
            {Username:not null, 
                Email: not null, 
                Studiengang: not null, 
                Semester: not -1, 
                Verband: not null,
                Gruppe: not -1} 
            ? new Student(
                scrapeResult.Username, 
                ("b" + scrapeResult.Username[..1]).ToUpper(), // FIXME some hacky way of creating the shortname
                scrapeResult.Studiengang,
                2000 + int.Parse(scrapeResult.Username[2..3]), // FIXME again a little hacky but it's 01:00 again and this works for now
                scrapeResult.Semester,
                (char)scrapeResult.Verband,
                scrapeResult.Gruppe) 
            : null;
    }
}