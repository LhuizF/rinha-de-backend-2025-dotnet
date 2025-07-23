namespace Rinha.Application.DTOs
{
  public record PaymentsSummaryDTO
  {
    public required SummaryDetailsDTO Default { get; init; }
    public required SummaryDetailsDTO Fallback { get; init; }

    public PaymentsSummaryDTO() { }
  }

  public record SummaryDetailsDTO
  {
    public required int TotalRequests { get; init; }
    public required decimal TotalAmount { get; init; }
    public SummaryDetailsDTO() { }
  }
}
