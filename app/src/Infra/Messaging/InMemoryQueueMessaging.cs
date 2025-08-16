using System.Threading.Channels;
using Rinha.Application.DTOs;
using Rinha.Application.Interfaces;

namespace Rinha.Infra.Messaging
{
  public class InMemoryQueueMessaging : IMessagePublisher
  {
    private readonly Channel<PaymentMessage> _channel = Channel.CreateUnbounded<PaymentMessage>();
    public ChannelReader<PaymentMessage> Reader => _channel.Reader;
    public void PublishAsync(PaymentMessage message)
    {
      _channel.Writer.WriteAsync(message);
    }
  }
}
