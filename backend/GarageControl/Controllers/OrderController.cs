using GarageControl.Core.Contracts;
using GarageControl.Core.ViewModels;
using GarageControl.Core.ViewModels.Orders;
using GarageControl.Core.ViewModels.Jobs;
using GarageControl.Core.ViewModels.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GarageControl.Core.Attributes;

namespace GarageControl.Controllers
{
    [Authorize]
    [RequireAccess("Orders")]
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly IPDFGeneratorService _pdfGeneratorService;

        public OrderController(IOrderService orderService, IPDFGeneratorService pdfGeneratorService)
        {
            _orderService = orderService;
            _pdfGeneratorService = pdfGeneratorService;
        }

        private string GetWorkshopId()
        {
            return User.FindFirst("WorkshopId")?.Value!;
        }

        private string GetUserId()
        {
            return User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value!;
        }

        [HttpGet]
        public async Task<IActionResult> GetOrders()
        {
            try
            {
                var orders = await _orderService.GetOrdersAsync(GetWorkshopId());
                return Ok(orders);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("active")]
        public async Task<IActionResult> GetActiveOrders()
        {
            try
            {
                var orders = await _orderService.GetOrdersAsync(GetWorkshopId(), isDone: false);
                return Ok(orders);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("completed")]
        public async Task<IActionResult> GetCompletedOrders()
        {
            try
            {
                var orders = await _orderService.GetOrdersAsync(GetWorkshopId(), isDone: true);
                return Ok(orders);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderVM model)
        {
            try
            {
                var result = await _orderService.CreateOrderAsync(GetUserId(), GetWorkshopId(), model);
                if (result is MethodResponseVM resp && !resp.Success)
                {
                    return BadRequest(new { message = resp.Message });
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{orderId}/invoice")]
        public async Task<IActionResult> GetInvoicePdf(string orderId)
        {
            var order = await _orderService.GetOrderInvoiceByIdAsync(orderId);
            if (order == null)
                return NotFound("Order not found.");

            var pdfBytes = await _pdfGeneratorService.GenerateInvoicePdfAsync(order);

            return File(pdfBytes, "application/pdf", $"Invoice_{orderId}.pdf");
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderById(string id)
        {
            try
            {
                var order = await _orderService.GetOrderByIdAsync(id, GetWorkshopId());
                if (order == null) return NotFound();
                return Ok(order);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateOrder(string id, [FromBody] UpdateOrderVM model)
        {
            try
            {
                var result = await _orderService.UpdateOrderAsync(GetUserId(), id, GetWorkshopId(), model);
                if (result is MethodResponseVM resp && !resp.Success)
                {
                    return BadRequest(new { message = resp.Message });
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(string id)
        {
            try
            {
                var result = await _orderService.DeleteOrderAsync(GetUserId(), id, GetWorkshopId());
                if (!result.Success)
                {
                    return BadRequest(new { message = result.Message });
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
