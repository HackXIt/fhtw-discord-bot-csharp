// ReSharper disable InconsistentNaming
namespace FHTW.Shared;

public class StudentDTO
{
    public string UID { get; set; } = string.Empty;
    public string StudentYear { get; set; } = string.Empty;
    public int Semester { get; set; }
    public char Association { get; set; }
    public int Group { get; set; }
}