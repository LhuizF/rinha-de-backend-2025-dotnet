using Rinha.Application.DTOs;

namespace Rinha.Application.Interfaces
{
    public interface IMessagePublisher
    {
        void PublishAsync(PaymentMessage message);
    }
}
