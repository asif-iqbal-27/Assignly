using ErrorOr;
using Assignly.Domain.Entities;

namespace Assignly.Application.Interfaces;

public interface IUserService
{
    Task<ErrorOr<string>> GenerateTokenAsync(ApplicationUser user);
}
