namespace Rinha.Domain.Entities
{
  public class PaymentsSummary
  {
    public SummaryDetails Default { get; set; } = new SummaryDetails();
    public SummaryDetails Fallback { get; set; } = new SummaryDetails();
    public PaymentsSummary() { }
  }

  public class SummaryDetails
  {
    public int TotalRequests { get; set; }
    public decimal TotalAmount { get; set; }
    public SummaryDetails() { }
  }
}
