using Microsoft.AspNetCore.Mvc;
using ResolveAI.Api.Services;
using ResolveAI.Api.Models;

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

    [HttpPost("orders")]
    public async Task<IActionResult> CreateOrder(
        [FromBody] CreatePayPalOrderRequest input)
    {
        try
        {
            if (input.Amount <= 0 ||
                string.IsNullOrWhiteSpace(input.Currency) ||
                input.Currency.Length != 3)
            {
                return BadRequest(new
                {
                    message = "Provide a positive amount and a 3-letter currency code."
                });
            }

            var order = await _payPalService.CreateOrderAsync(
                input.Amount,
                input.Currency.ToUpperInvariant());

            return Ok(order);
        }
        catch (HttpRequestException ex)
        {
            return StatusCode(502, new
            {
                message = "PayPal order creation failed.",
                error = ex.Message
            });
        }
        catch (Exception)
        {
            return StatusCode(500, new
            {
                message = "An unexpected error occurred while creating the order."
            });
        }
    }


    [HttpPost("orders/{orderId}/capture")]
    public async Task<IActionResult> CaptureOrder(string orderId)
    {
        try
        {
            var result = await _payPalService.CaptureOrderAsync(orderId);
            return Ok(result);
        }
        catch (HttpRequestException ex)
        {
            return StatusCode(502, new
            {
                message = "PayPal capture failed.",
                error = ex.Message
            });
        }
        catch (Exception)
        {
            return StatusCode(500, new
            {
                message = "An unexpected error occurred during capture."
            });
        }
    }


    [HttpGet("captures/{captureId}")]
    public async Task<IActionResult> GetCapture(string captureId)
    {
        try
        {
            var capture =
                await _payPalService.GetCaptureAsync(captureId);

            return Ok(capture);
        }
        catch (HttpRequestException)
        {
            return StatusCode(502, new
            {
                message = "Unable to retrieve capture from PayPal."
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }


}