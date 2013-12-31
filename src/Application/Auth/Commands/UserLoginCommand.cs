using System.ComponentModel;
using Application.Abstractions;
using Domain.Aggregates;
using Domain.Events;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Auth.Commands;

public sealed record UserLoginCommand(string Username, string Password, bool RememberMe = false) : IRequest<User?>
{
    public string Username { get; set; } = Username;

    public string Password { get; set; } = Password;

    public bool RememberMe { get; set; } = RememberMe;
}
// gio

[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class UserLoginValidator : AbstractValidator<UserLoginCommand>
{
    public UserLoginValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Username)
            .Length(3, 16)
            .Matches(@"^[a-zA-Z0-9\._]{3,32}$")
            .WithMessage("Username must contain only alphanumeric characters, underscores and dots.");

        RuleFor(x => x.Password)
            .Length(8, 64)
            .NotEmpty();
    }
}

[EditorBrowsable(EditorBrowsableState.Never)]
internal sealed class UserLoginHandler(IAppDbContext dbContext, IDateTimeProvider dateTimeProvider)
    : IRequestHandler<UserLoginCommand, User?>
{
    public async Task<User?> Handle(UserLoginCommand command, CancellationToken ct)
    {
        var user = await dbContext.Set<User>()
            .FirstOrDefaultAsync(x => x.Username == command.Username, ct);

        if (user is null)
            return null;

        if (!BC.Verify(command.Password, user.PasswordHash))
            return null;

        user.AddDomainEvent(new LoginEvent(user.Id, dateTimeProvider.UtcNow));

        return user;
    }
}