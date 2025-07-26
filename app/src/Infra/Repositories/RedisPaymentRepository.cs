using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using StackExchange.Redis;
using Rinha.Domain.Entities;
using Rinha.Domain.Enum;
using Rinha.Domain.Interfaces;

public class RedisPaymentRepository : IPaymentRepository
{
  private readonly IDatabase _redisDb;
  private readonly ILogger<RedisPaymentRepository> _logger;

  private const string PaymentIndexKey = "payments:index";
  private const string PaymentJsonKeyPrefix = "payment:json:";

  public RedisPaymentRepository(IConnectionMultiplexer redis, ILogger<RedisPaymentRepository> logger)
  {
    _redisDb = redis.GetDatabase();
    _logger = logger;
  }

  public async Task AddAsync(Payment payment)
  {
    var paymentKey = $"{PaymentJsonKeyPrefix}{payment.CorrelationId}";
    var paymentJson = JsonConvert.SerializeObject(payment);

    // 1. Armazena o JSON completo
    await _redisDb.StringSetAsync(paymentKey, paymentJson);
    _logger.LogDebug("Pagamento {CorrelationId} armazenado como JSON no Redis.", payment.CorrelationId);

    // 2. Indexa o pagamento por data
    await _redisDb.SortedSetAddAsync(PaymentIndexKey, payment.CorrelationId.ToString(), payment.RequestedAt.Ticks);

  }

  public async Task<PaymentsSummary> GetPaymentsSummaryAsync(DateTime? from, DateTime? to)
  {
    var fromTicks = from?.Ticks ?? DateTime.MinValue.Ticks;
    var toTicks = to?.Ticks ?? DateTime.MaxValue.Ticks;

    var correlationIds = await _redisDb.SortedSetRangeByScoreAsync(PaymentIndexKey, fromTicks, toTicks);

    if (!correlationIds.Any())
    {
      return new PaymentsSummary();
      }

    RedisKey[] paymentKeys = correlationIds.Select(redisValue => (RedisKey)$"{PaymentJsonKeyPrefix}{redisValue}").ToArray();

    var paymentJsons = await _redisDb.StringGetAsync(paymentKeys);

    var summary = new PaymentsSummary();
    var lockObject = new object();

    Parallel.ForEach(paymentJsons, paymentJson =>
    {
      if (!paymentJson.HasValue) return;

      Payment? payment;

      try
      {
        payment = JsonConvert.DeserializeObject<Payment>(paymentJson);
      }
      catch (Exception ex)
      {
        _logger.LogWarning(ex, "Erro ao desserializar um pagamento");
        return;
      }

      if (payment == null) return;

      lock (lockObject)
      {
        if (payment.Processor == PaymentProcessor.Default)
        {
          summary.Default.TotalAmount += payment.GetAmount();
          summary.Default.TotalRequests++;
        }
        else if (payment.Processor == PaymentProcessor.Fallback)
        {
          summary.Fallback.TotalAmount += payment.GetAmount();
          summary.Fallback.TotalRequests++;
        }
      }
    });

    return summary;
  }

  public async Task ClearAllAsync()
  {
    var server = _redisDb.Multiplexer.GetServer(_redisDb.Multiplexer.GetEndPoints().First());
    await server.FlushDatabaseAsync(_redisDb.Database);
  }
}
