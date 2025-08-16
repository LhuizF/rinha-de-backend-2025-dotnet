using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Rinha.Application.DTOs;
using Rinha.Application.Interfaces;
using Rinha.Infra.Messaging;

namespace Rinha.Infra.Consumer
{
  public class PaymentMessageConsumer : BackgroundService
  {
    private readonly InMemoryQueueMessaging _queue;
    private readonly IServiceProvider _serviceProvider;
    private readonly SemaphoreSlim _semaphore;

    public PaymentMessageConsumer(InMemoryQueueMessaging queue, IServiceProvider serviceProvider, int maxConcurrentProcesses)
    {
      Console.WriteLine($"Max concurrent processes: {maxConcurrentProcesses}");
      _queue = queue;
      _serviceProvider = serviceProvider;
      _semaphore = new SemaphoreSlim(maxConcurrentProcesses, maxConcurrentProcesses);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
      await foreach (var message in _queue.Reader.ReadAllAsync(stoppingToken))
      {
        await _semaphore.WaitAsync(stoppingToken);

        _ = ProcessAndReleaseAsync(message, stoppingToken);
      }
    }

    private async Task ProcessAndReleaseAsync(PaymentMessage message, CancellationToken stoppingToken)
    {
      try
      {
        using var scope = _serviceProvider.CreateScope();
        var paymentService = scope.ServiceProvider.GetRequiredService<IPaymentService>();

        if(paymentService == null)
        {
          Console.WriteLine("PaymentService is null, cannot process message.");
          return;
        }
        await paymentService.ProcessPaymentAsync(message);
      }
      catch (Exception ex)
      {
        Console.WriteLine($"Error processing payment message: {ex.Message}");
      }
      finally
      {
        _semaphore.Release();
      }
}
  }
}
