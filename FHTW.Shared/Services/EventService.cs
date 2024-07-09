using System;

namespace FHTW.Shared.Services;

public class EventService
{
    public class StudentRegisteredEventArgs : EventArgs
    {
        public StudentDTO Student { get; set; } = null!;
        public ulong DiscordUserId { get; set; }
    }

    public event EventHandler<StudentRegisteredEventArgs>? StudentRegistered;

    public void RaiseStudentRegistered(StudentDTO student, ulong discordUserId)
    {
        StudentRegistered?.Invoke(this, new StudentRegisteredEventArgs { Student = student, DiscordUserId = discordUserId});
    }
}