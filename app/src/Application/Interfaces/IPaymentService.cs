using Rinha.Application.DTOs;

namespace Rinha.Application.Interfaces
{
  public interface IPaymentService
  {
    Task AddPaymentToQueueAsync(Guid correlationId, decimal amount);
    Task ProcessPaymentAsync(PaymentMessage paymentMessage);
    Task<PaymentsSummaryDTO> GetPaymentsSummaryAsync(DateTime? from, DateTime? to);
    Task PurgePaymentsAsync();
  }
}
