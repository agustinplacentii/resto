using Microsoft.AspNetCore.Mvc;
using Restaurant.Api.Dtos;
using Restaurant.Api.Models;
using Restaurant.Api.Services;

namespace Restaurant.Api.Controllers;

[ApiController]
[Route("api/orders")]
public class OrdersController(IOrderService orders, IInvoiceService invoices) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<OrderDto>>> GetOrders()
    {
        return Ok(await orders.GetOrdersAsync());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<OrderDto>> GetOrder(int id)
    {
        var order = await orders.GetOrderAsync(id);
        return order is null ? NotFound() : Ok(order);
    }

    [HttpPost]
    public async Task<ActionResult<OrderDto>> CreateOrder(CreateOrderRequest request)
    {
        try
        {
            var order = await orders.CreateOrderAsync(request);
            return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPatch("{id:int}/status")]
    public async Task<ActionResult<OrderDto>> UpdateStatus(int id, UpdateOrderStatusRequest request)
    {
        var order = await orders.UpdateStatusAsync(id, request.Status);
        return order is null ? NotFound() : Ok(order);
    }

    [HttpGet("{id:int}/invoice.pdf")]
    public async Task<IActionResult> DownloadInvoice(int id)
    {
        var order = await orders.GetOrderEntityAsync(id);
        if (order is null)
        {
            return NotFound();
        }

        var pdf = invoices.BuildInvoicePdf(order);
        return File(pdf, "application/pdf", $"factura-pedido-{id}.pdf");
    }
}
