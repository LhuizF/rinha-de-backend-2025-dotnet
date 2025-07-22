
namespace Rinha.Application.DTOs
{
  public record PaymentsSummaryDTO(
    SummaryDetails Default,
    SummaryDetails Fallback
  );

  public record SummaryDetails(
    int TotalRequests,
    decimal TotalAmount
  );
}
