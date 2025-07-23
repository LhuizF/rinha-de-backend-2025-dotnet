using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Rinha.Api.Dtos;
using Rinha.Application.Interfaces;

namespace Rinha.Api.Controllers
{
  [ApiController]
  public class PaymentsController : ControllerBase
  {
    private readonly IPaymentService _paymentService;
    public PaymentsController(IPaymentService paymentService)
    {
      _paymentService = paymentService;
    }

    [HttpPost("/payments")]
    public IActionResult AddPayment([FromBody] PaymentRequest paymentRequest)
    {
      _paymentService.AddPaymentToQueueAsync(paymentRequest.CorrelationId, paymentRequest.Amount);

      return Ok(new { message = "Ok" });
    }

    [HttpGet("/payments-summary")]
    public async Task<IActionResult> PaymentsSummary([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
      var summary = await _paymentService.GetPaymentsSummaryAsync(from, to);

      return Ok(summary);
    }
  }
}
