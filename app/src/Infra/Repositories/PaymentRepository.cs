using System.Collections.Concurrent;
using Rinha.Domain.Entities;
using Rinha.Domain.Enum;
using Rinha.Domain.Interfaces;
using Rinha.Infra.Persistence;

namespace Rinha.Infra.Repositories
{
  public class PaymentRepository : IPaymentRepository
  {
    private readonly ConcurrentQueue<Payment> _payments = new ConcurrentQueue<Payment>();

    public Task AddAsync(Payment payment)
    {
      _payments.Enqueue(payment);
      return Task.CompletedTask;
    }
    public Task<PaymentsSummary> GetPaymentsSummaryAsync(DateTime? from, DateTime? to)
    {
      List<Payment> filteredPayments = _payments
        .AsParallel()
        .Where(p => (!from.HasValue || p.RequestedAt >= from.Value) && (!to.HasValue || p.RequestedAt <= to.Value))
        .ToList();

      var defaultPayments = filteredPayments.Where(p => p.Processor == PaymentProcessor.Default);
      var fallbackPayments = filteredPayments.Where(p => p.Processor == PaymentProcessor.Fallback);

      PaymentsSummary summary = new PaymentsSummary
      {
        Default =
        {
          TotalRequests = defaultPayments.Count(),
          TotalAmount = defaultPayments.Sum(p => p.GetAmount())
        },
        Fallback =
        {
          TotalRequests = fallbackPayments.Count(),
          TotalAmount = fallbackPayments.Sum(p => p.GetAmount())
        }
      };

      return Task.FromResult(summary);
    }

    public Task ClearAllAsync()
    {
      while (_payments.TryDequeue(out _)) { }
      return Task.CompletedTask;
    }

  }
}
