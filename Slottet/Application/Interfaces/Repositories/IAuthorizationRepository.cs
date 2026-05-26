using Domain.Entities;

namespace Application.Interfaces.Repositories;

public interface IAuthorizationRepository
{
    Task<IEnumerable<Authorization>> GetAllAsync();
    Task<bool> ExistsAsync(int id);
}
