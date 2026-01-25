using Mindspace.Data;
using Mindspace.Data.Entities;

namespace Mindspace.Services;

public class PinLockService : IPinLockService
{
    public bool IsUnlocked { get; private set; } = false;

    public event Action? OnLockStateChanged;

    public Task LockAsync()
    {
        IsUnlocked = false;
        OnLockStateChanged?.Invoke();
        return Task.CompletedTask;
    }

    public async Task<bool> HasPinAsync(int userId)
    {
        var db = await AppDatabase.GetDatabaseAsync();
        var u = await db.Table<UserEntity>().Where(x => x.UserID == userId).FirstOrDefaultAsync();
        return u != null && !string.IsNullOrWhiteSpace(u.PinHash);
    }

    public async Task<(bool Success, string Message)> SetPinAsync(int userId, string pin, string confirmPin)
    {
        if (pin != confirmPin) return (false, "PINs do not match.");
        if (!IsValidPin(pin)) return (false, "PIN must be exactly 4 digits.");

        var db = await AppDatabase.GetDatabaseAsync();
        var u = await db.Table<UserEntity>().Where(x => x.UserID == userId).FirstOrDefaultAsync();
        if (u == null) return (false, "User not found.");

        u.PinHash = Security.HashPassword(pin); // reuse your hashing
        await db.UpdateAsync(u);

        return (true, "PIN created successfully.");
    }

    public async Task<(bool Success, string Message)> UnlockAsync(int userId, string pin)
    {
        if (!IsValidPin(pin)) return (false, "PIN must be exactly 4 digits.");

        var db = await AppDatabase.GetDatabaseAsync();
        var u = await db.Table<UserEntity>().Where(x => x.UserID == userId).FirstOrDefaultAsync();
        if (u == null) return (false, "User not found.");

        if (string.IsNullOrWhiteSpace(u.PinHash))
            return (false, "PIN not set. Please create a PIN first.");

        var hash = Security.HashPassword(pin);
        if (!string.Equals(hash, u.PinHash, StringComparison.Ordinal))
            return (false, "Incorrect PIN.");

        IsUnlocked = true;
        OnLockStateChanged?.Invoke();
        return (true, "Unlocked.");
    }

    private static bool IsValidPin(string pin)
        => !string.IsNullOrWhiteSpace(pin) && pin.Length == 4 && pin.All(char.IsDigit);
}
