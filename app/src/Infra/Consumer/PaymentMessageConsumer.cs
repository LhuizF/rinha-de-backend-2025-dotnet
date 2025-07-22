using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Rinha.Application.Interfaces;
using Rinha.Infra.Messaging;

namespace Rinha.Infra.Consumer
{
  public class PaymentMessageConsumer : BackgroundService
  {
    private readonly InMemoryQueueMessaging _queue;
    private readonly IServiceProvider _serviceProvider;

    public PaymentMessageConsumer(InMemoryQueueMessaging queue, IServiceProvider serviceProvider)
    {
      _queue = queue;
      _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {

      await foreach (var message in _queue.Reader.ReadAllAsync(stoppingToken))
      {
        using var scope = _serviceProvider.CreateScope();
        var paymentService = scope.ServiceProvider.GetRequiredService<IPaymentService>();

        Console.WriteLine($"Mensagem recebida! {message.CorrelationId}");
      }
    }
  }
}
