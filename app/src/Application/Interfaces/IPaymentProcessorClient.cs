using Rinha.Domain.Entities;

namespace Rinha.Application.Interfaces
{
  public interface IPaymentProcessorClient
  {
    Task<bool> ProcessPaymentDefault(Payment paymentRequest);
    Task<bool> ProcessPaymentFallback(Payment paymentRequest);
  }
}
