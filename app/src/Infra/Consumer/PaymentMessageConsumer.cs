using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Rinha.Application.Interfaces;
using Rinha.Infra.Messaging;

namespace Rinha.Infra.Consumer
{
  public class PaymentMessageConsumer : BackgroundService
  {
    private readonly InMemoryQueueMessaging _queue;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PaymentMessageConsumer> _logger;
    private const int MaxConcurrentMessages = 20;
    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, MaxConcurrentMessages);

    public PaymentMessageConsumer(InMemoryQueueMessaging queue, IServiceProvider serviceProvider, ILogger<PaymentMessageConsumer> logger)
    {
      _queue = queue;
      _serviceProvider = serviceProvider;
      _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
      await foreach (var message in _queue.Reader.ReadAllAsync(stoppingToken))
      {
        await _semaphore.WaitAsync(stoppingToken);

        _ = Task.Run(async () =>
        {
          try
          {
            using var scope = _serviceProvider.CreateScope();
            var paymentService = scope.ServiceProvider.GetRequiredService<IPaymentService>();
            await paymentService.ProcessPaymentAsync(message);
          }
          catch (Exception ex)
          {
            _logger.LogError(ex, "Erro ao processar mensagem {CorrelationId}", message.CorrelationId);
          }
          finally
          {
            _semaphore.Release();
          }
        }, stoppingToken);
      }
    }
  }
}
