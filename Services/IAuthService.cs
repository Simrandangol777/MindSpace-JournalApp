// Services/IAuthService.cs
using Mindspace.Models;
namespace Mindspace.Services;

public interface IAuthService
{
    Task<(bool Success, string Message, User? User)> LoginAsync(LoginRequest request);
    Task<(bool Success, string Message)> RegisterAsync(RegisterRequest request);
    void Logout();
    User? GetCurrentUser();
    bool IsAuthenticated();
    Task<(bool Success, string Message)> ChangePasswordAsync(string currentPassword, string newPassword);
    Task TryRestoreSessionAsync();
}