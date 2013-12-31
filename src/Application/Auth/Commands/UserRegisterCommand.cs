using System.ComponentModel;
using Application.Abstractions;
using Domain.Aggregates;
using Domain.Events;
using Domain.ValueObjects;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Auth.Commands;

/// <summary>
/// User register command
/// </summary>
public sealed record UserRegisterCommand(string Username, string Email, string Password)
    : IRequest<bool>
{
    private User? _user;

    /// <summary>
    /// The created user
    /// </summary>
    public User CreateUser()
    {
        if (_user is not null)
            return _user;

        _user = new User
        {
            Username = Username,
            Email = Email,
            Wallet = new Wallet(),
            Inventory = [],
        };

        _user.SetPassword(Password);
        return _user;
    }
}

[EditorBrowsable(EditorBrowsableState.Never)]
internal sealed class UserRegisterValidator : AbstractValidator<UserRegisterCommand>
{
    public UserRegisterValidator(IAppDbContext dbContext)
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Username)
            .NotEmpty()
            .Matches(@"^[a-zA-Z0-9._]{3,32}$")
            .WithMessage(
                "Username must be between 3 and 32 characters long and contain only letters, underscores and dots.");

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();

        RuleSet("async", () =>
        {
            RuleFor(x => x.Email)
                .MustAsync(async (command, email, ct) =>
                {
                    var any = await dbContext.Set<User>()
                        .AnyAsync(u => u.Email == email, ct);

                    return !any;
                })
                .WithMessage("Email is already in use.");
        });

        RuleFor(x => x.Password)
            .NotEmpty()
            .Matches(@"^.{8,256}$")
            .WithMessage("Password must be at least 8 characters long");
    }
}

[EditorBrowsable(EditorBrowsableState.Never)]
internal sealed class UserRegisterHandler(IAppDbContext dbContext)
    : IRequestHandler<UserRegisterCommand, bool>
{
    public async Task<bool> Handle(UserRegisterCommand command, CancellationToken ct)
    {
        var user = command.CreateUser();
        user.AddDomainEvent(new UserCreatedEvent(user.Id));

        await dbContext
            .Set<User>()
            .AddAsync(user, ct);

        await dbContext.SaveChangesAsync(ct);

        return true;
    }
}