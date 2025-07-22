namespace Rinha.Application.DTOs
{
    public record PaymentMessage(
        Guid CorrelationId,
        decimal Amount,
        DateTime RequestedAt
    );
}
