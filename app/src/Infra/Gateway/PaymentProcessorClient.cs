using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Rinha.Application.DTOs;
using Rinha.Application.Interfaces;

namespace Rinha.Infra.Gateway
{
  public class PaymentProcessorClient : IPaymentProcessorClient
  {
    private readonly IHttpClientFactory _httpClient;

    public PaymentProcessorClient(IHttpClientFactory httpClient)
    {
        _httpClient = httpClient;
    }
    public async Task<bool> ProcessPaymentDefault(PaymentMessage paymentMessage)
    {
      var client = _httpClient.CreateClient("DefaultProcessor");
      var response = await client.PostAsJsonAsync("/payments", paymentMessage);
      return response.IsSuccessStatusCode;
    }

    public async Task<bool> ProcessPaymentFallback(PaymentMessage paymentMessage)
    {
      var client = _httpClient.CreateClient("FallbackProcessor");
      var response = await client.PostAsJsonAsync("/payments", paymentMessage);
      return response.IsSuccessStatusCode;
    }
  }
}
