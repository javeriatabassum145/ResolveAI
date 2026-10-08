using Microsoft.AspNetCore.Mvc;
using ResolveAI.Api.Services;

namespace ResolveAI.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PayPalController : ControllerBase
{
    private readonly PayPalService _payPalService;

    public PayPalController(PayPalService payPalService)
    {
        _payPalService = payPalService;
    }

    [HttpGet("test")]
    public async Task<IActionResult> Test()
    {
        try
        {
            var token =
                await _payPalService.GetAccessTokenAsync();

            return Ok(new
            {
                message = "PayPal connection successful!",
                tokenReceived = !string.IsNullOrEmpty(token)
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                message = "PayPal connection failed.",
                error = ex.Message
            });
        }
    }

    [HttpGet("orders/{orderId}")]
    public async Task<IActionResult> GetOrder(
        string orderId)
    {
        try
        {
            var order =
                await _payPalService.GetOrderAsync(orderId);

            return Ok(order);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                message = "Unable to retrieve PayPal order.",
                error = ex.Message
            });
        }
    }
}