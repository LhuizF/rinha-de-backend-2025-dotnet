namespace Rinha.Application.Interfaces
{
    public interface IMessagePublisher
    {
        Task PublishAsync<T>(T message);
    }
}
