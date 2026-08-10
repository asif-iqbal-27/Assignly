using ErrorOr;

namespace Assignly.Application.Features.Users;

public static class UserErrors
{
    public static Error NotFound(Guid id) => Error.NotFound(
        "User.NotFound",
        $"User with id '{id}' was not found.");

    public static Error CreateFailed(string description) => Error.Validation(
        "User.CreateFailed",
        description);
}
