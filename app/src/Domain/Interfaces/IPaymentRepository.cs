using Rinha.Domain.Entities;

namespace Rinha.Domain.Interfaces
{
    public interface IPaymentRepository
    {
        Task AddAsync(Payment payment);
        Task<PaymentsSummary> GetPaymentsSummaryAsync(DateTime? from, DateTime? to);
        Task ClearAllAsync();
    }
}
