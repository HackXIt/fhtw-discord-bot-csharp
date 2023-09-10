using System.Threading.Tasks;
using BIC_FHTW.Database.DatabaseContexts;
using BIC_FHTW.Database.Models;
using BIC_FHTW.Shared;
using Microsoft.EntityFrameworkCore;

namespace BIC_FHTW.Database.Services;

public class UserRepositoryManager
{
    private readonly ApplicationContext _context;

    public UserRepositoryManager(ApplicationContext context)
    {
        _context = context;
    }

    public async Task<DiscordUser> AddUserAsync(ulong discordUserId, string activationToken)
    {
        var user = new DiscordUser(discordUserId, activationToken, DiscordUser.UserStatus.Pending);

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return user;
    }
    
    public async Task<DiscordUser> AddStudentAndActivateUserAsync(ulong discordUserId, Student newStudentData)
    {
        // Find the existing DiscordUser by email
        var discordUser = await _context.Users.SingleOrDefaultAsync(u 
            => u.DiscordUserId == discordUserId);
        if (discordUser == null)
        {
            throw new UserNotFoundException($"No user found with Discord ID {discordUserId}");
        }

        // Link the provided Student object to the DiscordUser
        discordUser.Student = newStudentData;
        discordUser.StudentMail = newStudentData.UID;

        // Set DiscordUser status to Active
        discordUser.Status = DiscordUser.UserStatus.Active;

        // Add the new Student to the DbContext
        _context.Students.Add(newStudentData);

        // Update the DiscordUser in the DbContext
        _context.Users.Update(discordUser);

        // Save all changes
        await _context.SaveChangesAsync();

        return discordUser;
    }
    
    public async Task<bool> RemoveStudentAndDeactivateUserAsync(ulong discordUserId)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Find the existing DiscordUser by DiscordUserId
            var discordUser = await _context.Users.Include(d => d.Student)
                .SingleOrDefaultAsync(u => u.DiscordUserId == discordUserId);
        
            if (discordUser == null)
            {
                throw new UserNotFoundException($"No user found with Discord ID {discordUserId}");
            }

            if (discordUser.Student == null)
            {
                // User has no student linked, so there's nothing to remove
                return true;
            }
        
            // Set DiscordUser status to Inactive
            discordUser.Status = DiscordUser.UserStatus.Inactive;

            // Remove the Student if it exists
            if (discordUser.Student != null)
            {
                _context.Students.Remove(discordUser.Student);
            }

            // Unlink the Student from the DiscordUser
            discordUser.Student = null;
            discordUser.StudentMail = null;

            // Save all changes
            await _context.SaveChangesAsync();
        
            await transaction.CommitAsync();
        
            return true; // Successfully removed Student and deactivated DiscordUser
        }
        catch
        {
            await transaction.RollbackAsync();
            throw; // Rethrow the caught exception
        }
    }
    
    public async Task<DiscordUser> ResetUserWithNewTokenAsync(ulong discordUserId, string newActivationToken)
    {
        // Find the existing DiscordUser by DiscordUserId
        var discordUser = await _context.Users.SingleOrDefaultAsync(u => u.DiscordUserId == discordUserId);
        if (discordUser == null)
        {
            throw new UserNotFoundException($"No user found with Discord ID {discordUserId}");
        }

        // Set the DiscordUser status to Pending
        discordUser.Status = DiscordUser.UserStatus.Pending;

        // Update the activation token
        discordUser.ActivationToken = newActivationToken;

        // Update the DiscordUser in the DbContext
        _context.Users.Update(discordUser);

        // Save all changes
        await _context.SaveChangesAsync();

        return discordUser;
    }

    public async Task<DiscordUser> GetUserByDiscordIdAsync(ulong discordUserId)
    {
        // Find the existing DiscordUser by DiscordUserId
        var discordUser = await _context.Users.SingleOrDefaultAsync(u => u.DiscordUserId == discordUserId);
        if (discordUser == null)
        {
            throw new UserNotFoundException($"No user found with Discord ID {discordUserId}");
        }
        return discordUser;
    }

    public async Task<DiscordUser?> GetDiscordUserByStudentUidAsync(string studentUid)
    {
        return await _context.Users
            .Include(u => u.Student)
            .SingleOrDefaultAsync(u => u.StudentMail == studentUid);
    }

    public async Task<DiscordUser> GetUserByActivationTokenAsync(string activationToken)
    {
        var user = await _context.Users.SingleOrDefaultAsync(u => u.ActivationToken == activationToken);
        if (user == null)
        {
            throw new UserNotFoundException($"No user found with activation token {activationToken}");
        }
        return user;
    }

    public async Task UpdateUserStatusAsync(DiscordUser discordUser, DiscordUser.UserStatus newStatus)
    {
        discordUser.Status = newStatus;
        _context.Users.Update(discordUser);
        await _context.SaveChangesAsync();
    }
    
    public async Task<DiscordUser> LinkStudentToDiscordUserAsync(string? studentUid, ulong discordUserId)
    {
        // Find DiscordUser and Student by their respective IDs
        var discordUser = await _context.Users.FindAsync(discordUserId);
        if (discordUser == null)
        {
            throw new UserNotFoundException($"No user found with Discord ID {discordUserId}");
        }
        var student = await _context.Students.FindAsync(studentUid);
        if (student == null)
        {
            throw new UserNotFoundException($"No student found with UID {studentUid}");
        }

        // Link them together
        discordUser.StudentMail = studentUid;
        discordUser.Student = student;

        // Update and save changes
        _context.Users.Update(discordUser);
        await _context.SaveChangesAsync();

        return discordUser;
    }
    
    public async Task<Student?> GetStudentByUidAsync(string studentUid)
    {
        return await _context.Students.SingleOrDefaultAsync(s => s.UID == studentUid);
    }

    public async Task<Student?> GetStudentByDiscordIdAsync(ulong discordUserId)
    {
        // Find DiscordUser and include the Student entity
        var discordUser = await _context.Users
            .Include(u => u.Student)
            .SingleOrDefaultAsync(u => u.DiscordUserId == discordUserId);

        return discordUser?.Student; // This can still be null if the DiscordUser has no linked Student
    }

}