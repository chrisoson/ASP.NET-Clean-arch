using Domain.Abstractions;
using Domain.Common.Interfaces;
using Domain.ValueObjects;

namespace Domain.Aggregates;

/// <summary>
/// Represents a user in the app.
///
/// This is an aggregate root.
/// </summary>
public sealed class User : AggregateRootBase
{
    /// <summary>
    /// Gets the user's username.
    /// </summary>
    public string Username { get; init; } = default!;

    /// <summary>
    /// Gets the user's email.
    /// </summary>
    public string Email { get; init; } = default!;
    
    /// <summary>
    /// Gets the BCrypt password hash passed through 12+ rounds
    /// </summary>
    public string PasswordHash { get; private set; } = default!;

    /// <summary>
    /// Gets or sets the profile picture URL.
    /// </summary>
    public string? ProfilePictureUrl { get; set; }
    
    /// <summary>
    /// Gets the user's wallet.
    /// </summary>
    public Wallet Wallet { get; init; } = default!;

    /// <summary>
    /// Gets the user's inventory.
    /// </summary>
    public Inventory Inventory { get; init; } = default!;

    /// <summary>
    /// Changes the user's password.
    /// </summary>
    public void SetPassword(string newPassword)
    {
        PasswordHash = BC.HashPassword(newPassword);
    }
}