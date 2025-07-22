using Rinha.Application.DTOs;

namespace Rinha.Application.Interfaces
{
  public interface IPaymentProcessorClient
  {
    Task<bool> ProcessPaymentDefault(PaymentMessage paymentMessage);
    Task<bool> ProcessPaymentFallback(PaymentMessage paymentMessage);
  }
}
