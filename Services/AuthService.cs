using Mindspace.Models;
using Mindspace.Data;
using Mindspace.Data.Entities;

namespace Mindspace.Services;

public class AuthService : IAuthService
{
    private User? _currentUser;
    private const string SessionKey = "mindspace_userid";

    public bool IsAuthenticated() => _currentUser != null;
    public User? GetCurrentUser() => _currentUser;

    public void Logout()
    {
        _currentUser = null;
        Preferences.Remove(SessionKey);
    }

    public async Task<(bool Success, string Message)> RegisterAsync(RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            return (false, "Email and password are required.");
        }

        if (!ValidationHelper.IsValidEmail(request.Email))
            return (false, "Please enter a valid email address (example: name@example.com).");

        if (!ValidationHelper.IsStrongPassword(request.Password))
            return (false, "Password must be at least 6 characters and include uppercase, lowercase, number, and special character.");

        if (request.Password != request.ConfirmPassword)
            return (false, "Passwords do not match.");

        if (!request.AgreeToTerms)
            return (false, "You must agree to the terms.");

        var db = await AppDatabase.GetDatabaseAsync();

        var email = request.Email.Trim().ToLowerInvariant();

        var existingUser = await db.Table<UserEntity>()
            .Where(u => u.Email == email)
            .FirstOrDefaultAsync();

        if (existingUser != null)
            return (false, "Email already registered.");

        var entity = new UserEntity
        {
            Email = email,
            PasswordHash = Security.HashPassword(request.Password),
            CreatedAt = DateTime.Now
        };

        await db.InsertAsync(entity);

        Preferences.Set(SessionKey, entity.UserID);

        _currentUser = new User
        {
            Id = entity.UserID,
            Email = entity.Email,
            CreatedAt = entity.CreatedAt
        };

        return (true, "Registration successful.");
    }

    public async Task<(bool Success, string Message, User? User)> LoginAsync(LoginRequest request)
    {
        var db = await AppDatabase.GetDatabaseAsync();

        var email = request.Email.Trim().ToLowerInvariant();
        var hash = Security.HashPassword(request.Password);

        var entity = await db.Table<UserEntity>()
            .Where(u => u.Email == email && u.PasswordHash == hash)
            .FirstOrDefaultAsync();

        if (entity == null)
            return (false, "Invalid email or password.", null);

        entity.LastLoginAt = DateTime.Now;
        await db.UpdateAsync(entity);

        _currentUser = new User
        {
            Id = entity.UserID,
            Email = entity.Email,
            CreatedAt = entity.CreatedAt
        };

        Preferences.Set(SessionKey, entity.UserID);

        return (true, "Login successful.", _currentUser);
    }

    public async Task<(bool Success, string Message)> ChangePasswordAsync(string currentPassword, string newPassword)
    {
        if (_currentUser == null)
            return (false, "You are not logged in.");

        if (string.IsNullOrWhiteSpace(currentPassword) || string.IsNullOrWhiteSpace(newPassword))
            return (false, "Current and new password are required.");

        if (!ValidationHelper.IsStrongPassword(newPassword))
            return (false, "New password must be at least 6 characters and include uppercase, lowercase, number, and special character.");

        var db = await AppDatabase.GetDatabaseAsync();

        var entity = await db.Table<UserEntity>()
            .Where(u => u.UserID == _currentUser.Id)
            .FirstOrDefaultAsync();

        if (entity == null)
            return (false, "User not found.");

        var currentHash = Security.HashPassword(currentPassword);
        if (!string.Equals(entity.PasswordHash, currentHash, StringComparison.Ordinal))
            return (false, "Current password is incorrect.");

        entity.PasswordHash = Security.HashPassword(newPassword);
        await db.UpdateAsync(entity);

        return (true, "Password updated successfully.");
    }

    public async Task TryRestoreSessionAsync()
    {
        var userId = Preferences.Get(SessionKey, 0);
        if (userId <= 0) return;

        var db = await AppDatabase.GetDatabaseAsync();
        var entity = await db.Table<UserEntity>()
            .Where(u => u.UserID == userId)
            .FirstOrDefaultAsync();

        if (entity == null)
        {
            Preferences.Remove(SessionKey);
            return;
        }

        _currentUser = new User
        {
            Id = entity.UserID,
            Email = entity.Email,
            CreatedAt = entity.CreatedAt
        };
    }
}
