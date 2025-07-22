using Rinha.Domain.Entities;
using Rinha.Domain.Interfaces;
using Rinha.Infra.Persistence;

namespace Rinha.Infra.Repositories
{
  public class PaymentRepository : IPaymentRepository
  {
    private readonly RinhaDbContext _context;

    public PaymentRepository(RinhaDbContext context)
    {
      _context = context;
    }

    public Task AddAsync(Payment payment)
    {
      _context.Payments.Add(payment);
      return _context.SaveChangesAsync();
    }
    public Task<IEnumerable<Payment>> FindAsync(DateTime? from, DateTime? to)
    {
      var query = _context.Payments.AsQueryable();

      if (from.HasValue)
      {
        query = query.Where(p => p.RequestedAt >= from.Value);
      }

      if (to.HasValue)
      {
        query = query.Where(p => p.RequestedAt <= to.Value);
      }

      return Task.FromResult(query.AsEnumerable());
    }

    public Task ClearAllAsync()
    {
      _context.Payments.RemoveRange(_context.Payments);
      return _context.SaveChangesAsync();
    }

  }
}
