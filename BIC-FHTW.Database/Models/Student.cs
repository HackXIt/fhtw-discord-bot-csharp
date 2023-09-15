using System.ComponentModel.DataAnnotations;

namespace BIC_FHTW.Database.Models;

public class Student
{
    // ReSharper disable once InconsistentNaming
    [Key]
    public string UID { get; set; } // Primary key
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
    
    public DiscordUser? DiscordUser { get; set; } // Navigation Property
    
    // Constructor that sets all properties
    public Student(string uID, string courseOfStudyShortname, string courseOfStudy, int year, int semester, char association, int group)
    {
        UID = uID;
        CourseOfStudyShortname = courseOfStudyShortname;
        CourseOfStudy = courseOfStudy;
        Year = year;
        Semester = semester;
        Association = association;
        Group = group;
    }
}