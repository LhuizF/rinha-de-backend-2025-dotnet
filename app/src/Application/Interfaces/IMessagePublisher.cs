using Rinha.Application.DTOs;

namespace Rinha.Application.Interfaces
{
    public interface IMessagePublisher
    {
        Task PublishAsync(PaymentMessage message);
    }
}
