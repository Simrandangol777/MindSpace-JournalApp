namespace Mindspace.Services;

public interface IPinLockService
{
    bool IsUnlocked { get; }
    event Action? OnLockStateChanged;
    Task LockAsync();
    Task<bool> HasPinAsync(int userId);
    Task<(bool Success, string Message)> SetPinAsync(int userId, string pin, string confirmPin);
    Task<(bool Success, string Message)> UnlockAsync(int userId, string pin);
}