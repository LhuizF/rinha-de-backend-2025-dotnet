using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Rinha.Application.DTOs;
using Rinha.Application.Interfaces;
using Rinha.Domain.Entities;

namespace Rinha.Infra.Gateway
{
  public class PaymentProcessorClient : IPaymentProcessorClient
  {
    private readonly IHttpClientFactory _httpClient;

    public PaymentProcessorClient(IHttpClientFactory httpClient)
    {
        _httpClient = httpClient;
    }
    public async Task<bool> ProcessPaymentDefault(Payment payment)
    {
      var client = _httpClient.CreateClient("DefaultProcessor");
      var response = await client.PostAsJsonAsync("/payments", payment);
      return response.IsSuccessStatusCode;
      // Console.WriteLine("ProcessPaymentDefault");
      // return true;
    }

    public async Task<bool> ProcessPaymentFallback(Payment payment)
    {
      var client = _httpClient.CreateClient("FallbackProcessor");
      var response = await client.PostAsJsonAsync("/payments", payment);
      return response.IsSuccessStatusCode;
      // Console.WriteLine("ProcessPaymentFallback");
      // return true;
    }
  }
}
