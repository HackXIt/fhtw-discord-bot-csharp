using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net.Mail;

namespace BIC_FHTW.Database.Models;

public class Student
{
    [Key]
    public string EmailString { get; set; } // Primary key
    // ReSharper disable once InconsistentNaming
    public string UID => Email.User;
    public string StudentYear => $"{CourseOfStudyShortname}{Year}";
    [Required]
    public string CourseOfStudyShortname { get; set; } // z.B.: BIC
    
    [Required]
    public string CourseOfStudy { get; set; } // z.B.: Informations- und Kommunikationssysteme
    
    [Required]
    public int Year { get; set; } // Year of entry to FHTW
    
    [Required]
    public int Semester { get; set; }
    
    [Required]
    public char Association { get; set; } // Verband
    
    [Required]
    public int Group { get; set; } // Gruppe

    [NotMapped]
    public MailAddress Email
    {
        get => new(EmailString);
        set => EmailString = value.ToString();
    }
    
    public DiscordUser? DiscordUser { get; set; } // Navigation Property
    
    // Constructor that sets all properties
    public Student(string courseOfStudyShortname, string courseOfStudy, int year, int semester, char association, int group, string emailString)
    {
        CourseOfStudyShortname = courseOfStudyShortname;
        CourseOfStudy = courseOfStudy;
        Year = year;
        Semester = semester;
        Association = association;
        Group = group;
        EmailString = emailString;
    }
}