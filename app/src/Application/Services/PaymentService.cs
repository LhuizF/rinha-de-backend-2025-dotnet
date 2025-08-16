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
      var paymentMessage = new PaymentMessage(correlationId, amount);
      _messagePublisher.PublishAsync(paymentMessage);
    }

    public async Task ProcessPaymentAsync(PaymentMessage paymentMessage)
    {

      var payment = await SendPaymentAsync(paymentMessage);

      if (payment != null)
      {
        await _paymentRepository.AddAsync(payment);

        return;
      }
      _logger.LogWarning("Falha final no processamento do pagamento {CorrelationId}", paymentMessage.CorrelationId);

      AddPaymentToQueueAsync(paymentMessage.CorrelationId, paymentMessage.Amount);
    }

    private async Task<Payment?> SendPaymentAsync(PaymentMessage paymentMessage)
    {
      var payment = new Payment(paymentMessage.CorrelationId, paymentMessage.Amount);

      if (await _paymentProcessorClient.ProcessPaymentDefault(payment))
      {
        payment.SetProcessor(PaymentProcessor.Default);
        return payment;
      }

      if (await _paymentProcessorClient.ProcessPaymentFallback(payment))
      {
        payment.SetProcessor(PaymentProcessor.Fallback);
        return payment;
      }

      return null;
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
