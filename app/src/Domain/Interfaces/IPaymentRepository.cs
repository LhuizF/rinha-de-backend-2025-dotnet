using Rinha.Domain.Entities;

namespace Rinha.Domain.Interfaces
{
    public interface IPaymentRepository
    {
        Task AddAsync(Payment payment);
        Task<IEnumerable<Payment>> FindAsync(DateTime? from, DateTime? to);
        Task ClearAllAsync();
    }
}
