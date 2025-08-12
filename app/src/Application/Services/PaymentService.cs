using AutoMapper;
using Rinha.Application.DTOs;
using Rinha.Application.Interfaces;
using Rinha.Domain.Entities;
using Rinha.Domain.Enum;
using Rinha.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Rinha.Application.Services
{
  public class PaymentService : IPaymentService
  {
    private readonly IPaymentRepository _paymentRepository;
    private readonly IPaymentProcessorClient _paymentProcessorClient;
    private readonly IMessagePublisher _messagePublisher;
    private readonly IMapper _mapper;
    private readonly ILogger<PaymentService> _logger;

    private const int RetryCount = 3;

    public PaymentService(IPaymentRepository paymentRepository, IPaymentProcessorClient paymentProcessorClient, IMessagePublisher messagePublisher, IMapper mapper, ILogger<PaymentService> logger)
    {
      _paymentRepository = paymentRepository;
      _paymentProcessorClient = paymentProcessorClient;
      _messagePublisher = messagePublisher;
      _mapper = mapper;
      _logger = logger;
    }

    public void AddPaymentToQueueAsync(Guid correlationId, decimal amount)
    {
      var paymentMessage = new PaymentMessage(correlationId, amount, DateTime.UtcNow);
      _messagePublisher.PublishAsync(paymentMessage);
    }

    public async Task ProcessPaymentAsync(PaymentMessage paymentMessage)
    {
      var payment = new Payment(
        paymentMessage.CorrelationId,
        paymentMessage.Amount,
        paymentMessage.RequestedAt
      );

      var isSuccess = await SendPaymentAsync(payment);

      if (isSuccess)
      {
        await _paymentRepository.AddAsync(payment);

        return;
      }
      _logger.LogWarning("Falha final no processamento do pagamento {CorrelationId}, {isSuccess}", payment.CorrelationId, isSuccess);

      AddPaymentToQueueAsync(payment.CorrelationId, payment.GetAmount());
    }

    private async Task<bool> SendPaymentAsync(Payment payment)
    {
      var request = new PaymentMessage(payment.CorrelationId, payment.GetAmount(), payment.RequestedAt);

      for (int i = 1; i <= RetryCount; i++)
      {
        if (await _paymentProcessorClient.ProcessPaymentDefault(request))
        {
          payment.SetProcessor(PaymentProcessor.Default);
          return true;
        }

        // await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, i)));
      }

      if (await _paymentProcessorClient.ProcessPaymentFallback(request))
      {
        payment.SetProcessor(PaymentProcessor.Fallback);
        return true;
      }

      return false;
    }

    public async Task<PaymentsSummaryDTO> GetPaymentsSummaryAsync(DateTime? from, DateTime? to)
    {
      var summary = await _paymentRepository.GetPaymentsSummaryAsync(from, to);

      return _mapper.Map<PaymentsSummaryDTO>(summary);
    }

    public async Task PurgePaymentsAsync()
    {
      await _paymentRepository.ClearAllAsync();
    }
  }
}
