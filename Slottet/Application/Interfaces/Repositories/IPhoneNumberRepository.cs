using Domain.Entities;

namespace Application.Interfaces.Repositories;

public interface IPhoneNumberRepository
{
    Task<IEnumerable<PhoneNumber>> GetAllAsync();
    Task<PhoneNumber?> GetByIdAsync(int id);
    void Add(PhoneNumber phoneNumber);
    void Remove(PhoneNumber phoneNumber);
    Task<int> SaveChangesAsync();
}
