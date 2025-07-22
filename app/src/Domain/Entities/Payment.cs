using Rinha.Domain.Enum;

namespace Rinha.Domain.Entities
{
  public class Payment
  {
    public Guid CorrelationId { get; set; }
    public int AmountInCents { get; set; }
    public PaymentProcessor Processor { get; set; }
    public DateTime RequestedAt { get; set; }

    public Payment(Guid correlationId, decimal amount, DateTime requestedAt)
    {
      CorrelationId = correlationId;
      AmountInCents = (int)(amount * 100);
      RequestedAt = requestedAt;
    }

    public void SetProcessor(PaymentProcessor processor)
    {
      Processor = processor;
    }

    public decimal GetAmount()
    {
      return AmountInCents / 100m;
    }
  }
}
