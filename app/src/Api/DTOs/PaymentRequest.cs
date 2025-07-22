
namespace Rinha.Api.Dtos
{
  public record PaymentRequest(
    Guid CorrelationId,
    decimal Amount
  );
}
